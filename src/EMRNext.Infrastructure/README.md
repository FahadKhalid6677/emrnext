# EMRNext Infrastructure Layer

This module contains the infrastructure layer implementations for EMRNext, providing integrations with external services and systems.

## External Services

### Available Services

- **Drug Database Service**: Medication information, interactions, and formulary data
- **Payment Gateway Service**: Payment processing and transaction management
- **Insurance Verification Service**: Coverage verification and eligibility checking
- **Lab Interface Service**: Laboratory order management and results retrieval
- **Telehealth Service**: Virtual visit session management and monitoring

### Configuration

1. Add the following section to your `appsettings.json`:

```json
{
  "ExternalServices": {
    "DrugDatabase": {
      "BaseUrl": "your-drug-db-url",
      "ApiKey": "your-api-key",
      "CacheExpirationMinutes": 60,
      "EnableRealTimeChecks": true
    },
    "PaymentGateway": {
      "MerchantId": "your-merchant-id",
      "PublicKey": "your-public-key",
      "PrivateKey": "your-private-key",
      "UseSandbox": true
    },
    "InsuranceVerification": {
      "BaseUrl": "your-insurance-api-url",
      "Username": "your-username",
      "Password": "your-password",
      "TimeoutSeconds": 30
    },
    "LabInterface": {
      "BaseUrl": "your-lab-api-url",
      "FacilityId": "your-facility-id",
      "Username": "your-username",
      "Password": "your-password"
    },
    "Telehealth": {
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret",
      "AccountId": "your-account-id",
      "SessionTimeoutMinutes": 60
    }
  }
}
```

2. Register services in your `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddExternalServices(Configuration);
}
```

## Usage Examples

### Drug Database Service

```csharp
public class MedicationController : ControllerBase
{
    private readonly IDrugDatabaseService _drugService;

    public MedicationController(IDrugDatabaseService drugService)
    {
        _drugService = drugService;
    }

    public async Task<IActionResult> CheckInteractions(string[] ndcList)
    {
        var interactions = await _drugService.CheckInteractionsAsync(ndcList);
        return Ok(interactions);
    }
}
```

### Payment Gateway Service

```csharp
public class PaymentController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentService;

    public PaymentController(IPaymentGatewayService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        var result = await _paymentService.ProcessPaymentAsync(request);
        return Ok(result);
    }
}
```

### Lab Interface Service

```csharp
public class LabOrderController : ControllerBase
{
    private readonly ILabInterfaceService _labService;

    public LabOrderController(ILabInterfaceService labService)
    {
        _labService = labService;
    }

    public async Task<IActionResult> SubmitOrder([FromBody] LabOrder order)
    {
        var result = await _labService.SubmitLabOrderAsync(order);
        return Ok(result);
    }
}
```

### Telehealth Service

```csharp
public class TelehealthController : ControllerBase
{
    private readonly ITelehealthService _telehealthService;

    public TelehealthController(ITelehealthService telehealthService)
    {
        _telehealthService = telehealthService;
    }

    public async Task<IActionResult> CreateSession([FromBody] SessionRequest request)
    {
        var session = await _telehealthService.CreateSessionAsync(request);
        return Ok(session);
    }
}
```

## Error Handling

All services implement comprehensive error handling and logging. Common exceptions include:

- `ArgumentException`: Invalid input parameters
- `HttpRequestException`: Communication errors with external services
- `UnauthorizedException`: Authentication/authorization failures
- `ValidationException`: Business rule violations

Example error handling:

```csharp
try
{
    var result = await _drugService.GetDrugInfoAsync(ndc);
    return Ok(result);
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "Invalid NDC format: {NDC}", ndc);
    return BadRequest(ex.Message);
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "Drug database service unavailable");
    return StatusCode(503, "Service temporarily unavailable");
}
```

## Testing

The infrastructure layer includes both unit tests and integration tests:

1. Run unit tests:
```bash
dotnet test EMRNext.Infrastructure.Tests --filter Category=Unit
```

2. Run integration tests:
```bash
dotnet test EMRNext.Infrastructure.Tests --filter Category=Integration
```

Note: Integration tests require proper configuration in `appsettings.integration.json`

## Security Considerations

1. **API Keys and Secrets**:
   - Never commit API keys or secrets to source control
   - Use secure configuration management
   - Rotate keys regularly

2. **Data Protection**:
   - All external communication uses HTTPS
   - Sensitive data is encrypted at rest
   - PII is handled according to HIPAA requirements

3. **Authentication**:
   - Each service implements appropriate authentication
   - Credentials are securely stored
   - Access tokens are properly managed

## Performance Optimization

1. **Caching**:
   - Drug information is cached
   - Cache duration is configurable
   - Cache invalidation on updates

2. **Connection Management**:
   - HTTP client pooling
   - Configurable timeouts
   - Retry policies

3. **Monitoring**:
   - Performance metrics logging
   - Error tracking
   - Resource usage monitoring
