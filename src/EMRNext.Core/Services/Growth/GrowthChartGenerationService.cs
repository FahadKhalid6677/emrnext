using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EMRNext.Core.Models.Growth;

namespace EMRNext.Core.Services.Growth
{
    public interface IGrowthChartGenerationService
    {
        Task<byte[]> GenerateGrowthChartAsync(
            ChartDefinition chartDef,
            List<GrowthMeasurement> measurements,
            int width = 800,
            int height = 600);
        
        Task<byte[]> GenerateMultiChartReportAsync(
            int patientId,
            List<MeasurementType> measurementTypes,
            DateTime startDate,
            DateTime endDate);
    }

    public class GrowthChartGenerationService : IGrowthChartGenerationService
    {
        private readonly ILogger<GrowthChartGenerationService> _logger;
        private readonly IGrowthDataRepository _repository;
        private const int Padding = 50;
        private const int LegendWidth = 150;

        public GrowthChartGenerationService(
            ILogger<GrowthChartGenerationService> logger,
            IGrowthDataRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<byte[]> GenerateGrowthChartAsync(
            ChartDefinition chartDef,
            List<GrowthMeasurement> measurements,
            int width = 800,
            int height = 600)
        {
            try
            {
                using (var bitmap = new Bitmap(width, height))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.Clear(Color.White);

                    // Calculate chart area
                    var chartArea = new Rectangle(
                        Padding,
                        Padding,
                        width - (Padding * 2) - LegendWidth,
                        height - (Padding * 2));

                    // Draw chart background
                    DrawChartBackground(graphics, chartArea);

                    // Draw axes
                    DrawAxes(graphics, chartArea, chartDef);

                    // Draw grid
                    DrawGrid(graphics, chartArea, chartDef);

                    // Draw percentile curves
                    foreach (var curve in chartDef.PercentileCurves)
                    {
                        DrawPercentileCurve(graphics, chartArea, curve, chartDef);
                    }

                    // Draw patient measurements
                    DrawMeasurements(graphics, chartArea, measurements, chartDef);

                    // Draw legend
                    DrawLegend(graphics, new Rectangle(
                        chartArea.Right + 10,
                        chartArea.Top,
                        LegendWidth - 20,
                        chartArea.Height),
                        chartDef);

                    // Save to memory stream
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating growth chart");
                throw;
            }
        }

        public async Task<byte[]> GenerateMultiChartReportAsync(
            int patientId,
            List<MeasurementType> measurementTypes,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                // Calculate total height needed for all charts
                int chartHeight = 600;
                int totalHeight = (chartHeight * measurementTypes.Count) + (Padding * (measurementTypes.Count + 1));
                int width = 800;

                using (var bitmap = new Bitmap(width, totalHeight))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.White);

                    int currentY = Padding;

                    foreach (var measurementType in measurementTypes)
                    {
                        // Get chart definition and measurements for this type
                        var chartDef = await _repository.GetChartDefinitionAsync(measurementType, "M", 0, 60); // Example parameters
                        var measurements = await _repository.GetMeasurementsAsync(patientId, measurementType, startDate, endDate);

                        // Create chart area for this measurement type
                        var chartArea = new Rectangle(Padding, currentY, width - (Padding * 2), chartHeight);

                        // Generate individual chart
                        await GenerateGrowthChartAsync(chartDef, measurements, width, chartHeight);

                        currentY += chartHeight + Padding;
                    }

                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating multi-chart report for patient {PatientId}", patientId);
                throw;
            }
        }

        private void DrawChartBackground(Graphics g, Rectangle area)
        {
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillRectangle(brush, area);
            }
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawRectangle(pen, area);
            }
        }

        private void DrawAxes(Graphics g, Rectangle area, ChartDefinition chartDef)
        {
            using (var pen = new Pen(Color.Black, 1))
            {
                // X axis
                g.DrawLine(pen, area.Left, area.Bottom, area.Right, area.Bottom);

                // Y axis
                g.DrawLine(pen, area.Left, area.Top, area.Left, area.Bottom);
            }

            // Draw labels
            using (var font = new Font("Arial", 8))
            {
                // X axis label
                g.DrawString(chartDef.XAxisLabel,
                    font,
                    Brushes.Black,
                    area.Left + (area.Width / 2),
                    area.Bottom + 20,
                    new StringFormat { Alignment = StringAlignment.Center });

                // Y axis label
                using (var matrix = new Matrix())
                {
                    matrix.RotateAt(-90, new PointF(area.Left - 30, area.Top + (area.Height / 2)));
                    g.Transform = matrix;
                    g.DrawString(chartDef.YAxisLabel,
                        font,
                        Brushes.Black,
                        area.Left - 30,
                        area.Top + (area.Height / 2),
                        new StringFormat { Alignment = StringAlignment.Center });
                    g.ResetTransform();
                }
            }
        }

        private void DrawGrid(Graphics g, Rectangle area, ChartDefinition chartDef)
        {
            using (var pen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dot })
            {
                // Draw vertical grid lines (age)
                int ageStep = GetAgeGridStep(chartDef.MaxAgeMonths - chartDef.MinAgeMonths);
                for (int age = chartDef.MinAgeMonths; age <= chartDef.MaxAgeMonths; age += ageStep)
                {
                    float x = MapAgeToX(age, chartDef.MinAgeMonths, chartDef.MaxAgeMonths, area);
                    g.DrawLine(pen, x, area.Top, x, area.Bottom);
                }

                // Draw horizontal grid lines (measurement values)
                var valueRange = GetValueRange(chartDef.PercentileCurves);
                decimal valueStep = GetValueGridStep(valueRange.max - valueRange.min);
                for (decimal value = valueRange.min; value <= valueRange.max; value += valueStep)
                {
                    float y = MapValueToY(value, valueRange.min, valueRange.max, area);
                    g.DrawLine(pen, area.Left, y, area.Right, y);
                }
            }
        }

        private void DrawPercentileCurve(Graphics g, Rectangle area, PercentileCurve curve, ChartDefinition chartDef)
        {
            var valueRange = GetValueRange(chartDef.PercentileCurves);
            var points = curve.Points.Select(p => new PointF(
                MapAgeToX(p.AgeMonths, chartDef.MinAgeMonths, chartDef.MaxAgeMonths, area),
                MapValueToY(p.Value, valueRange.min, valueRange.max, area)
            )).ToArray();

            var percentileDef = chartDef.Percentiles.First(p => p.Value == curve.Percentile);
            using (var pen = new Pen(ColorTranslator.FromHtml(percentileDef.Color), percentileDef.IsMainLine ? 2 : 1))
            {
                if (percentileDef.LineStyle == "dashed")
                {
                    pen.DashStyle = DashStyle.Dash;
                }
                g.DrawLines(pen, points);
            }
        }

        private void DrawMeasurements(Graphics g, Rectangle area, List<GrowthMeasurement> measurements, ChartDefinition chartDef)
        {
            var valueRange = GetValueRange(chartDef.PercentileCurves);
            
            foreach (var measurement in measurements)
            {
                var age = CalculateAgeInMonths(measurement.MeasurementDate);
                var point = new PointF(
                    MapAgeToX(age, chartDef.MinAgeMonths, chartDef.MaxAgeMonths, area),
                    MapValueToY(measurement.Value, valueRange.min, valueRange.max, area)
                );

                // Draw point
                using (var brush = new SolidBrush(Color.Blue))
                {
                    g.FillEllipse(brush, point.X - 3, point.Y - 3, 6, 6);
                }
            }

            // Draw lines between points if more than one measurement
            if (measurements.Count > 1)
            {
                var sortedMeasurements = measurements.OrderBy(m => m.MeasurementDate).ToList();
                var points = sortedMeasurements.Select(m => new PointF(
                    MapAgeToX(CalculateAgeInMonths(m.MeasurementDate), chartDef.MinAgeMonths, chartDef.MaxAgeMonths, area),
                    MapValueToY(m.Value, valueRange.min, valueRange.max, area)
                )).ToArray();

                using (var pen = new Pen(Color.Blue, 1) { DashStyle = DashStyle.Dash })
                {
                    g.DrawLines(pen, points);
                }
            }
        }

        private void DrawLegend(Graphics g, Rectangle area, ChartDefinition chartDef)
        {
            int y = area.Top;
            using (var font = new Font("Arial", 8))
            {
                foreach (var percentile in chartDef.Percentiles.OrderByDescending(p => p.Value))
                {
                    // Draw line sample
                    using (var pen = new Pen(ColorTranslator.FromHtml(percentile.Color), percentile.IsMainLine ? 2 : 1))
                    {
                        if (percentile.LineStyle == "dashed")
                        {
                            pen.DashStyle = DashStyle.Dash;
                        }
                        g.DrawLine(pen, area.Left, y + 7, area.Left + 20, y + 7);
                    }

                    // Draw label
                    g.DrawString($"{percentile.Label} percentile",
                        font,
                        Brushes.Black,
                        area.Left + 25,
                        y);

                    y += 20;
                }

                // Draw patient data point example
                y += 10;
                using (var brush = new SolidBrush(Color.Blue))
                {
                    g.FillEllipse(brush, area.Left + 7, y + 4, 6, 6);
                }
                g.DrawString("Patient measurement",
                    font,
                    Brushes.Black,
                    area.Left + 25,
                    y);
            }
        }

        private float MapAgeToX(int age, int minAge, int maxAge, Rectangle area)
        {
            return area.Left + (float)(age - minAge) / (maxAge - minAge) * area.Width;
        }

        private float MapValueToY(decimal value, decimal minValue, decimal maxValue, Rectangle area)
        {
            return area.Bottom - (float)(value - minValue) / (float)(maxValue - minValue) * area.Height;
        }

        private (decimal min, decimal max) GetValueRange(List<PercentileCurve> curves)
        {
            var allValues = curves.SelectMany(c => c.Points.Select(p => p.Value));
            return (allValues.Min(), allValues.Max());
        }

        private int GetAgeGridStep(int ageRange)
        {
            if (ageRange <= 24) return 3;
            if (ageRange <= 60) return 6;
            if (ageRange <= 120) return 12;
            return 24;
        }

        private decimal GetValueGridStep(decimal valueRange)
        {
            if (valueRange <= 10) return 1;
            if (valueRange <= 50) return 5;
            if (valueRange <= 100) return 10;
            return 20;
        }

        private int CalculateAgeInMonths(DateTime measurementDate)
        {
            // This should be calculated based on patient's birth date
            // For now, returning a placeholder
            return 0;
        }
    }
}
