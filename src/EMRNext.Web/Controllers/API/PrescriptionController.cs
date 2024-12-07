using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EMRNext.Core.Models;
using EMRNext.Core.Services;
using EMRNext.Web.Models.API;

namespace EMRNext.Web.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrescriptionController : ControllerBase
    {
        private readonly PrescriptionService _prescriptionService;

        public PrescriptionController(PrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        [HttpPost]
        public async Task<ActionResult<PrescriptionDto>> CreatePrescription([FromBody] PrescriptionDto prescriptionDto)
        {
            try
            {
                var prescription = new Prescription
                {
                    PatientId = prescriptionDto.PatientId,
                    ProviderId = prescriptionDto.ProviderId,
                    MedicationName = prescriptionDto.MedicationName,
                    Dosage = prescriptionDto.Dosage,
                    Frequency = prescriptionDto.Frequency,
                    Duration = prescriptionDto.Duration,
                    DurationUnit = prescriptionDto.DurationUnit,
                    Instructions = prescriptionDto.Instructions,
                    Warnings = prescriptionDto.Warnings
                };

                var createdPrescription = await _prescriptionService.CreatePrescriptionAsync(prescription);

                return Ok(new PrescriptionDto
                {
                    Id = createdPrescription.Id,
                    PatientId = createdPrescription.PatientId,
                    ProviderId = createdPrescription.ProviderId,
                    MedicationName = createdPrescription.MedicationName,
                    Dosage = createdPrescription.Dosage,
                    Frequency = createdPrescription.Frequency,
                    Duration = createdPrescription.Duration,
                    DurationUnit = createdPrescription.DurationUnit,
                    PrescriptionDate = createdPrescription.PrescriptionDate,
                    Status = createdPrescription.Status.ToString(),
                    Instructions = createdPrescription.Instructions,
                    Warnings = createdPrescription.Warnings
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<PrescriptionDto>>> GetPatientPrescriptions(Guid patientId)
        {
            var prescriptions = await _prescriptionService.GetPatientPrescriptionsAsync(patientId);

            var prescriptionDtos = prescriptions.Select(p => new PrescriptionDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                ProviderId = p.ProviderId,
                MedicationName = p.MedicationName,
                Dosage = p.Dosage,
                Frequency = p.Frequency,
                Duration = p.Duration,
                DurationUnit = p.DurationUnit,
                PrescriptionDate = p.PrescriptionDate,
                Status = p.Status.ToString(),
                Instructions = p.Instructions,
                Warnings = p.Warnings
            }).ToList();

            return Ok(prescriptionDtos);
        }

        [HttpPut("{prescriptionId}/discontinue")]
        public async Task<ActionResult<PrescriptionDto>> DiscontinuePrescription(Guid prescriptionId)
        {
            try
            {
                var discontinuedPrescription = await _prescriptionService.DiscontinuePrescriptionAsync(prescriptionId);

                return Ok(new PrescriptionDto
                {
                    Id = discontinuedPrescription.Id,
                    Status = discontinuedPrescription.Status.ToString(),
                    EndDate = discontinuedPrescription.EndDate
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("interactions")]
        public async Task<ActionResult<List<DrugInteractionDto>>> CheckDrugInteractions(
            [FromQuery] List<string> medications)
        {
            var interactions = await _drugInteractionRepository.GetDrugInteractionsAsync(medications);

            return Ok(interactions.Select(i => new DrugInteractionDto
            {
                Drug1 = i.Drug1,
                Drug2 = i.Drug2,
                Severity = i.Severity.ToString(),
                Description = i.Description
            }).ToList());
        }
    }
}
