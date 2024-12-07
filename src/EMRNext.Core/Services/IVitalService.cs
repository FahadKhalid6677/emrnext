using EMRNext.Core.Domain.Entities;
using System.Threading.Tasks;

namespace EMRNext.Core.Services
{
    public interface IVitalService
    {
        Task<Vital> CreateAsync(Vital vital);
        Task<Vital> GetByIdAsync(int id);
        Task UpdateAsync(Vital vital);
    }
}
