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

        public SummaryReportDocument(ICollection<Tour> toursModel)
            => _toursModel = toursModel;

        public DocumentMetadata GetMetadata()
            => new() { Title = "TourPlanner - Summary Report", PdfA = true };

        public void Compose(IContainer container)
        {
            container
                .Padding(50)
                .Page(page =>
                {
                    page.Header(ComposeHeader);
                    page.Content(ComposeContent);
                    page.Footer(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Stack(stack =>
                {
                    stack.Element().Text("Summary of All Tours", TextStyle.Default.Size(20));
                    stack.Element().Text("Listing " + _toursModel.Count + " tours.");
                });

                row.ConstantColumn(100).Height(50).Placeholder();
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).PageableStack(stack =>
            {
                stack.Spacing(5);
                stack.Element(ComposeListing);
                stack.Element(ComposeTotals);
            });
        }

        private void ComposeListing(IContainer container)
        {
            container.PaddingVertical(10).Section(section =>
            {
                section.Header().BorderBottom(1).Padding(5).Row(row =>
                {
                    row.RelativeColumn(4).AlignLeft().Text("Name");
                    row.RelativeColumn(1).AlignRight().Text("Steps");
                    row.RelativeColumn(1).AlignRight().Text("Total Distance");
                });

                section.Content().PageableStack(stack =>
                {
                    foreach (var tour in _toursModel)
                    {
                        stack.Element().BorderBottom(1).BorderColor("CCC").Padding(5).Row(row =>
                        {
                            row.RelativeColumn(4).AlignLeft().Text(tour.Name);
                            row.RelativeColumn(1).AlignRight().Text(tour.Route.Steps.Count);
                            row.RelativeColumn(1).AlignRight().Text(ReportGenerator.DistanceToString(tour.Route.TotalDistance));
                        });
                    }
                });
            });
        }

        private void ComposeTotals(IContainer container)
        {
            container.Background("EEE").Padding(10).Stack(stack =>
            {
                stack.Spacing(5);
                stack.Element().Text("Statistical Summary", TextStyle.Default.Size(14));

                if (!_toursModel.Any())
                {
                    stack.Element().Text("No tours available.");
                    return;
                }

                stack.Element().Text("Mean number of steps: " + _toursModel.Average(t => t.Route.Steps.Count).ToString("0"));
                stack.Element().Text("Total number of steps: " + _toursModel.Sum(t => t.Route.Steps.Count));
                stack.Element().Text("Shortest tour: " + _toursModel.Aggregate((min, t) => min.Route.TotalDistance < t.Route.TotalDistance ? min : t).Name);
                stack.Element().Text("Longest tour: " + _toursModel.Aggregate((max, t) => max.Route.TotalDistance > t.Route.TotalDistance ? max : t).Name);
                stack.Element().Text("Summed total distance: " + ReportGenerator.DistanceToString(_toursModel.Sum(t => t.Route.TotalDistance)));
            });
        }

        private void ComposeFooter(IContainer container)
            => container.AlignRight().PageNumber("Page {number}");
    }
}
