namespace EMRNext.Core.Domain.Enums
{
    /// <summary>
    /// Defines color scheme types for accessibility
    /// </summary>
    public enum ColorSchemeType
    {
        /// <summary>
        /// Default color scheme
        /// </summary>
        Default,

        /// <summary>
        /// High contrast mode for improved readability
        /// </summary>
        HighContrast,

        /// <summary>
        /// Dark mode for reduced eye strain
        /// </summary>
        DarkMode,

        /// <summary>
        /// Light mode with soft colors
        /// </summary>
        LightMode,

        /// <summary>
        /// Sepia tone for reduced glare
        /// </summary>
        Sepia
    }

    /// <summary>
    /// Defines contrast levels for accessibility
    /// </summary>
    public enum ContrastLevel
    {
        /// <summary>
        /// Standard contrast
        /// </summary>
        Standard,

        /// <summary>
        /// Enhanced contrast for improved readability
        /// </summary>
        Enhanced,

        /// <summary>
        /// Maximum contrast for severe visual impairments
        /// </summary>
        Maximum
    }

    /// <summary>
    /// Defines color blindness mitigation strategies
    /// </summary>
    public enum ColorBlindnessMitigationType
    {
        /// <summary>
        /// No specific color blindness mitigation
        /// </summary>
        None,

        /// <summary>
        /// Basic color adaptation
        /// </summary>
        BasicAdaptation,

        /// <summary>
        /// Full color scheme adaptation
        /// </summary>
        FullAdaptation
    }

    /// <summary>
    /// Defines screen reader verbosity levels
    /// </summary>
    public enum VerbosityLevel
    {
        /// <summary>
        /// Minimal information
        /// </summary>
        Minimal,

        /// <summary>
        /// Standard information level
        /// </summary>
        Standard,

        /// <summary>
        /// Detailed information
        /// </summary>
        Detailed
    }

    /// <summary>
    /// Defines speech rate for screen readers
    /// </summary>
    public enum SpeechRate
    {
        /// <summary>
        /// Slow speech rate
        /// </summary>
        Slow,

        /// <summary>
        /// Medium speech rate
        /// </summary>
        Medium,

        /// <summary>
        /// Fast speech rate
        /// </summary>
        Fast
    }

    /// <summary>
    /// Defines punctuation announcement levels
    /// </summary>
    public enum PunctuationLevel
    {
        /// <summary>
        /// No punctuation announced
        /// </summary>
        None,

        /// <summary>
        /// Some punctuation announced
        /// </summary>
        Some,

        /// <summary>
        /// All punctuation announced
        /// </summary>
        All
    }
}
