namespace EMRNext.Core.Authorization
{
    /// <summary>
    /// Centralized policy constants for authorization
    /// </summary>
    public static class PolicyConstants
    {
        // Patient-related policies
        public const string ViewPatientRecord = "Patient.View";
        public const string EditPatientRecord = "Patient.Edit";
        public const string CreatePatientRecord = "Patient.Create";
        public const string DeletePatientRecord = "Patient.Delete";

        // Clinical policies
        public const string ViewClinicalDocuments = "Clinical.ViewDocuments";
        public const string EditClinicalDocuments = "Clinical.EditDocuments";
        public const string CreateClinicalDocuments = "Clinical.CreateDocuments";

        // Prescription policies
        public const string ViewPrescriptions = "Prescription.View";
        public const string CreatePrescription = "Prescription.Create";
        public const string ModifyPrescription = "Prescription.Modify";
        public const string CancelPrescription = "Prescription.Cancel";

        // Administrative policies
        public const string SystemAdministration = "System.Administration";
        public const string UserManagement = "User.Management";
        public const string AuditLogging = "System.AuditLogging";

        // Role-specific policies
        public const string PhysicianAccess = "Role.Physician";
        public const string NurseAccess = "Role.Nurse";
        public const string AdministratorAccess = "Role.Administrator";
        public const string ResearcherAccess = "Role.Researcher";
    }
}
