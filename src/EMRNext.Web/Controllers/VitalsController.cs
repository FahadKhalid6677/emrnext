using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Services;
using EMRNext.Core.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EMRNext.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VitalsController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IVitalService _vitalService;

        public VitalsController(
            IValidationService validationService,
            IVitalService vitalService)
        {
            _validationService = validationService;
            _vitalService = vitalService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Vital vital)
        {
            // Validate the vital signs
            var validationResult = await _validationService.ValidateAsync(vital);
            
            if (!validationResult.IsValid)
            {
                // Return validation errors
                return BadRequest(new
                {
                    Errors = validationResult.Errors,
                    Warnings = validationResult.Warnings
                });
            }

            // Save the vital signs
            var savedVital = await _vitalService.CreateAsync(vital);
            return CreatedAtAction(nameof(GetById), new { id = savedVital.Id }, savedVital);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Vital vital)
        {
            if (id != vital.Id)
            {
                return BadRequest("ID mismatch");
            }

            // Validate the vital signs
            var context = ValidationContext.Create(isNew: false);
            var validationResult = await _validationService.ValidateAsync(vital, context);
            
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Errors = validationResult.Errors,
                    Warnings = validationResult.Warnings
                });
            }

            // Update the vital signs
            await _vitalService.UpdateAsync(vital);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vital = await _vitalService.GetByIdAsync(id);
            if (vital == null)
            {
                return NotFound();
            }

            return Ok(vital);
        }
    }
}
