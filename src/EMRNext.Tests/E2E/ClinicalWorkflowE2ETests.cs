using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using EMRNext.Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EMRNext.Tests.E2E
{
    public class ClinicalWorkflowE2ETests : IClassFixture<SeleniumFixture>, IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly string _baseUrl;
        private readonly TestDataHelper _testData;

        public ClinicalWorkflowE2ETests(SeleniumFixture fixture)
        {
            _driver = fixture.Driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            _baseUrl = "https://localhost:5001";
            _testData = new TestDataHelper(fixture.ServiceProvider);
        }

        [Fact]
        public async Task CompletePatientVisit_E2E_Workflow()
        {
            try
            {
                // 1. Login
                await LoginAsDoctor();

                // 2. Search and select patient
                var patient = await SearchAndSelectPatient();

                // 3. Create clinical note
                var noteId = await CreateClinicalNote(patient.Id);

                // 4. Add vital signs
                await AddVitalSigns(noteId);

                // 5. Create prescription
                await CreatePrescription(patient.Id, noteId);

                // 6. Create lab order
                await CreateLabOrder(patient.Id, noteId);

                // 7. Generate invoice
                await GenerateInvoice(patient.Id);

                // 8. Sign and finalize note
                await FinalizeNote(noteId);

                // Verify final state
                await VerifyFinalState(patient.Id, noteId);
            }
            catch (Exception ex)
            {
                TakeScreenshot("error");
                throw;
            }
        }

        [Fact]
        public async Task PatientRegistration_E2E_Workflow()
        {
            // 1. Navigate to patient registration
            _driver.Navigate().GoToUrl($"{_baseUrl}/patients/register");
            
            // 2. Fill patient information
            FillPatientForm(new
            {
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = "1990-01-01",
                Gender = "Male",
                Phone = "123-456-7890",
                Email = "john.doe@example.com",
                Address = "123 Main St",
                Insurance = "Blue Cross"
            });

            // 3. Submit form
            _driver.FindElement(By.Id("submit-button")).Click();

            // 4. Verify success
            _wait.Until(d => d.FindElement(By.ClassName("success-message")))
                .Text.Should().Contain("successfully registered");

            // 5. Verify patient exists in database
            var patient = await _testData.GetPatientByEmail("john.doe@example.com");
            patient.Should().NotBeNull();
        }

        [Fact]
        public async Task MedicationPrescription_E2E_Workflow()
        {
            // 1. Login and navigate to prescriptions
            await LoginAsDoctor();
            var patient = await _testData.CreateTestPatient();
            _driver.Navigate().GoToUrl($"{_baseUrl}/patients/{patient.Id}/prescriptions");

            // 2. Create new prescription
            _driver.FindElement(By.Id("new-prescription")).Click();
            
            // 3. Fill prescription details
            var prescriptionData = new
            {
                Medication = "Amoxicillin",
                Dosage = "500mg",
                Frequency = "3 times daily",
                Duration = "7 days",
                Instructions = "Take with food"
            };
            FillPrescriptionForm(prescriptionData);

            // 4. Submit prescription
            _driver.FindElement(By.Id("submit-prescription")).Click();

            // 5. Verify prescription was created
            _wait.Until(d => d.FindElement(By.ClassName("prescription-success")));
            
            // 6. Verify in database
            var prescriptions = await _testData.GetPatientPrescriptions(patient.Id);
            prescriptions.Should().Contain(p => p.MedicationName == "Amoxicillin");
        }

        [Fact]
        public async Task LabResults_E2E_Workflow()
        {
            // 1. Setup test data
            await LoginAsLabTechnician();
            var patient = await _testData.CreateTestPatient();
            var order = await _testData.CreateTestLabOrder(patient.Id);

            // 2. Navigate to lab results
            _driver.Navigate().GoToUrl($"{_baseUrl}/lab/orders/{order.Id}");

            // 3. Enter test results
            var results = new Dictionary<string, string>
            {
                {"glucose", "95"},
                {"cholesterol", "180"},
                {"triglycerides", "150"}
            };
            EnterLabResults(results);

            // 4. Submit results
            _driver.FindElement(By.Id("submit-results")).Click();

            // 5. Verify results submission
            _wait.Until(d => d.FindElement(By.ClassName("results-submitted")));

            // 6. Verify in database
            var savedResults = await _testData.GetLabResults(order.Id);
            savedResults.Should().NotBeNull();
        }

        private async Task LoginAsDoctor()
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/login");
            _driver.FindElement(By.Id("email")).SendKeys("doctor@test.com");
            _driver.FindElement(By.Id("password")).SendKeys("Test123!");
            _driver.FindElement(By.Id("login-button")).Click();
            _wait.Until(d => d.FindElement(By.ClassName("dashboard")));
        }

        private async Task<Patient> SearchAndSelectPatient()
        {
            var patient = await _testData.CreateTestPatient();
            _driver.Navigate().GoToUrl($"{_baseUrl}/patients");
            
            var searchBox = _driver.FindElement(By.Id("patient-search"));
            searchBox.SendKeys(patient.LastName);
            _wait.Until(d => d.FindElement(By.ClassName("patient-row")));
            
            _driver.FindElement(By.CssSelector($"[data-patient-id='{patient.Id}']")).Click();
            return patient;
        }

        private async Task<string> CreateClinicalNote(string patientId)
        {
            _driver.FindElement(By.Id("new-note")).Click();
            
            // Fill note details
            _driver.FindElement(By.Id("note-type")).SendKeys("Progress Note");
            _driver.FindElement(By.Id("chief-complaint"))
                .SendKeys("Patient presents with fever and cough");
            _driver.FindElement(By.Id("assessment"))
                .SendKeys("Upper respiratory infection");
            
            _driver.FindElement(By.Id("save-note")).Click();
            
            var noteElement = _wait.Until(d => 
                d.FindElement(By.ClassName("note-created")));
            return noteElement.GetAttribute("data-note-id");
        }

        private async Task AddVitalSigns(string noteId)
        {
            _driver.FindElement(By.Id("add-vitals")).Click();
            
            // Enter vital signs
            _driver.FindElement(By.Id("temperature")).SendKeys("98.6");
            _driver.FindElement(By.Id("blood-pressure")).SendKeys("120/80");
            _driver.FindElement(By.Id("heart-rate")).SendKeys("72");
            _driver.FindElement(By.Id("respiratory-rate")).SendKeys("16");
            
            _driver.FindElement(By.Id("save-vitals")).Click();
            _wait.Until(d => d.FindElement(By.ClassName("vitals-saved")));
        }

        private async Task CreatePrescription(string patientId, string noteId)
        {
            _driver.FindElement(By.Id("new-prescription")).Click();
            
            // Fill prescription details
            _driver.FindElement(By.Id("medication")).SendKeys("Amoxicillin");
            _driver.FindElement(By.Id("dosage")).SendKeys("500mg");
            _driver.FindElement(By.Id("frequency")).SendKeys("3 times daily");
            _driver.FindElement(By.Id("duration")).SendKeys("7 days");
            
            _driver.FindElement(By.Id("save-prescription")).Click();
            _wait.Until(d => d.FindElement(By.ClassName("prescription-saved")));
        }

        private async Task CreateLabOrder(string patientId, string noteId)
        {
            _driver.FindElement(By.Id("new-lab-order")).Click();
            
            // Select lab tests
            _driver.FindElement(By.Id("test-cbc")).Click();
            _driver.FindElement(By.Id("test-metabolic")).Click();
            
            _driver.FindElement(By.Id("save-order")).Click();
            _wait.Until(d => d.FindElement(By.ClassName("order-saved")));
        }

        private async Task GenerateInvoice(string patientId)
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}/billing/new");
            
            // Select services
            _driver.FindElement(By.Id("service-office-visit")).Click();
            _driver.FindElement(By.Id("service-lab-tests")).Click();
            
            _driver.FindElement(By.Id("generate-invoice")).Click();
            _wait.Until(d => d.FindElement(By.ClassName("invoice-generated")));
        }

        private async Task FinalizeNote(string noteId)
        {
            _driver.FindElement(By.Id("finalize-note")).Click();
            
            // Confirm finalization
            _driver.FindElement(By.Id("confirm-finalize")).Click();
            _wait.Until(d => d.FindElement(By.ClassName("note-finalized")));
        }

        private async Task VerifyFinalState(string patientId, string noteId)
        {
            // Verify note status
            var note = await _testData.GetNote(noteId);
            note.Status.Should().Be("Finalized");
            
            // Verify prescriptions
            var prescriptions = await _testData.GetPatientPrescriptions(patientId);
            prescriptions.Should().NotBeEmpty();
            
            // Verify lab orders
            var orders = await _testData.GetPatientLabOrders(patientId);
            orders.Should().NotBeEmpty();
            
            // Verify invoice
            var invoice = await _testData.GetPatientInvoice(patientId);
            invoice.Should().NotBeNull();
        }

        private void FillPatientForm(dynamic data)
        {
            _driver.FindElement(By.Id("firstName")).SendKeys(data.FirstName);
            _driver.FindElement(By.Id("lastName")).SendKeys(data.LastName);
            _driver.FindElement(By.Id("dateOfBirth")).SendKeys(data.DateOfBirth);
            _driver.FindElement(By.Id("gender")).SendKeys(data.Gender);
            _driver.FindElement(By.Id("phone")).SendKeys(data.Phone);
            _driver.FindElement(By.Id("email")).SendKeys(data.Email);
            _driver.FindElement(By.Id("address")).SendKeys(data.Address);
            _driver.FindElement(By.Id("insurance")).SendKeys(data.Insurance);
        }

        private void FillPrescriptionForm(dynamic data)
        {
            _driver.FindElement(By.Id("medication")).SendKeys(data.Medication);
            _driver.FindElement(By.Id("dosage")).SendKeys(data.Dosage);
            _driver.FindElement(By.Id("frequency")).SendKeys(data.Frequency);
            _driver.FindElement(By.Id("duration")).SendKeys(data.Duration);
            _driver.FindElement(By.Id("instructions")).SendKeys(data.Instructions);
        }

        private void EnterLabResults(Dictionary<string, string> results)
        {
            foreach (var (test, value) in results)
            {
                _driver.FindElement(By.Id($"result-{test}")).SendKeys(value);
            }
        }

        private void TakeScreenshot(string name)
        {
            var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
            screenshot.SaveAsFile($"screenshots/{name}_{DateTime.Now:yyyyMMddHHmmss}.png");
        }

        public void Dispose()
        {
            _driver?.Quit();
        }
    }
}
