using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;

namespace EMRNext.Tests.Security
{
    public class SecurityAuditTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public SecurityAuditTests(TestServerFixture fixture)
        {
            _server = fixture.Server;
            _client = fixture.Client;
        }

        [Fact]
        public async Task SecurityHeaders_ShouldBePresent_InAllResponses()
        {
            // Arrange
            var endpoints = new[]
            {
                "/api/patients",
                "/api/clinical/notes",
                "/api/prescriptions",
                "/api/billing/invoices"
            };

            foreach (var endpoint in endpoints)
            {
                // Act
                var response = await _client.GetAsync(endpoint);

                // Assert
                response.Headers.Should().ContainKey("X-Content-Type-Options")
                    .WhoseValue.Should().BeEquivalentTo("nosniff");
                response.Headers.Should().ContainKey("X-Frame-Options")
                    .WhoseValue.Should().BeEquivalentTo("DENY");
                response.Headers.Should().ContainKey("X-XSS-Protection")
                    .WhoseValue.Should().BeEquivalentTo("1; mode=block");
                response.Headers.Should().ContainKey("Content-Security-Policy");
                response.Headers.Should().ContainKey("Strict-Transport-Security");
            }
        }

        [Fact]
        public async Task RateLimiting_ShouldBlock_ExcessiveRequests()
        {
            // Arrange
            const string endpoint = "/api/patients";
            const int maxRequests = 100;
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            for (int i = 0; i < maxRequests + 10; i++)
            {
                tasks.Add(_client.GetAsync(endpoint));
            }
            var responses = await Task.WhenAll(tasks);

            // Assert
            responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task JWT_ShouldValidate_TokenClaims()
        {
            // Arrange
            var loginData = new
            {
                email = "test@example.com",
                password = "TestPass123!"
            };

            // Act
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginData);
            var token = (await loginResponse.Content.ReadAsAsync<JObject>())["token"].ToString();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role);
            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task CSRF_Protection_ShouldBeEnforced()
        {
            // Arrange
            var data = new { name = "Test Patient" };

            // Act - Without CSRF token
            var response = await _client.PostAsJsonAsync("/api/patients", data);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            // Act - With CSRF token
            var csrfToken = await GetCsrfTokenAsync();
            _client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrfToken);
            response = await _client.PostAsJsonAsync("/api/patients", data);

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task SQL_Injection_ShouldBeBlocked()
        {
            // Arrange
            var maliciousQueries = new[]
            {
                "'; DROP TABLE Patients; --",
                "' OR '1'='1",
                "'; SELECT * FROM Users; --"
            };

            foreach (var query in maliciousQueries)
            {
                // Act
                var response = await _client.GetAsync($"/api/patients/search?q={query}");

                // Assert
                response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotContain("SQL");
            }
        }

        [Fact]
        public async Task XSS_Prevention_ShouldBeEnforced()
        {
            // Arrange
            var xssPayloads = new[]
            {
                "<script>alert('xss')</script>",
                "javascript:alert('xss')",
                "<img src='x' onerror='alert(1)'>"
            };

            foreach (var payload in xssPayloads)
            {
                var data = new { notes = payload };

                // Act
                var response = await _client.PostAsJsonAsync("/api/clinical/notes", data);
                var content = await response.Content.ReadAsStringAsync();

                // Assert
                content.Should().NotContain("<script>");
                content.Should().NotContain("javascript:");
                content.Should().NotContain("onerror=");
            }
        }

        [Fact]
        public async Task FileUpload_ShouldValidate_FileTypes()
        {
            // Arrange
            var invalidFiles = new[]
            {
                ("test.exe", "application/x-msdownload"),
                ("test.php", "application/x-php"),
                ("test.jsp", "application/x-jsp")
            };

            foreach (var (filename, contentType) in invalidFiles)
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(new byte[] { 0x00 });
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                content.Add(fileContent, "file", filename);

                // Act
                var response = await _client.PostAsync("/api/documents/upload", content);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Authentication_ShouldPrevent_CommonAttacks()
        {
            // Test password brute force protection
            var attempts = 0;
            var loginData = new { email = "test@example.com", password = "wrong" };

            while (attempts++ < 10)
            {
                var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    break;
                }
            }

            attempts.Should().BeLessThan(10, "should block after multiple failed attempts");

            // Test password complexity
            var weakPasswords = new[]
            {
                "password",
                "123456",
                "qwerty"
            };

            foreach (var password in weakPasswords)
            {
                var registerData = new
                {
                    email = "test@example.com",
                    password = password
                };

                var response = await _client.PostAsJsonAsync("/api/auth/register", registerData);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        private async Task<string> GetCsrfTokenAsync()
        {
            var response = await _client.GetAsync("/api/auth/csrf-token");
            var token = await response.Content.ReadAsStringAsync();
            return token.Trim('"');
        }
    }
}
