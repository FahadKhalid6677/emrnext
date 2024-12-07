using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EMRNext.Core.Domain.Models.EDI;
using EMRNext.Core.Infrastructure;
using EMRNext.Core.Services.Interface;
using EMRNext.Core.Services.Notification;

namespace EMRNext.Core.Services.EDI
{
    public class EDIService : IEDIService
    {
        private readonly EMRNextDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly IEDIConnectionService _connectionService;
        private readonly IEDIValidationService _validationService;
        private readonly IEDITransformService _transformService;
        private readonly IEDIConfigService _configService;

        public EDIService(
            EMRNextDbContext context,
            INotificationService notificationService,
            IAuditService auditService,
            IEDIConnectionService connectionService,
            IEDIValidationService validationService,
            IEDITransformService transformService,
            IEDIConfigService configService)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
            _connectionService = connectionService;
            _validationService = validationService;
            _transformService = transformService;
            _configService = configService;
        }

        public async Task<string> GenerateEDI837Async(EDI837Request request)
        {
            try
            {
                // Validate request
                await ValidateEDI837RequestAsync(request);

                // Get trading partner configuration
                var config = await GetTradingPartnerConfigAsync(request.TradingPartnerId);

                // Transform data to EDI format
                var ediContent = await _transformService.TransformToEDI837Async(request, config);

                // Validate EDI content
                if (!await ValidateEDI837Async(ediContent))
                {
                    throw new EDIValidationException("EDI 837 validation failed");
                }

                // Log transaction
                await LogEDITransactionAsync("837", "Generate", ediContent);

                return ediContent;
            }
            catch (Exception ex)
            {
                await HandleEDIErrorAsync(new EDIError
                {
                    TransactionType = "837",
                    Operation = "Generate",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
                throw;
            }
        }

        public async Task<bool> SendEDI837Async(string ediContent)
        {
            try
            {
                // Validate EDI content before sending
                if (!await ValidateEDI837Async(ediContent))
                {
                    throw new EDIValidationException("EDI 837 validation failed");
                }

                // Get connection configuration
                var config = await _configService.GetCurrentConfigAsync();

                // Send EDI content
                var response = await _connectionService.SendEDIAsync(ediContent, config);

                // Process acknowledgment
                if (response.IsSuccessful)
                {
                    await ProcessEDI999Async(response.Acknowledgment);
                }

                // Log transaction
                await LogEDITransactionAsync("837", "Send", ediContent);

                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                await HandleEDIErrorAsync(new EDIError
                {
                    TransactionType = "837",
                    Operation = "Send",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
                throw;
            }
        }

        public async Task<string> GenerateEDI270Async(EDI270Request request)
        {
            try
            {
                // Validate request
                await ValidateEDI270RequestAsync(request);

                // Get trading partner configuration
                var config = await GetTradingPartnerConfigAsync(request.TradingPartnerId);

                // Transform data to EDI format
                var ediContent = await _transformService.TransformToEDI270Async(request, config);

                // Validate EDI content
                if (!await ValidateEDI270Async(ediContent))
                {
                    throw new EDIValidationException("EDI 270 validation failed");
                }

                // Log transaction
                await LogEDITransactionAsync("270", "Generate", ediContent);

                return ediContent;
            }
            catch (Exception ex)
            {
                await HandleEDIErrorAsync(new EDIError
                {
                    TransactionType = "270",
                    Operation = "Generate",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
                throw;
            }
        }

        public async Task<EDI271Response> ProcessEDI271Async(string ediContent)
        {
            try
            {
                // Validate EDI content
                if (!await ValidateEDI271Async(ediContent))
                {
                    throw new EDIValidationException("EDI 271 validation failed");
                }

                // Transform EDI to response object
                var response = await _transformService.TransformFromEDI271Async(ediContent);

                // Log transaction
                await LogEDITransactionAsync("271", "Process", ediContent);

                return response;
            }
            catch (Exception ex)
            {
                await HandleEDIErrorAsync(new EDIError
                {
                    TransactionType = "271",
                    Operation = "Process",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
                throw;
            }
        }

        public async Task<EDI835Response> ProcessEDI835Async(string ediContent)
        {
            try
            {
                // Validate EDI content
                if (!await ValidateEDI835Async(ediContent))
                {
                    throw new EDIValidationException("EDI 835 validation failed");
                }

                // Transform EDI to response object
                var response = await _transformService.TransformFromEDI835Async(ediContent);

                // Process payments
                await ProcessPaymentsAsync(response);

                // Generate acknowledgment
                await GenerateEDI999Async(ediContent);

                // Log transaction
                await LogEDITransactionAsync("835", "Process", ediContent);

                return response;
            }
            catch (Exception ex)
            {
                await HandleEDIErrorAsync(new EDIError
                {
                    TransactionType = "835",
                    Operation = "Process",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
                throw;
            }
        }

        public async Task<BatchResult> ProcessEDIBatchAsync(string batchContent, string transactionType)
        {
            var result = new BatchResult
            {
                BatchId = Guid.NewGuid().ToString(),
                StartTime = DateTime.UtcNow,
                TransactionType = transactionType,
                Status = "Processing"
            };

            try
            {
                // Split batch into individual transactions
                var transactions = await _transformService.SplitBatchAsync(batchContent);

                var processedCount = 0;
                var errorCount = 0;

                foreach (var transaction in transactions)
                {
                    try
                    {
                        switch (transactionType)
                        {
                            case "835":
                                await ProcessEDI835Async(transaction);
                                break;
                            case "271":
                                await ProcessEDI271Async(transaction);
                                break;
                            case "277":
                                await ProcessEDI277Async(transaction);
                                break;
                            case "999":
                                await ProcessEDI999Async(transaction);
                                break;
                        }

                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        result.Errors.Add(new EDIError
                        {
                            TransactionType = transactionType,
                            Operation = "Process",
                            ErrorMessage = ex.Message,
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }

                result.EndTime = DateTime.UtcNow;
                result.ProcessedCount = processedCount;
                result.ErrorCount = errorCount;
                result.Status = errorCount == 0 ? "Completed" : "CompletedWithErrors";

                // Log batch processing
                await LogEDIBatchAsync(result);

                return result;
            }
            catch (Exception ex)
            {
                result.EndTime = DateTime.UtcNow;
                result.Status = "Failed";
                result.ErrorMessage = ex.Message;

                await HandleEDIErrorAsync(new EDIError
                {
                    TransactionType = transactionType,
                    Operation = "BatchProcess",
                    ErrorMessage = ex.Message,
                    Timestamp = DateTime.UtcNow
                });

                return result;
            }
        }

        private async Task LogEDITransactionAsync(
            string transactionType,
            string operation,
            string content)
        {
            var transaction = new EDITransaction
            {
                TransactionType = transactionType,
                Operation = operation,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            _context.EDITransactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "EDITransaction",
                $"Processed {transactionType} {operation}",
                transaction);
        }

        private async Task LogEDIBatchAsync(BatchResult result)
        {
            var batch = new EDIBatch
            {
                BatchId = result.BatchId,
                TransactionType = result.TransactionType,
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                ProcessedCount = result.ProcessedCount,
                ErrorCount = result.ErrorCount,
                Status = result.Status
            };

            _context.EDIBatches.Add(batch);
            await _context.SaveChangesAsync();

            await _auditService.LogActivityAsync(
                "EDIBatch",
                $"Processed batch {result.BatchId}",
                batch);
        }

        private async Task ProcessPaymentsAsync(EDI835Response response)
        {
            foreach (var payment in response.Payments)
            {
                // Update claim payment status
                await UpdateClaimPaymentAsync(payment);

                // Create payment adjustments
                await CreatePaymentAdjustmentsAsync(payment);

                // Update account balances
                await UpdateAccountBalancesAsync(payment);
            }
        }
    }
}
