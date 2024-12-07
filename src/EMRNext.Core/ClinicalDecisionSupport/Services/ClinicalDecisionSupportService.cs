using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.ClinicalDecisionSupport.Models;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Repositories;

namespace EMRNext.Core.ClinicalDecisionSupport.Services
{
    /// <summary>
    /// Service for managing and executing clinical decision support rules
    /// </summary>
    public class ClinicalDecisionSupportService
    {
        private readonly ILogger<ClinicalDecisionSupportService> _logger;
        private readonly IGenericRepository<ClinicalRule> _ruleRepository;
        private readonly IGenericRepository<Patient> _patientRepository;

        public ClinicalDecisionSupportService(
            ILogger<ClinicalDecisionSupportService> logger,
            IGenericRepository<ClinicalRule> ruleRepository,
            IGenericRepository<Patient> patientRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
        }

        /// <summary>
        /// Evaluate clinical rules for a specific patient
        /// </summary>
        public async Task<ClinicalDecisionResult> EvaluatePatientRulesAsync(Guid patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
            {
                _logger.LogWarning($"Patient with ID {patientId} not found");
                return new ClinicalDecisionResult { Success = false };
            }

            var activeRules = await _ruleRepository.GetAllAsync(r => r.IsActive);
            var evaluationResults = new List<RuleEvaluationResult>();

            foreach (var rule in activeRules)
            {
                var ruleResult = EvaluateRule(patient, rule);
                if (ruleResult.Triggered)
                {
                    evaluationResults.Add(ruleResult);
                }
            }

            return new ClinicalDecisionResult
            {
                Success = true,
                Patient = patient,
                RuleResults = evaluationResults,
                OverallRecommendations = AggregateRecommendations(evaluationResults)
            };
        }

        /// <summary>
        /// Evaluate a single rule against a patient
        /// </summary>
        private RuleEvaluationResult EvaluateRule(Patient patient, ClinicalRule rule)
        {
            bool ruleTriggered = false;
            var triggeredConditions = new List<RuleCondition>();

            foreach (var condition in rule.Conditions)
            {
                bool conditionMet = EvaluateCondition(patient, condition);
                if (conditionMet)
                {
                    ruleTriggered = true;
                    triggeredConditions.Add(condition);
                }
            }

            return new RuleEvaluationResult
            {
                Rule = rule,
                Triggered = ruleTriggered,
                TriggeredConditions = triggeredConditions,
                Recommendations = ruleTriggered ? rule.Recommendations : new List<RuleRecommendation>()
            };
        }

        /// <summary>
        /// Evaluate a single condition against patient data
        /// </summary>
        private bool EvaluateCondition(Patient patient, RuleCondition condition)
        {
            // Complex condition evaluation logic
            // This is a simplified example and would need to be expanded
            try 
            {
                // Use compiled expression for flexible condition evaluation
                if (condition.ConditionExpression != null)
                {
                    var compiledCondition = condition.ConditionExpression.Compile();
                    return compiledCondition(GetPatientDataForCondition(patient, condition.DataType));
                }

                // Fallback to basic comparison if no expression
                return CompareValues(
                    GetPatientDataForCondition(patient, condition.DataType), 
                    condition.Operator, 
                    condition.ExpectedValue
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error evaluating condition for data type {condition.DataType}");
                return false;
            }
        }

        /// <summary>
        /// Get patient data for a specific condition
        /// </summary>
        private object GetPatientDataForCondition(Patient patient, string dataType)
        {
            // Implement logic to retrieve patient data based on data type
            // This is a placeholder and would need to be expanded
            return dataType switch
            {
                "Age" => patient.Age,
                "Weight" => patient.Weight,
                "Height" => patient.Height,
                "BloodPressure" => patient.BloodPressure,
                "MedicalHistory" => patient.MedicalHistory,
                _ => null
            };
        }

        /// <summary>
        /// Compare values based on comparison operator
        /// </summary>
        private bool CompareValues(object patientValue, ComparisonOperator op, object expectedValue)
        {
            if (patientValue == null || expectedValue == null)
                return false;

            return op switch
            {
                ComparisonOperator.Equals => patientValue.Equals(expectedValue),
                ComparisonOperator.NotEquals => !patientValue.Equals(expectedValue),
                ComparisonOperator.GreaterThan => 
                    Convert.ToDouble(patientValue) > Convert.ToDouble(expectedValue),
                ComparisonOperator.LessThan => 
                    Convert.ToDouble(patientValue) < Convert.ToDouble(expectedValue),
                ComparisonOperator.GreaterThanOrEqual => 
                    Convert.ToDouble(patientValue) >= Convert.ToDouble(expectedValue),
                ComparisonOperator.LessThanOrEqual => 
                    Convert.ToDouble(patientValue) <= Convert.ToDouble(expectedValue),
                ComparisonOperator.Contains => 
                    patientValue.ToString().Contains(expectedValue.ToString()),
                ComparisonOperator.NotContains => 
                    !patientValue.ToString().Contains(expectedValue.ToString()),
                _ => false
            };
        }

        /// <summary>
        /// Aggregate recommendations from multiple rule evaluations
        /// </summary>
        private List<RuleRecommendation> AggregateRecommendations(List<RuleEvaluationResult> ruleResults)
        {
            return ruleResults
                .Where(r => r.Triggered)
                .SelectMany(r => r.Recommendations)
                .OrderByDescending(r => r.Urgency)
                .ToList();
        }

        /// <summary>
        /// Create a new clinical rule
        /// </summary>
        public async Task<ClinicalRule> CreateRuleAsync(ClinicalRule rule)
        {
            try 
            {
                var createdRule = await _ruleRepository.AddAsync(rule);
                _logger.LogInformation($"Created new clinical rule: {rule.Name}");
                return createdRule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating clinical rule: {rule.Name}");
                throw;
            }
        }
    }

    /// <summary>
    /// Result of clinical decision support evaluation
    /// </summary>
    public class ClinicalDecisionResult
    {
        /// <summary>
        /// Indicates if the evaluation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Patient for whom rules were evaluated
        /// </summary>
        public Patient Patient { get; set; }

        /// <summary>
        /// Results of individual rule evaluations
        /// </summary>
        public List<RuleEvaluationResult> RuleResults { get; set; }

        /// <summary>
        /// Aggregated recommendations
        /// </summary>
        public List<RuleRecommendation> OverallRecommendations { get; set; }
    }

    /// <summary>
    /// Result of a single rule evaluation
    /// </summary>
    public class RuleEvaluationResult
    {
        /// <summary>
        /// The evaluated clinical rule
        /// </summary>
        public ClinicalRule Rule { get; set; }

        /// <summary>
        /// Indicates if the rule was triggered
        /// </summary>
        public bool Triggered { get; set; }

        /// <summary>
        /// Conditions that were met
        /// </summary>
        public List<RuleCondition> TriggeredConditions { get; set; }

        /// <summary>
        /// Recommendations from the rule
        /// </summary>
        public List<RuleRecommendation> Recommendations { get; set; }
    }
}
