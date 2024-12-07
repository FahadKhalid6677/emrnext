using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Models;

namespace EMRNext.Core.Services.Patients
{
    public interface IPatientService
    {
        Task<Patient> RegisterPatientAsync(PatientRegistrationRequest request);
        Task<Patient> GetPatientAsync(string id);
        Task<Patient> UpdatePatientAsync(string id, PatientUpdateRequest request);
        Task<IEnumerable<Patient>> SearchPatientsAsync(PatientSearchRequest request);
        Task<bool> DeactivatePatientAsync(string id, string reason);
        Task<bool> MergePatientRecordsAsync(string sourceId, string targetId);
        Task<PatientDemographics> GetDemographicsAsync(string id);
        Task<IEnumerable<PatientInsurance>> GetInsuranceInfoAsync(string id);
    }
}
