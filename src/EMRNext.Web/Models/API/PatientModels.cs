using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMRNext.Web.Models.API
{
    public class PatientRegistrationRequest
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string SSN { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public string PreferredLanguage { get; set; }
        public string MaritalStatus { get; set; }
        public string EmploymentStatus { get; set; }
        public List<InsuranceInfo> Insurances { get; set; }
    }

    public class PatientUpdateRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public string PreferredLanguage { get; set; }
        public string MaritalStatus { get; set; }
        public string EmploymentStatus { get; set; }
    }

    public class InsuranceInfo
    {
        [Required]
        public string PayerId { get; set; }

        [Required]
        public string PolicyNumber { get; set; }

        public string GroupNumber { get; set; }
        public string SubscriberId { get; set; }
        public string SubscriberName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public int Priority { get; set; }
    }

    public class PatientResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public string PreferredLanguage { get; set; }
        public string MaritalStatus { get; set; }
        public string EmploymentStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public List<InsuranceResponse> Insurances { get; set; }
    }

    public class InsuranceResponse
    {
        public int Id { get; set; }
        public string PayerId { get; set; }
        public string PayerName { get; set; }
        public string PolicyNumber { get; set; }
        public string GroupNumber { get; set; }
        public string SubscriberId { get; set; }
        public string SubscriberName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? LastVerificationDate { get; set; }
    }

    public class PatientSearchRequest
    {
        public string SearchTerm { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class PatientSearchResponse
    {
        public List<PatientResponse> Patients { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PatientDocumentRequest
    {
        [Required]
        public string DocumentType { get; set; }

        [Required]
        public string DocumentContent { get; set; }

        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class PatientDocumentResponse
    {
        public int Id { get; set; }
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; }
    }
}
