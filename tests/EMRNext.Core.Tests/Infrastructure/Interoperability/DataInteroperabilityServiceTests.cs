using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Hl7.Fhir.Model;
using EMRNext.Core.Infrastructure.Interoperability;
using static EMRNext.Core.Infrastructure.Interoperability.DataInteroperabilityService;

namespace EMRNext.Core.Tests.Infrastructure.Interoperability
{
    public class DataInteroperabilityServiceTests
    {
        private readonly Mock<ILogger<DataInteroperabilityService>> _mockLogger;
        private readonly DataInteroperabilityService _interoperabilityService;

        public DataInteroperabilityServiceTests()
        {
            _mockLogger = new Mock<ILogger<DataInteroperabilityService>>();
            _interoperabilityService = new DataInteroperabilityService(_mockLogger.Object);
        }

        [Fact]
        public async Task TransformData_DirectStrategy_Succeeds()
        {
            // Arrange
            var sourceData = new Dictionary<string, object>
            {
                { "patientId", "12345" },
                { "firstName", "John" },
                { "lastName", "Doe" }
            };

            var mappingConfig = new DataMappingConfiguration
            {
                SourceSystem = "Legacy",
                TargetSystem = "FHIR",
                SourceType = DataSourceType.Custom,
                TargetType = DataSourceType.FHIR,
                Strategy = TransformationStrategy.Direct,
                FieldMappings = new Dictionary<string, string>
                {
                    { "patientId", "Identifier" },
                    { "firstName", "GivenName" },
                    { "lastName", "FamilyName" }
                },
                RequiredFields = new List<string> { "patientId" }
            };

            // Act
            var result = await _interoperabilityService.TransformDataAsync(sourceData, mappingConfig);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.TransformedData);
            var transformedDict = result.TransformedData as Dictionary<string, object>;
            Assert.Equal("12345", transformedDict["Identifier"]);
            Assert.Equal("John", transformedDict["GivenName"]);
            Assert.Equal("Doe", transformedDict["FamilyName"]);
        }

        [Fact]
        public async Task TransformData_NormalizedStrategy_Succeeds()
        {
            // Arrange
            var sourcePatient = new Patient
            {
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Given = new[] { "John" },
                        Family = "Doe"
                    }
                },
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        Value = "12345",
                        System = "MRN"
                    }
                },
                Gender = AdministrativeGender.Male
            };

            var mappingConfig = new DataMappingConfiguration
            {
                SourceSystem = "FHIR",
                TargetSystem = "EMRNext",
                SourceType = DataSourceType.FHIR,
                TargetType = DataSourceType.Custom,
                Strategy = TransformationStrategy.Normalized
            };

            // Act
            var result = await _interoperabilityService.TransformDataAsync(sourcePatient, mappingConfig);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.NotNull(result.TransformedData);
            var transformedPatient = result.TransformedData as Patient;
            Assert.NotNull(transformedPatient);
            Assert.Equal("DOE", transformedPatient.Name[0].Family);
            Assert.Equal(1, transformedPatient.Name[0].Given.Count);
            Assert.Equal("JOHN", transformedPatient.Name[0].Given[0]);
        }

        [Fact]
        public async Task TransformData_MissingRequiredField_Fails()
        {
            // Arrange
            var sourceData = new Dictionary<string, object>
            {
                { "firstName", "John" },
                { "lastName", "Doe" }
            };

            var mappingConfig = new DataMappingConfiguration
            {
                SourceSystem = "Legacy",
                TargetSystem = "FHIR",
                SourceType = DataSourceType.Custom,
                TargetType = DataSourceType.FHIR,
                Strategy = TransformationStrategy.Direct,
                FieldMappings = new Dictionary<string, string>
                {
                    { "firstName", "GivenName" },
                    { "lastName", "FamilyName" }
                },
                RequiredFields = new List<string> { "patientId" }
            };

            // Act
            var result = await _interoperabilityService.TransformDataAsync(sourceData, mappingConfig);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("Required field patientId is missing", result.ValidationErrors);
        }

        [Fact]
        public async Task NormalizeIdentifier_RemovesSpecialCharacters()
        {
            // Arrange
            var patient = new Patient
            {
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        Value = "MRN-12345 - ABC"
                    }
                }
            };

            var mappingConfig = new DataMappingConfiguration
            {
                Strategy = TransformationStrategy.Normalized
            };

            // Act
            var result = await _interoperabilityService.TransformDataAsync(patient, mappingConfig);

            // Assert
            Assert.True(result.IsSuccessful);
            var transformedPatient = result.TransformedData as Patient;
            Assert.Equal("MRN12345ABC", transformedPatient.Identifier[0].Value);
        }
    }
}
