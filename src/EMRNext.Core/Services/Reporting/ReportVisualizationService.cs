using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Infrastructure.Reporting;
using SkiaSharp;

namespace EMRNext.Core.Services.Reporting
{
    public class ReportVisualizationService
    {
        // Visualization Types
        public enum VisualizationType
        {
            Bar,
            Line,
            Pie,
            Scatter
        }

        // Visualization Configuration
        public class VisualizationConfig
        {
            public VisualizationType Type { get; set; }
            public string Title { get; set; }
            public Dictionary<string, object> Data { get; set; }
            public Dictionary<string, string> Colors { get; set; }
        }

        // Generate Visualization
        public byte[] GenerateVisualization(VisualizationConfig config)
        {
            using (var surface = SKSurface.Create(new SKImageInfo(800, 600)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                switch (config.Type)
                {
                    case VisualizationType.Bar:
                        RenderBarChart(canvas, config);
                        break;
                    case VisualizationType.Line:
                        RenderLineChart(canvas, config);
                        break;
                    case VisualizationType.Pie:
                        RenderPieChart(canvas, config);
                        break;
                    case VisualizationType.Scatter:
                        RenderScatterPlot(canvas, config);
                        break;
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return data.ToArray();
                }
            }
        }

        // Render Bar Chart
        private void RenderBarChart(SKCanvas canvas, VisualizationConfig config)
        {
            var paint = new SKPaint
            {
                Color = SKColors.Blue,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            var data = config.Data.Values.Cast<double>().ToArray();
            var labels = config.Data.Keys.ToArray();

            float width = 800f / data.Length;
            float scale = 500f / data.Max();

            for (int i = 0; i < data.Length; i++)
            {
                float height = (float)data[i] * scale;
                canvas.DrawRect(
                    new SKRect(
                        i * width, 
                        600 - height, 
                        (i + 1) * width, 
                        600
                    ), 
                    paint
                );
            }
        }

        // Render Line Chart
        private void RenderLineChart(SKCanvas canvas, VisualizationConfig config)
        {
            var paint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3
            };

            var data = config.Data.Values.Cast<double>().ToArray();
            float width = 800f / (data.Length - 1);
            float scale = 500f / data.Max();

            var path = new SKPath();
            path.MoveTo(0, 600 - (float)data[0] * scale);

            for (int i = 1; i < data.Length; i++)
            {
                path.LineTo(
                    i * width, 
                    600 - (float)data[i] * scale
                );
            }

            canvas.DrawPath(path, paint);
        }

        // Render Pie Chart
        private void RenderPieChart(SKCanvas canvas, VisualizationConfig config)
        {
            var data = config.Data.Values.Cast<double>().ToArray();
            var total = data.Sum();
            var colors = config.Colors?.Select(c => SKColor.Parse(c.Value)).ToArray() 
                ?? new[] { SKColors.Blue, SKColors.Red, SKColors.Green };

            float startAngle = 0;
            for (int i = 0; i < data.Length; i++)
            {
                var sweepAngle = (float)(data[i] / total * 360);
                var paint = new SKPaint
                {
                    Color = colors[i % colors.Length],
                    Style = SKPaintStyle.Fill
                };

                canvas.DrawArc(
                    new SKRect(200, 100, 600, 500), 
                    startAngle, 
                    sweepAngle, 
                    true, 
                    paint
                );

                startAngle += sweepAngle;
            }
        }

        // Render Scatter Plot
        private void RenderScatterPlot(SKCanvas canvas, VisualizationConfig config)
        {
            var paint = new SKPaint
            {
                Color = SKColors.Purple,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            var xValues = config.Data["X"] as double[];
            var yValues = config.Data["Y"] as double[];

            float xScale = 700f / xValues.Max();
            float yScale = 500f / yValues.Max();

            for (int i = 0; i < xValues.Length; i++)
            {
                canvas.DrawCircle(
                    (float)xValues[i] * xScale + 50, 
                    600 - (float)yValues[i] * yScale, 
                    5, 
                    paint
                );
            }
        }
    }
}
