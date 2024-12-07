using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Models;
using EMRNext.Core.Services;
using EMRNext.Web.Models.API;

namespace EMRNext.Web.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderEntryController : ControllerBase
    {
        private readonly OrderEntryService _orderEntryService;
        private readonly ILogger<OrderEntryController> _logger;

        public OrderEntryController(
            OrderEntryService orderEntryService,
            ILogger<OrderEntryController> logger)
        {
            _orderEntryService = orderEntryService;
            _logger = logger;
        }

        [HttpPost("create")]
        [Authorize(Policy = "CreateOrder")]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderRequestDto orderRequest)
        {
            try
            {
                // Map DTO to domain model
                var order = MapOrderRequestToDomainModel(orderRequest);
                
                // Set ordering provider from authenticated user
                order.OrderedBy = User.Identity.Name;

                var createdOrder = await _orderEntryService.CreateOrderAsync(order);

                return Ok(MapOrderToDtoResponse(createdOrder));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid order creation request");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpPut("{orderId}/status")]
        [Authorize(Policy = "UpdateOrderStatus")]
        public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(
            Guid orderId, 
            [FromBody] UpdateOrderStatusRequestDto statusUpdate)
        {
            try
            {
                var updatedOrder = await _orderEntryService.UpdateOrderStatusAsync(
                    orderId, 
                    statusUpdate.NewStatus, 
                    User.Identity.Name
                );

                return Ok(MapOrderToDtoResponse(updatedOrder));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid order status update");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Illegal order status transition");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("patient/{patientId}")]
        [Authorize(Policy = "ViewPatientOrders")]
        public async Task<ActionResult<List<OrderResponseDto>>> GetPatientOrders(
            string patientId, 
            [FromQuery] OrderType? orderType = null, 
            [FromQuery] OrderStatus? status = null)
        {
            try
            {
                var orders = await _orderEntryService.GetPatientOrdersAsync(
                    patientId, 
                    orderType, 
                    status
                );

                return Ok(orders.Select(MapOrderToDtoResponse).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient orders");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        [HttpGet("analytics")]
        [Authorize(Policy = "ViewOrderAnalytics")]
        public async Task<ActionResult<OrderAnalyticsDto>> GetOrderAnalytics(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var analytics = await _orderEntryService.GenerateOrderAnalyticsAsync(
                    startDate.Value, 
                    endDate.Value
                );

                return Ok(MapOrderAnalyticsToDto(analytics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating order analytics");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

        private ClinicalOrder MapOrderRequestToDomainModel(CreateOrderRequestDto orderRequest)
        {
            return orderRequest.Type switch
            {
                OrderType.Medication => new MedicationOrder
                {
                    Type = OrderType.Medication,
                    MedicationId = orderRequest.Details.GetProperty("medicationId").GetString(),
                    Dosage = decimal.Parse(orderRequest.Details.GetProperty("dosage").GetString()),
                    DosageUnit = Enum.Parse<DosageUnit>(orderRequest.Details.GetProperty("dosageUnit").GetString()),
                    Frequency = orderRequest.Details.GetProperty("frequency").GetString()
                },
                OrderType.DiagnosticTest => new DiagnosticTestOrder
                {
                    Type = OrderType.DiagnosticTest,
                    TestCode = orderRequest.Details.GetProperty("testCode").GetString(),
                    TestType = Enum.Parse<DiagnosticTestType>(orderRequest.Details.GetProperty("testType").GetString())
                },
                OrderType.Procedure => new ProcedureOrder
                {
                    Type = OrderType.Procedure,
                    ProcedureCode = orderRequest.Details.GetProperty("procedureCode").GetString(),
                    ProcedureType = Enum.Parse<ProcedureType>(orderRequest.Details.GetProperty("procedureType").GetString())
                },
                OrderType.Referral => new ReferralOrder
                {
                    Type = OrderType.Referral,
                    SpecialtyId = orderRequest.Details.GetProperty("specialtyId").GetString(),
                    Urgency = Enum.Parse<ReferralUrgency>(orderRequest.Details.GetProperty("urgency").GetString())
                },
                _ => throw new ArgumentException("Unsupported order type")
            };
        }

        private OrderResponseDto MapOrderToDtoResponse(ClinicalOrder order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                PatientId = order.PatientId,
                OrderedBy = order.OrderedBy,
                OrderedAt = order.OrderedAt,
                Type = order.Type,
                Priority = order.Priority,
                Status = order.Status,
                Notes = order.Notes,
                PotentialInteractions = order is MedicationOrder medOrder 
                    ? medOrder.PotentialInteractions?.Select(i => new MedicationInteractionDto
                    {
                        MedicationId = i.MedicationId,
                        Severity = i.Severity,
                        InteractionDescription = i.InteractionDescription
                    }).ToList() 
                    : null
            };
        }

        private OrderAnalyticsDto MapOrderAnalyticsToDto(OrderAnalytics analytics)
        {
            return new OrderAnalyticsDto
            {
                TotalOrders = analytics.TotalOrders,
                OrderTypeBreakdown = analytics.OrderTypeBreakdown.Select(otb => new OrderTypeAnalyticsDto
                {
                    Type = otb.Type,
                    Count = otb.Count,
                    CompletionRate = otb.CompletionRate
                }).ToList(),
                StatusBreakdown = analytics.StatusBreakdown.Select(sb => new OrderStatusAnalyticsDto
                {
                    Status = sb.Status,
                    Count = sb.Count
                }).ToList(),
                AverageComplianceScore = analytics.AverageComplianceScore
            };
        }
    }
}
