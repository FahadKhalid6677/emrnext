using System.Threading.Tasks;

namespace EMRNext.Core.Interfaces
{
    public interface ISecretManager
    {
        Task<string> GetSecretAsync(string secretName);
        Task SetSecretAsync(string secretName, string secretValue);
    }
}
