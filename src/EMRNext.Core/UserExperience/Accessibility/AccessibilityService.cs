using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Domain.Entities.User;
using EMRNext.Core.Domain.Enums;

namespace EMRNext.Core.UserExperience.Accessibility
{
    /// <summary>
    /// Advanced accessibility service for adaptive user experience
    /// </summary>
    public class AccessibilityService : IAccessibilityService
    {
        private readonly ILogger<AccessibilityService> _logger;
        private readonly IUserPreferenceRepository _userPreferenceRepository;

        public AccessibilityService(
            ILogger<AccessibilityService> logger,
            IUserPreferenceRepository userPreferenceRepository)
        {
            _logger = logger;
            _userPreferenceRepository = userPreferenceRepository;
        }

        /// <summary>
        /// Generate personalized accessibility configuration
        /// </summary>
        public async Task<AccessibilityConfiguration> GenerateAccessibilityConfigAsync(Guid userId)
        {
            try 
            {
                var userPreferences = await _userPreferenceRepository.GetUserAccessibilityPreferencesAsync(userId);
                
                return new AccessibilityConfiguration
                {
                    UserId = userId,
                    ColorScheme = DetermineColorScheme(userPreferences),
                    FontSettings = GenerateFontSettings(userPreferences),
                    NavigationAssistance = ConfigureNavigationAssistance(userPreferences),
                    ScreenReaderOptimization = ConfigureScreenReaderSettings(userPreferences),
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating accessibility config for user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Determine optimal color scheme based on user preferences and accessibility needs
        /// </summary>
        private ColorSchemeConfiguration DetermineColorScheme(UserAccessibilityPreferences preferences)
        {
            return new ColorSchemeConfiguration
            {
                BaseScheme = preferences.PreferredColorScheme ?? ColorSchemeType.HighContrast,
                ContrastLevel = preferences.ContrastPreference ?? ContrastLevel.Enhanced,
                ColorBlindnessMitigation = preferences.HasColorBlindness 
                    ? ColorBlindnessMitigationType.FullAdaptation 
                    : ColorBlindnessMitigationType.None
            };
        }

        /// <summary>
        /// Generate personalized font settings
        /// </summary>
        private FontConfiguration GenerateFontSettings(UserAccessibilityPreferences preferences)
        {
            return new FontConfiguration
            {
                BaseFontSize = preferences.PreferredFontSize ?? 16,
                FontFamily = preferences.PreferredFontFamily ?? "Atkinson Hyperlegible",
                LineHeight = preferences.PreferredLineHeight ?? 1.5,
                LetterSpacing = preferences.HasDyslexia ? 0.1 : 0.05
            };
        }

        /// <summary>
        /// Configure navigation assistance
        /// </summary>
        private NavigationAssistanceConfiguration ConfigureNavigationAssistance(UserAccessibilityPreferences preferences)
        {
            return new NavigationAssistanceConfiguration
            {
                KeyboardNavigation = preferences.PreferKeyboardNavigation,
                VoiceNavigation = preferences.SupportVoiceNavigation,
                SimplifiedNavigation = preferences.PreferSimplifiedInterface,
                NavigationHints = preferences.NeedsNavigationAssistance
            };
        }

        /// <summary>
        /// Configure screen reader settings
        /// </summary>
        private ScreenReaderConfiguration ConfigureScreenReaderSettings(UserAccessibilityPreferences preferences)
        {
            return new ScreenReaderConfiguration
            {
                Enabled = preferences.ScreenReaderRequired,
                VerbosityLevel = preferences.ScreenReaderVerbosity ?? VerbosityLevel.Detailed,
                SpeechRate = preferences.PreferredSpeechRate ?? SpeechRate.Medium,
                PunctuationLevel = preferences.PunctuationAnnouncement ?? PunctuationLevel.All
            };
        }

        /// <summary>
        /// Validate and update user accessibility preferences
        /// </summary>
        public async Task<UserAccessibilityPreferences> UpdateAccessibilityPreferencesAsync(
            Guid userId, 
            UserAccessibilityPreferences preferences)
        {
            try 
            {
                // Validate preferences
                ValidateAccessibilityPreferences(preferences);

                // Save preferences
                await _userPreferenceRepository.UpdateUserAccessibilityPreferencesAsync(userId, preferences);

                _logger.LogInformation($"Updated accessibility preferences for user {userId}");

                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating accessibility preferences for user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Comprehensive validation of accessibility preferences
        /// </summary>
        private void ValidateAccessibilityPreferences(UserAccessibilityPreferences preferences)
        {
            // Validate font size
            if (preferences.PreferredFontSize.HasValue)
            {
                preferences.PreferredFontSize = Math.Clamp(
                    preferences.PreferredFontSize.Value, 12, 48);
            }

            // Validate contrast levels
            if (preferences.ContrastPreference.HasValue)
            {
                preferences.ContrastPreference = preferences.ContrastPreference.Value;
            }

            // Additional validation logic can be added here
        }

        /// <summary>
        /// Generate accessibility compliance report
        /// </summary>
        public async Task<AccessibilityComplianceReport> GenerateAccessibilityComplianceReportAsync()
        {
            try 
            {
                var totalUsers = await _userPreferenceRepository.GetTotalUsersCountAsync();
                var accessibilityEnabledUsers = await _userPreferenceRepository.GetAccessibilityEnabledUsersCountAsync();

                return new AccessibilityComplianceReport
                {
                    GeneratedAt = DateTime.UtcNow,
                    TotalUsers = totalUsers,
                    AccessibilityEnabledUsers = accessibilityEnabledUsers,
                    AccessibilityCompliancePercentage = 
                        (double)accessibilityEnabledUsers / totalUsers * 100,
                    TopAccessibilityFeatures = await GetTopAccessibilityFeaturesAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating accessibility compliance report");
                throw;
            }
        }

        /// <summary>
        /// Retrieve top accessibility features used
        /// </summary>
        private async Task<List<AccessibilityFeatureUsage>> GetTopAccessibilityFeaturesAsync()
        {
            return await _userPreferenceRepository.GetTopAccessibilityFeaturesAsync();
        }
    }

    /// <summary>
    /// Comprehensive accessibility configuration
    /// </summary>
    public class AccessibilityConfiguration
    {
        public Guid UserId { get; set; }
        public ColorSchemeConfiguration ColorScheme { get; set; }
        public FontConfiguration FontSettings { get; set; }
        public NavigationAssistanceConfiguration NavigationAssistance { get; set; }
        public ScreenReaderConfiguration ScreenReaderOptimization { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Color scheme configuration
    /// </summary>
    public class ColorSchemeConfiguration
    {
        public ColorSchemeType BaseScheme { get; set; }
        public ContrastLevel ContrastLevel { get; set; }
        public ColorBlindnessMitigationType ColorBlindnessMitigation { get; set; }
    }

    /// <summary>
    /// Font configuration
    /// </summary>
    public class FontConfiguration
    {
        public int BaseFontSize { get; set; }
        public string FontFamily { get; set; }
        public double LineHeight { get; set; }
        public double LetterSpacing { get; set; }
    }

    /// <summary>
    /// Navigation assistance configuration
    /// </summary>
    public class NavigationAssistanceConfiguration
    {
        public bool KeyboardNavigation { get; set; }
        public bool VoiceNavigation { get; set; }
        public bool SimplifiedNavigation { get; set; }
        public bool NavigationHints { get; set; }
    }

    /// <summary>
    /// Screen reader configuration
    /// </summary>
    public class ScreenReaderConfiguration
    {
        public bool Enabled { get; set; }
        public VerbosityLevel VerbosityLevel { get; set; }
        public SpeechRate SpeechRate { get; set; }
        public PunctuationLevel PunctuationLevel { get; set; }
    }

    /// <summary>
    /// Accessibility compliance report
    /// </summary>
    public class AccessibilityComplianceReport
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalUsers { get; set; }
        public int AccessibilityEnabledUsers { get; set; }
        public double AccessibilityCompliancePercentage { get; set; }
        public List<AccessibilityFeatureUsage> TopAccessibilityFeatures { get; set; }
    }

    /// <summary>
    /// Accessibility feature usage tracking
    /// </summary>
    public class AccessibilityFeatureUsage
    {
        public string FeatureName { get; set; }
        public int UsageCount { get; set; }
        public double UsagePercentage { get; set; }
    }

    /// <summary>
    /// Interface for accessibility service
    /// </summary>
    public interface IAccessibilityService
    {
        /// <summary>
        /// Generate personalized accessibility configuration
        /// </summary>
        Task<AccessibilityConfiguration> GenerateAccessibilityConfigAsync(Guid userId);

        /// <summary>
        /// Update user accessibility preferences
        /// </summary>
        Task<UserAccessibilityPreferences> UpdateAccessibilityPreferencesAsync(
            Guid userId, 
            UserAccessibilityPreferences preferences);

        /// <summary>
        /// Generate accessibility compliance report
        /// </summary>
        Task<AccessibilityComplianceReport> GenerateAccessibilityComplianceReportAsync();
    }
}
