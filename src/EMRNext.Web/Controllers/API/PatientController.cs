using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMRNext.Core.Services.Patients;
using EMRNext.Web.Models.API;
using Microsoft.Extensions.Logging;

namespace EMRNext.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly ILogger<PatientController> _logger;
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly IMedicalHistoryService _historyService;

        public PatientController(
            ILogger<PatientController> logger,
            IPatientService patientService,
            IAppointmentService appointmentService,
            IMedicalHistoryService historyService)
        {
            _logger = logger;
            _patientService = patientService;
            _appointmentService = appointmentService;
            _historyService = historyService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] PatientRegistrationRequest request)
        {
            try
            {
                var patient = await _patientService.RegisterPatientAsync(request);
                return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return StatusCode(500, "Error creating patient");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(string id)
        {
            try
            {
                var patient = await _patientService.GetPatientAsync(id);
                if (patient == null)
                    return NotFound();
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient {Id}", id);
                return StatusCode(500, "Error retrieving patient");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] PatientUpdateRequest request)
        {
            try
            {
                var patient = await _patientService.UpdatePatientAsync(id, request);
                if (patient == null)
                    return NotFound();
                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient {Id}", id);
                return StatusCode(500, "Error updating patient");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPatients([FromQuery] PatientSearchRequest request)
        {
            try
            {
                var patients = await _patientService.SearchPatientsAsync(request);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching patients");
                return StatusCode(500, "Error searching patients");
            }
        }

        [HttpPost("{id}/appointments")]
        public async Task<IActionResult> CreateAppointment(string id, [FromBody] AppointmentRequest request)
        {
            try
            {
                request.PatientId = id;
                var appointment = await _appointmentService.CreateAppointmentAsync(request);
                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for patient {Id}", id);
                return StatusCode(500, "Error creating appointment");
            }
        }

        [HttpGet("{id}/appointments")]
        public async Task<IActionResult> GetAppointments(string id)
        {
            try
            {
                var appointments = await _appointmentService.GetPatientAppointmentsAsync(id);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments for patient {Id}", id);
                return StatusCode(500, "Error retrieving appointments");
            }
        }

        [HttpGet("appointments/{id}")]
        public async Task<IActionResult> GetAppointment(string id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentAsync(id);
                if (appointment == null)
                    return NotFound();
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment {Id}", id);
                return StatusCode(500, "Error retrieving appointment");
            }
        }

        [HttpPost("{id}/history")]
        public async Task<IActionResult> AddMedicalHistory(string id, [FromBody] MedicalHistoryRequest request)
        {
            try
            {
                request.PatientId = id;
                var history = await _historyService.AddMedicalHistoryAsync(request);
                return CreatedAtAction(nameof(GetMedicalHistory), new { id = history.Id }, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medical history for patient {Id}", id);
                return StatusCode(500, "Error adding medical history");
            }
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetMedicalHistory(string id)
        {
            try
            {
                var history = await _historyService.GetPatientMedicalHistoryAsync(id);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical history for patient {Id}", id);
                return StatusCode(500, "Error retrieving medical history");
            }
        }

        [HttpPost("{id}/allergies")]
        public async Task<IActionResult> AddAllergy(string id, [FromBody] AllergyRequest request)
        {
            try
            {
                request.PatientId = id;
                var allergy = await _historyService.AddAllergyAsync(request);
                return CreatedAtAction(nameof(GetAllergies), new { id = allergy.Id }, allergy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding allergy for patient {Id}", id);
                return StatusCode(500, "Error adding allergy");
            }
        }

        [HttpGet("{id}/allergies")]
        public async Task<IActionResult> GetAllergies(string id)
        {
            try
            {
                var allergies = await _historyService.GetPatientAllergiesAsync(id);
                return Ok(allergies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allergies for patient {Id}", id);
                return StatusCode(500, "Error retrieving allergies");
            }
        }
    }
}
