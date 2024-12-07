using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using EMRNext.Core.Domain.Entities;

namespace EMRNext.Core.Services.FHIR
{
    public interface IFHIRMapper
    {
        // Patient Resource Mapping
        Patient MapToFHIRPatient(Domain.Entities.Patient patient);
        Domain.Entities.Patient MapFromFHIRPatient(Patient fhirPatient);

        // Encounter Resource Mapping
        Encounter MapToFHIREncounter(Domain.Entities.Encounter encounter);
        Domain.Entities.Encounter MapFromFHIREncounter(Encounter fhirEncounter);

        // Observation Resource Mapping (for Vitals)
        Observation MapToFHIRObservation(Vital vital);
        Vital MapFromFHIRObservation(Observation fhirObservation);

        // Clinical Note Resource Mapping
        DocumentReference MapToFHIRDocumentReference(ClinicalNote note);
        ClinicalNote MapFromFHIRDocumentReference(DocumentReference fhirDocument);

        // Medication Order Resource Mapping
        MedicationRequest MapToFHIRMedicationRequest(Prescription prescription);
        Prescription MapFromFHIRMedicationRequest(MedicationRequest fhirMedRequest);

        // Service Request Resource Mapping (for Orders)
        ServiceRequest MapToFHIRServiceRequest(Order order);
        Order MapFromFHIRServiceRequest(ServiceRequest fhirServiceRequest);

        // Diagnostic Report Resource Mapping (for Results)
        DiagnosticReport MapToFHIRDiagnosticReport(Result result);
        Result MapFromFHIRDiagnosticReport(DiagnosticReport fhirDiagReport);

        // Bundle Resource Creation
        Bundle CreateFHIRBundle(string type, IEnumerable<Resource> resources);
    }
}
