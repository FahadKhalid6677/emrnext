using System;
using System.Linq;
using Hl7.Fhir.Model;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Domain.ValueObjects;

namespace EMRNext.Core.Interoperability.Fhir.Models
{
    /// <summary>
    /// FHIR Patient resource mapping
    /// </summary>
    public class FhirPatient : FhirResourceBase
    {
        public override string ResourceType => "Patient";

        /// <summary>
        /// Patient identifier
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Patient's name
        /// </summary>
        public HumanName Name { get; set; }

        /// <summary>
        /// Patient's contact information
        /// </summary>
        public ContactPoint Telecom { get; set; }

        /// <summary>
        /// Patient's gender
        /// </summary>
        public AdministrativeGender? Gender { get; set; }

        /// <summary>
        /// Patient's birthdate
        /// </summary>
        public Date BirthDate { get; set; }

        /// <summary>
        /// Patient's address
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Convert EMRNext Patient to FHIR Patient
        /// </summary>
        public override Resource ToFhirResource()
        {
            return new Patient
            {
                Id = Id,
                Identifier = Identifier != null 
                    ? new List<Identifier> 
                    { 
                        new Identifier(null, Identifier) 
                    } 
                    : null,
                Name = Name != null 
                    ? new List<HumanName> { Name } 
                    : null,
                Telecom = Telecom != null 
                    ? new List<ContactPoint> { Telecom } 
                    : null,
                Gender = Gender,
                BirthDate = BirthDate?.ToString(),
                Address = Address != null 
                    ? new List<Address> { Address } 
                    : null
            };
        }

        /// <summary>
        /// Create FhirPatient from EMRNext Patient
        /// </summary>
        public static FhirPatient FromDomainPatient(Patient patient)
        {
            return new FhirPatient
            {
                Id = patient.Id.ToString(),
                Identifier = patient.MedicalRecordNumber,
                Name = new HumanName()
                    .WithGiven(patient.FirstName)
                    .AndFamily(patient.LastName),
                Telecom = new ContactPoint
                {
                    System = ContactPoint.ContactPointSystem.Phone,
                    Value = patient.ContactNumber
                },
                Gender = patient.Gender switch
                {
                    "Male" => AdministrativeGender.Male,
                    "Female" => AdministrativeGender.Female,
                    "Other" => AdministrativeGender.Other,
                    _ => AdministrativeGender.Unknown
                },
                BirthDate = new Date(patient.DateOfBirth.Year, patient.DateOfBirth.Month, patient.DateOfBirth.Day),
                Address = new Address
                {
                    Line = new[] { patient.Address.Street },
                    City = patient.Address.City,
                    PostalCode = patient.Address.PostalCode,
                    Country = patient.Address.Country
                }
            };
        }

        /// <summary>
        /// Convert from FHIR Patient to EMRNext Patient
        /// </summary>
        public override void FromFhirResource(Resource resource)
        {
            if (resource is not Patient fhirPatient)
                throw new ArgumentException("Invalid resource type");

            Id = fhirPatient.Id;
            Identifier = fhirPatient.Identifier?.FirstOrDefault()?.Value;
            Name = fhirPatient.Name?.FirstOrDefault();
            Telecom = fhirPatient.Telecom?.FirstOrDefault();
            Gender = fhirPatient.Gender;
            BirthDate = fhirPatient.BirthDate != null 
                ? new Date(fhirPatient.BirthDate) 
                : null;
            Address = fhirPatient.Address?.FirstOrDefault();
        }

        /// <summary>
        /// Convert to EMRNext Patient domain model
        /// </summary>
        public Patient ToDomainPatient()
        {
            return new Patient
            {
                Id = Guid.Parse(Id),
                MedicalRecordNumber = Identifier,
                FirstName = Name?.Given?.FirstOrDefault(),
                LastName = Name?.Family,
                ContactNumber = Telecom?.Value,
                Gender = Gender switch
                {
                    AdministrativeGender.Male => "Male",
                    AdministrativeGender.Female => "Female",
                    AdministrativeGender.Other => "Other",
                    _ => "Unknown"
                },
                DateOfBirth = BirthDate != null 
                    ? new DateTime(BirthDate.Value.Year, BirthDate.Value.Month, BirthDate.Value.Day) 
                    : DateTime.MinValue,
                Address = new PostalAddress
                {
                    Street = Address?.Line?.FirstOrDefault(),
                    City = Address?.City,
                    PostalCode = Address?.PostalCode,
                    Country = Address?.Country
                }
            };
        }

        /// <summary>
        /// Validate FHIR Patient resource
        /// </summary>
        public override bool Validate()
        {
            return base.Validate() &&
                   !string.IsNullOrEmpty(Identifier) &&
                   Name != null &&
                   !string.IsNullOrEmpty(Name.Family);
        }
    }
}
