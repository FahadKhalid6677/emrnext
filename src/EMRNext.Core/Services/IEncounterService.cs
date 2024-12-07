using EMRNext.Core.Domain.Entities;
using System.Threading.Tasks;

namespace EMRNext.Core.Services
{
    public interface IEncounterService
    {
        Task<Encounter> GetByIdAsync(int id);
    }
}
