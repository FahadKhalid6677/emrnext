using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace EMRNext.E2ETests
{
    public class PatientJourneyTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const string BaseUrl = "https://emrnext.railway.app";

        public PatientJourneyTests()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task CompletePatientRegistrationWorkflow()
        {
            // Login
            _driver.Navigate().GoToUrl($"{BaseUrl}/login");
            
            var emailInput = _wait.Until(d => d.FindElement(By.Id("email")));
            var passwordInput = _driver.FindElement(By.Id("password"));
            var loginButton = _driver.FindElement(By.Id("login-submit"));

            emailInput.SendKeys("admin@emrnext.com");
            passwordInput.SendKeys("SecurePassword123!");
            loginButton.Click();

            // Wait for dashboard
            _wait.Until(d => d.FindElement(By.Id("dashboard-container")));

            // Navigate to Patient Registration
            var patientRegLink = _driver.FindElement(By.Id("nav-patient-registration"));
            patientRegLink.Click();

            // Fill Patient Registration Form
            _wait.Until(d => d.FindElement(By.Id("patient-form")));
            
            _driver.FindElement(By.Id("first-name")).SendKeys("John");
            _driver.FindElement(By.Id("last-name")).SendKeys("Doe");
            _driver.FindElement(By.Id("date-of-birth")).SendKeys("1980-01-01");
            _driver.FindElement(By.Id("email")).SendKeys("johndoe@example.com");
            _driver.FindElement(By.Id("phone")).SendKeys("555-123-4567");

            var submitButton = _driver.FindElement(By.Id("submit-patient"));
            submitButton.Click();

            // Verify Patient Registration
            var successMessage = _wait.Until(d => d.FindElement(By.Id("registration-success")));
            Assert.True(successMessage.Displayed, "Patient registration failed");
        }

        [Fact]
        public async Task ScheduleAppointmentWorkflow()
        {
            // Login (reuse login logic)
            _driver.Navigate().GoToUrl($"{BaseUrl}/login");
            
            var emailInput = _wait.Until(d => d.FindElement(By.Id("email")));
            var passwordInput = _driver.FindElement(By.Id("password"));
            var loginButton = _driver.FindElement(By.Id("login-submit"));

            emailInput.SendKeys("admin@emrnext.com");
            passwordInput.SendKeys("SecurePassword123!");
            loginButton.Click();

            // Navigate to Scheduling
            _wait.Until(d => d.FindElement(By.Id("nav-scheduling")));
            var schedulingLink = _driver.FindElement(By.Id("nav-scheduling"));
            schedulingLink.Click();

            // Select Patient
            _wait.Until(d => d.FindElement(By.Id("patient-search")));
            _driver.FindElement(By.Id("patient-search")).SendKeys("John Doe");
            
            var patientResult = _wait.Until(d => d.FindElement(By.Id("patient-result-1")));
            patientResult.Click();

            // Select Appointment Type
            var appointmentTypeSelect = new SelectElement(_driver.FindElement(By.Id("appointment-type")));
            appointmentTypeSelect.SelectByValue("general-checkup");

            // Select Date and Time
            _driver.FindElement(By.Id("appointment-date")).SendKeys(DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"));
            _driver.FindElement(By.Id("appointment-time")).SendKeys("10:00");

            // Submit Appointment
            var scheduleButton = _driver.FindElement(By.Id("schedule-submit"));
            scheduleButton.Click();

            // Verify Appointment Scheduling
            var confirmationMessage = _wait.Until(d => d.FindElement(By.Id("appointment-confirmation")));
            Assert.True(confirmationMessage.Displayed, "Appointment scheduling failed");
        }

        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
