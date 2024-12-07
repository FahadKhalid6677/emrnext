using System;
using System.Threading.Tasks;
using Xunit;
using EMRNext.Infrastructure.Services.External;

namespace EMRNext.Infrastructure.Tests.Integration
{
    [Collection("Integration Tests")]
    public class DrugDatabaseServiceIntegrationTests : IntegrationTestBase
    {
        private readonly IDrugDatabaseService _drugDatabaseService;

        public DrugDatabaseServiceIntegrationTests()
        {
            _drugDatabaseService = GetService<IDrugDatabaseService>();
        }

        [Fact]
        public async Task GetDrugInfo_ValidNDC_ReturnsDrugInfo()
        {
            // Arrange
            var ndc = "0002-1433-80"; // Prozac 20mg capsule

            // Act
            var result = await _drugDatabaseService.GetDrugInfoAsync(ndc);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ndc, result.NDC);
            Assert.NotEmpty(result.Name);
            Assert.NotEmpty(result.Manufacturer);
        }

        [Fact]
        public async Task CheckInteractions_ValidDrugs_ReturnsInteractions()
        {
            // Arrange
            var ndcList = new[]
            {
                "0002-1433-80", // Prozac
                "00093-0058-01" // Warfarin
            };

            // Act
            var results = await _drugDatabaseService.CheckInteractionsAsync(ndcList);

            // Assert
            Assert.NotNull(results);
            var interactions = Assert.IsAssignableFrom<IEnumerable<DrugInteraction>>(results);
            Assert.NotEmpty(interactions);
        }

        [Fact]
        public async Task CheckAllergyInteractions_ValidData_ReturnsAllergies()
        {
            // Arrange
            var ndc = "0002-1433-80"; // Prozac
            var allergies = new[] { "Fluoxetine", "SSRIs" };

            // Act
            var results = await _drugDatabaseService.CheckAllergyInteractionsAsync(ndc, allergies);

            // Assert
            Assert.NotNull(results);
            var allergyInteractions = Assert.IsAssignableFrom<IEnumerable<DrugAllergy>>(results);
            Assert.NotEmpty(allergyInteractions);
        }

        [Fact]
        public async Task SearchDrugs_ValidQuery_ReturnsResults()
        {
            // Arrange
            var query = "metformin";
            var limit = 5;

            // Act
            var results = await _drugDatabaseService.SearchDrugsAsync(query, limit);

            // Assert
            Assert.NotNull(results);
            var drugs = Assert.IsAssignableFrom<IEnumerable<DrugInfo>>(results);
            Assert.True(drugs.Count() <= limit);
            Assert.Contains(drugs, d => d.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFormularyInfo_ValidData_ReturnsFormulary()
        {
            // Arrange
            var ndc = "0002-1433-80"; // Prozac
            var insurancePlanId = "TEST-PLAN-001";

            // Act
            var result = await _drugDatabaseService.GetFormularyInfoAsync(ndc, insurancePlanId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ndc, result.NDC);
            Assert.NotNull(result.Tier);
            Assert.NotNull(result.CopayAmount);
        }
    }
}
