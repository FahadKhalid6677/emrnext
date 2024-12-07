using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EMRNext.IntegrationTests
{
    public class AuthenticationTests : BaseIntegrationTest
    {
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginModel = new 
            {
                Email = "test@emrnext.com",
                Password = "ValidTestPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginModel);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var tokenResponse = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
            Assert.NotNull(tokenResponse?.Token);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var loginModel = new 
            {
                Email = "invalid@emrnext.com",
                Password = "InvalidPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginModel);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private class AuthTokenResponse
        {
            public string Token { get; set; }
        }
    }
}
