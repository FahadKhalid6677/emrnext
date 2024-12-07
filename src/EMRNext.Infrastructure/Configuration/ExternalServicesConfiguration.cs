using System;

namespace EMRNext.Infrastructure.Configuration
{
    public class ExternalServicesConfiguration
    {
        public DrugDatabaseConfig DrugDatabase { get; set; }
        public PaymentGatewayConfig PaymentGateway { get; set; }
        public InsuranceVerificationConfig InsuranceVerification { get; set; }
        public TelehealthConfig Telehealth { get; set; }
        public LabInterfaceConfig LabInterface { get; set; }
        public ImageStorageConfig ImageStorage { get; set; }
    }

    public class DrugDatabaseConfig
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public int CacheExpirationMinutes { get; set; } = 60;
        public bool EnableRealTimeChecks { get; set; } = true;
    }

    public class PaymentGatewayConfig
    {
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public bool UseSandbox { get; set; }
        public string WebhookSecret { get; set; }
    }

    public class InsuranceVerificationConfig
    {
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool EnableRealTimeVerification { get; set; } = true;
    }

    public class TelehealthConfig
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string AccountId { get; set; }
        public int SessionTimeoutMinutes { get; set; } = 60;
        public bool EnableRecording { get; set; } = false;
    }

    public class LabInterfaceConfig
    {
        public string BaseUrl { get; set; }
        public string FacilityId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string[] SupportedLabSystems { get; set; }
    }

    public class ImageStorageConfig
    {
        public string Provider { get; set; } // "AWS", "Azure", or "Local"
        public string BucketName { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public bool EnableCompression { get; set; } = true;
    }
}
