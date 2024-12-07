using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface IValidationService
    {
        Task<bool> ValidateEmailAsync(string email);
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
        Task<bool> ValidateAddressAsync(string address);
        Task<bool> ValidateIdentifierAsync(string identifier, string type);
    }
}
