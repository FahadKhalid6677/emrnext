using System.Threading.Tasks;

namespace EMRNext.Core.Services.Integration
{
    public interface IHL7Service
    {
        Task<string> ProcessHL7MessageAsync(string message);
        Task<string> GenerateHL7MessageAsync(string messageType, object data);
        Task<bool> ValidateHL7MessageAsync(string message);
        Task<string> TransformToFHIRAsync(string hl7Message);
        Task<string> TransformFromFHIRAsync(string fhirResource, string messageType);
    }
}
