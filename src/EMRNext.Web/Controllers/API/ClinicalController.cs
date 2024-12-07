using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMRNext.Core.Services.Clinical;
using EMRNext.Web.Models.API;
using Microsoft.Extensions.Logging;

namespace EMRNext.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicalController : ControllerBase
    {
        private readonly ILogger<ClinicalController> _logger;
        private readonly IClinicalDocumentationService _documentationService;
        private readonly ILabOrderService _labService;
        private readonly IImagingService _imagingService;
        private readonly IWorkQueueService _workQueueService;

        public ClinicalController(
            ILogger<ClinicalController> logger,
            IClinicalDocumentationService documentationService,
            ILabOrderService labService,
            IImagingService imagingService,
            IWorkQueueService workQueueService)
        {
            _logger = logger;
            _documentationService = documentationService;
            _labService = labService;
            _imagingService = imagingService;
            _workQueueService = workQueueService;
        }

        [HttpGet("documents/{patientId}")]
        public async Task<IActionResult> GetDocuments(string patientId)
        {
            try
            {
                var documents = await _documentationService.GetPatientDocumentsAsync(patientId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for patient {PatientId}", patientId);
                return StatusCode(500, "Error retrieving documents");
            }
        }

        [HttpPost("documents")]
        public async Task<IActionResult> CreateDocument([FromBody] DocumentRequest request)
        {
            try
            {
                var document = await _documentationService.CreateDocumentAsync(request);
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document");
                return StatusCode(500, "Error creating document");
            }
        }

        [HttpGet("documents/{id}")]
        public async Task<IActionResult> GetDocument(string id)
        {
            try
            {
                var document = await _documentationService.GetDocumentAsync(id);
                if (document == null)
                    return NotFound();
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {Id}", id);
                return StatusCode(500, "Error retrieving document");
            }
        }

        [HttpPost("lab/orders")]
        public async Task<IActionResult> CreateLabOrder([FromBody] LabOrderRequest request)
        {
            try
            {
                var order = await _labService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetLabOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lab order");
                return StatusCode(500, "Error creating lab order");
            }
        }

        [HttpGet("lab/orders/{id}")]
        public async Task<IActionResult> GetLabOrder(string id)
        {
            try
            {
                var order = await _labService.GetOrderAsync(id);
                if (order == null)
                    return NotFound();
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lab order {Id}", id);
                return StatusCode(500, "Error retrieving lab order");
            }
        }

        [HttpPost("imaging/orders")]
        public async Task<IActionResult> CreateImagingOrder([FromBody] ImagingOrderRequest request)
        {
            try
            {
                var order = await _imagingService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetImagingOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating imaging order");
                return StatusCode(500, "Error creating imaging order");
            }
        }

        [HttpGet("imaging/orders/{id}")]
        public async Task<IActionResult> GetImagingOrder(string id)
        {
            try
            {
                var order = await _imagingService.GetOrderAsync(id);
                if (order == null)
                    return NotFound();
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving imaging order {Id}", id);
                return StatusCode(500, "Error retrieving imaging order");
            }
        }

        [HttpGet("workqueue")]
        public async Task<IActionResult> GetWorkQueue()
        {
            try
            {
                var tasks = await _workQueueService.GetTasksAsync(User.Identity.Name);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work queue");
                return StatusCode(500, "Error retrieving work queue");
            }
        }

        [HttpPost("workqueue/tasks")]
        public async Task<IActionResult> CreateTask([FromBody] WorkQueueTaskRequest request)
        {
            try
            {
                var task = await _workQueueService.CreateTaskAsync(request);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, "Error creating task");
            }
        }

        [HttpGet("workqueue/tasks/{id}")]
        public async Task<IActionResult> GetTask(string id)
        {
            try
            {
                var task = await _workQueueService.GetTaskAsync(id);
                if (task == null)
                    return NotFound();
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {Id}", id);
                return StatusCode(500, "Error retrieving task");
            }
        }

        [HttpPut("workqueue/tasks/{id}")]
        public async Task<IActionResult> UpdateTask(string id, [FromBody] WorkQueueTaskRequest request)
        {
            try
            {
                var task = await _workQueueService.UpdateTaskAsync(id, request);
                if (task == null)
                    return NotFound();
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {Id}", id);
                return StatusCode(500, "Error updating task");
            }
        }
    }
}
