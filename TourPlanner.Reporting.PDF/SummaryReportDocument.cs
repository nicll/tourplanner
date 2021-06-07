using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using TourPlanner.Core.Models;

namespace TourPlanner.Reporting.PDF
{
    internal class SummaryReportDocument : IDocument
    {
        private readonly ICollection<Tour> _toursModel;
        private readonly LogEntry[] _logsModel; // for fast access

        public SummaryReportDocument(ICollection<Tour> toursModel)
        {
            _toursModel = toursModel;
            _logsModel = toursModel.SelectMany(t => t.Log).ToArray();
        }

        public DocumentMetadata GetMetadata()
            => new() { Title = "TourPlanner - Summary Report", PdfA = true };

        public void Compose(IContainer container)
        {
            container
                .Padding(50)
                .Page(page =>
                {
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Stack(stack =>
                {
                    stack.Item().Text("Summary of all tours", TextStyle.Default.Size(20));
                    stack.Item().Text("Listing " + _toursModel.Count + " tours and " + _logsModel.Length + " logs.");
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Stack(stack =>
            {
                stack.Spacing(5);
                stack.Item().Element(ComposeTourListing);
                stack.Item().Element(ComposeTourTotals);
                stack.Item().Element(ComposeLogTotals);
            });
        }

        private void ComposeTourListing(IContainer container)
        {
            container.PaddingVertical(10).Decoration(decoration =>
            {
                decoration.Header().BorderBottom(1).Padding(5).Row(row =>
                {
                    row.RelativeColumn(4).AlignLeft().Text("Name");
                    row.RelativeColumn(1).AlignRight().Text("Steps");
                    row.RelativeColumn(1).AlignRight().Text("Total Distance");
                });

                decoration.Content().Stack(stack =>
                {
                    foreach (var tour in _toursModel)
                    {
                        stack.Item().BorderBottom(1).BorderColor("CCC").Padding(5).Row(row =>
                        {
                            row.RelativeColumn(4).AlignLeft().Text(tour.Name);
                            row.RelativeColumn(1).AlignRight().Text(tour.Route.Steps.Count);
                            row.RelativeColumn(1).AlignRight().Text(ReportGenerator.DistanceToString(tour.Route.TotalDistance));
                        });
                    }
                });
            });
        }

        private void ComposeTourTotals(IContainer container)
        {
            container.Background("EEE").Padding(10).Stack(stack =>
            {
                stack.Spacing(5);
                stack.Item().Text("Statistical Summary of Tours", TextStyle.Default.Size(14));

                if (!_toursModel.Any())
                {
                    stack.Item().Text("No tours available.");
                    return;
                }

                stack.Item().Text("Mean number of steps: " + _toursModel.Average(t => t.Route.Steps.Count).ToString("0"));
                stack.Item().Text("Total number of steps: " + _toursModel.Sum(t => t.Route.Steps.Count));
                stack.Item().Text("Shortest tour: " + _toursModel.Aggregate((min, t) => min.Route.TotalDistance < t.Route.TotalDistance ? min : t).Name);
                stack.Item().Text("Longest tour: " + _toursModel.Aggregate((max, t) => max.Route.TotalDistance > t.Route.TotalDistance ? max : t).Name);
                stack.Item().Text("Summed total distance: " + ReportGenerator.DistanceToString(_toursModel.Sum(t => t.Route.TotalDistance)));
            });
        }

        private void ComposeLogTotals(IContainer container)
        {
            container.Background("EEE").Padding(10).Stack(stack =>
            {
                stack.Spacing(5);
                stack.Item().Text("Statistical Summary of Tour Logs", TextStyle.Default.Size(14));

                if (!_logsModel.Any())
                {
                    stack.Item().Text("No logs available.");
                    return;
                }

                stack.Item().Text("Total number of logs: " + _logsModel.Length);
                stack.Item().Text("Total time spent on tours: " + TimeSpan.FromSeconds(_logsModel.Sum(l => l.Duration.TotalSeconds)));
                stack.Item().Text("Total distance travelled: " + ReportGenerator.DistanceToString(_logsModel.Sum(l => l.Distance)));
                stack.Item().Text("Average rating of all tours: " + _logsModel.Average(l => l.Rating).ToString("P0"));
                stack.Item().Text("Average amount of energy used: " + _logsModel.Average(l => l.EnergyUsed) + " kWh");
            });
        }

        private void ComposeFooter(IContainer container)
            => container.AlignRight().PageNumber("Page {number}");
    }
}
