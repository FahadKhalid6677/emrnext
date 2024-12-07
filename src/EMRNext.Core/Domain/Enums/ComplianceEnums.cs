namespace EMRNext.Core.Domain.Enums
{
    /// <summary>
    /// Represents the type of compliance audit
    /// </summary>
    public enum ComplianceAuditType
    {
        /// <summary>
        /// Routine periodic audit
        /// </summary>
        Routine,

        /// <summary>
        /// Triggered by a specific event or incident
        /// </summary>
        Triggered,

        /// <summary>
        /// Comprehensive system-wide audit
        /// </summary>
        Comprehensive,

        /// <summary>
        /// Follow-up audit to verify previous corrective actions
        /// </summary>
        FollowUp,

        /// <summary>
        /// External regulatory audit
        /// </summary>
        External
    }

    /// <summary>
    /// Represents the overall compliance status
    /// </summary>
    public enum ComplianceStatus
    {
        /// <summary>
        /// Fully compliant with all requirements
        /// </summary>
        Compliant,

        /// <summary>
        /// Partially compliant with some minor issues
        /// </summary>
        PartiallyCompliant,

        /// <summary>
        /// Significant compliance gaps identified
        /// </summary>
        NonCompliant,

        /// <summary>
        /// Audit in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Requires immediate corrective action
        /// </summary>
        CriticalNonCompliance
    }

    /// <summary>
    /// Represents the severity of compliance findings
    /// </summary>
    public enum ComplianceSeverity
    {
        /// <summary>
        /// Low-impact finding with minimal risk
        /// </summary>
        Low,

        /// <summary>
        /// Moderate finding that requires attention
        /// </summary>
        Medium,

        /// <summary>
        /// Significant finding with potential legal or operational risks
        /// </summary>
        High,

        /// <summary>
        /// Severe finding that poses immediate and substantial risk
        /// </summary>
        Critical
    }
}
