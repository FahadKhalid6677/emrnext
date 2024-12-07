using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EMRNext.Core.Domain.Common;

namespace EMRNext.Core.Reporting.Models
{
    /// <summary>
    /// Represents a configurable report definition
    /// </summary>
    public class ReportDefinition : Entity<Guid>
    {
        /// <summary>
        /// Unique code for the report
        /// </summary>
        public string ReportCode { get; set; }

        /// <summary>
        /// Name of the report
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the report's purpose
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category of the report
        /// </summary>
        public ReportCategory Category { get; set; }

        /// <summary>
        /// Data source for the report
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Filters applied to the report
        /// </summary>
        public List<ReportFilter> Filters { get; set; } = new List<ReportFilter>();

        /// <summary>
        /// Aggregation rules for the report
        /// </summary>
        public List<AggregationRule> AggregationRules { get; set; } = new List<AggregationRule>();

        /// <summary>
        /// Visualization settings
        /// </summary>
        public VisualizationSettings VisualizationSettings { get; set; }

        /// <summary>
        /// Indicates if the report is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Permissions required to view the report
        /// </summary>
        public List<string> RequiredPermissions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Report filter definition
    /// </summary>
    public class ReportFilter
    {
        /// <summary>
        /// Field to filter on
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Comparison operator
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Value to compare against
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Compiled filter expression
        /// </summary>
        public Expression<Func<object, bool>> FilterExpression { get; set; }
    }

    /// <summary>
    /// Aggregation rule for report generation
    /// </summary>
    public class AggregationRule
    {
        /// <summary>
        /// Field to aggregate
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Aggregation method
        /// </summary>
        public AggregationType AggregationType { get; set; }
    }

    /// <summary>
    /// Visualization settings for reports
    /// </summary>
    public class VisualizationSettings
    {
        /// <summary>
        /// Type of chart or visualization
        /// </summary>
        public VisualizationType Type { get; set; }

        /// <summary>
        /// Color palette for visualization
        /// </summary>
        public string[] ColorPalette { get; set; }

        /// <summary>
        /// Additional rendering options
        /// </summary>
        public Dictionary<string, object> RenderingOptions { get; set; } = 
            new Dictionary<string, object>();
    }

    /// <summary>
    /// Report categories
    /// </summary>
    public enum ReportCategory
    {
        Clinical,
        Financial,
        Operational,
        Demographic,
        Performance,
        Quality
    }

    /// <summary>
    /// Filter comparison operators
    /// </summary>
    public enum FilterOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        NotContains,
        Between
    }

    /// <summary>
    /// Aggregation types
    /// </summary>
    public enum AggregationType
    {
        Sum,
        Average,
        Count,
        Min,
        Max,
        Median
    }

    /// <summary>
    /// Visualization types
    /// </summary>
    public enum VisualizationType
    {
        BarChart,
        LineChart,
        PieChart,
        ScatterPlot,
        HeatMap,
        TreeMap,
        RadarChart
    }
}
