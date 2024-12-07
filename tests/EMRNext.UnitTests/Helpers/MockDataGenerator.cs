using System;
using System.Collections.Generic;
using Bogus;
using EMRNext.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace EMRNext.UnitTests.Helpers
{
    public static class MockDataGenerator
    {
        public static List<Patient> GeneratePatients(int count = 10)
        {
            var faker = new Faker<Patient>()
                .RuleFor(p => p.Id, f => Guid.NewGuid())
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.DateOfBirth, f => f.Date.Past(50))
                .RuleFor(p => p.Gender, f => f.PickRandom(new[] { "Male", "Female", "Other" }))
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.PhoneNumber, f => f.Phone.PhoneNumber());

            return faker.Generate(count);
        }

        public static List<LabOrder> GenerateLabOrders(List<Patient> patients, int count = 10)
        {
            var faker = new Faker<LabOrder>()
                .RuleFor(lo => lo.Id, f => Guid.NewGuid())
                .RuleFor(lo => lo.PatientId, f => f.PickRandom(patients).Id)
                .RuleFor(lo => lo.OrderDate, f => f.Date.Recent())
                .RuleFor(lo => lo.Status, f => f.PickRandom(new[] { "Pending", "In Progress", "Completed", "Cancelled" }))
                .RuleFor(lo => lo.TestType, f => f.PickRandom(new[] { "Blood Test", "Urine Test", "X-Ray", "MRI" }));

            return faker.Generate(count);
        }

        public static IdentityUser GenerateIdentityUser(string role = "Patient")
        {
            var faker = new Faker();
            return new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = faker.Internet.UserName(),
                Email = faker.Internet.Email(),
                EmailConfirmed = true,
            };
        }

        public static List<IdentityRole> GenerateRoles()
        {
            return new List<IdentityRole>
            {
                new IdentityRole("SystemAdmin"),
                new IdentityRole("Physician"),
                new IdentityRole("Nurse"),
                new IdentityRole("Patient")
            };
        }
    }
}
