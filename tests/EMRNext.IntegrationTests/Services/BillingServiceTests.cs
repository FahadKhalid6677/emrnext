using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FluentAssertions;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Exceptions;

namespace EMRNext.IntegrationTests.Services
{
    public class BillingServiceTests : IClassFixture<TestFixture>
    {
        private readonly IBillingService _billingService;
        private readonly IBillingRepository _billingRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILoggingService _loggingService;

        public BillingServiceTests(TestFixture fixture)
        {
            _billingService = fixture.ServiceProvider.GetRequiredService<IBillingService>();
            _billingRepository = fixture.ServiceProvider.GetRequiredService<IBillingRepository>();
            _patientRepository = fixture.ServiceProvider.GetRequiredService<IPatientRepository>();
            _loggingService = fixture.ServiceProvider.GetRequiredService<ILoggingService>();
        }

        [Fact]
        public async Task CreateClaim_WithValidData_ShouldCreateClaim()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var insurance = await CreateTestInsurance(patient.Id);
            var claim = new Claim
            {
                PatientId = patient.Id,
                ProviderId = 1,
                InsuranceId = insurance.Id,
                ServiceDate = DateTime.UtcNow.AddDays(-1),
                DiagnosisCodes = new List<string> { "J20.0" },
                ProcedureCodes = new List<string> { "99213" },
                ClaimAmount = 150.00m
            };

            // Act
            var result = await _billingService.CreateClaimAsync(claim);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Status.Should().Be(ClaimStatus.Pending);
            result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CreateClaim_WithInvalidInsurance_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var claim = new Claim
            {
                PatientId = patient.Id,
                ProviderId = 1,
                InsuranceId = -1,
                ServiceDate = DateTime.UtcNow.AddDays(-1),
                DiagnosisCodes = new List<string> { "J20.0" },
                ProcedureCodes = new List<string> { "99213" },
                ClaimAmount = 150.00m
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => 
                _billingService.CreateClaimAsync(claim));
        }

        [Fact]
        public async Task ProcessPayment_WithValidData_ShouldProcessPayment()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var account = await CreateTestAccount(patient.Id);
            var payment = new Payment
            {
                AccountId = account.Id,
                PaymentAmount = 100.00m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.CreditCard,
                TransactionId = Guid.NewGuid().ToString()
            };

            // Act
            var result = await _billingService.ProcessPaymentAsync(payment);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Status.Should().Be(PaymentStatus.Processed);
            result.ProcessedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task ProcessPayment_WithInactiveAccount_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var account = await CreateTestAccount(patient.Id);
            account.Status = AccountStatus.Inactive;
            await _billingRepository.UpdateAccountAsync(account.Id, account);

            var payment = new Payment
            {
                AccountId = account.Id,
                PaymentAmount = 100.00m,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = PaymentMethod.CreditCard,
                TransactionId = Guid.NewGuid().ToString()
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => 
                _billingService.ProcessPaymentAsync(payment));
        }

        [Fact]
        public async Task GenerateStatement_WithUnbilledCharges_ShouldGenerateStatement()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var account = await CreateTestAccount(patient.Id);
            await CreateTestCharges(patient.Id);

            // Act
            var result = await _billingService.GenerateStatementAsync(patient.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Status.Should().Be(StatementStatus.Generated);
            result.StatementDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.TotalAmount.Should().BeGreaterThan(0);
            result.Charges.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GenerateStatement_WithNoCharges_ShouldThrowBusinessRuleException()
        {
            // Arrange
            var patient = await CreateTestPatient();
            var account = await CreateTestAccount(patient.Id);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => 
                _billingService.GenerateStatementAsync(patient.Id));
        }

        // Helper methods
        private async Task<Patient> CreateTestPatient()
        {
            var patient = new Patient
            {
                FirstName = "Test",
                LastName = "Patient",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                Gender = "M"
            };
            return await _patientRepository.AddAsync(patient);
        }

        private async Task<Insurance> CreateTestInsurance(int patientId)
        {
            var insurance = new Insurance
            {
                PatientId = patientId,
                InsuranceProvider = "Test Insurance",
                PolicyNumber = "TEST123",
                GroupNumber = "GRP456",
                EffectiveDate = DateTime.UtcNow.AddMonths(-1),
                ExpirationDate = DateTime.UtcNow.AddYears(1),
                Status = InsuranceStatus.Active
            };
            return await _billingRepository.CreateInsuranceAsync(insurance);
        }

        private async Task<Account> CreateTestAccount(int patientId)
        {
            var account = new Account
            {
                PatientId = patientId,
                AccountType = AccountType.Patient,
                Status = AccountStatus.Active,
                BillingAddress = "123 Test St",
                PaymentTerms = "Net 30",
                CurrentBalance = 0
            };
            return await _billingRepository.CreateAccountAsync(account);
        }

        private async Task CreateTestCharges(int patientId)
        {
            var charges = new List<Charge>
            {
                new Charge
                {
                    PatientId = patientId,
                    ServiceDate = DateTime.UtcNow.AddDays(-5),
                    ProcedureCode = "99213",
                    Description = "Office Visit",
                    Amount = 150.00m,
                    Status = ChargeStatus.Pending
                },
                new Charge
                {
                    PatientId = patientId,
                    ServiceDate = DateTime.UtcNow.AddDays(-5),
                    ProcedureCode = "85025",
                    Description = "Blood Test",
                    Amount = 75.00m,
                    Status = ChargeStatus.Pending
                }
            };

            foreach (var charge in charges)
            {
                await _billingRepository.CreateChargeAsync(charge);
            }
        }
    }
}
