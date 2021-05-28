using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TourPlanner.Core.Models;

namespace TourPlanner.Reporting.PDF
{
    internal class TourReportDocument : IDocument
    {
        private readonly Tour _tourModel;
        private byte[] _imageData;

        public TourReportDocument(Tour tourModel)
            => _tourModel = tourModel;

        public async Task LoadImage()
            => _imageData = await File.ReadAllBytesAsync(_tourModel.ImagePath).ConfigureAwait(false);

        public DocumentMetadata GetMetadata()
            => new() { Title = "TourPlanner - Tour Report", PdfA = true };

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
                    stack.Item().Text(_tourModel.Name, TextStyle.Default.Size(20));
                    stack.Item().Text("TourId: " + _tourModel.TourId);
                    stack.Item().Text("RouteId: " + _tourModel.Route.RouteId);
                    stack.Item().Text("Total distance: " + ReportGenerator.DistanceToString(_tourModel.Route.TotalDistance));
                });

                row.ConstantColumn(160).Height(80).Image(_imageData, ImageScaling.FitArea);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Stack(stack =>
            {
                stack.Spacing(5);
                stack.Item().Element(ComposeDescription);
                stack.Item().Element(ComposeSteps);
                stack.Item().Element(ComposeLog);
            });
        }

        private void ComposeDescription(IContainer container)
        {
            if (String.IsNullOrEmpty(_tourModel.CustomDescription))
                return;

            container.Background("EEE").Padding(10).Stack(stack =>
            {
                stack.Spacing(5);
                stack.Item().Text("Description", TextStyle.Default.Size(16));
                stack.Item().Text(_tourModel.CustomDescription);
            });
        }

        private void ComposeSteps(IContainer container)
        {
            container.PaddingVertical(10).Decoration(decoration =>
            {
                decoration.Header().BorderBottom(1).Padding(5).Row(row =>
                {
                    row.ConstantColumn(25).Text("#");
                    row.RelativeColumn(3).AlignLeft().Text("Description");
                    row.RelativeColumn(1).AlignRight().Text("Distance");
                });

                decoration.Content().Stack(stack =>
                {
                    for (int i = 0; i < _tourModel.Route.Steps.Count; ++i)
                    {
                        stack.Item().BorderBottom(1).BorderColor("CCC").Padding(5).Row(row =>
                        {
                            row.ConstantColumn(25).Text(i + 1);
                            row.RelativeColumn(3).AlignLeft().Text(_tourModel.Route.Steps[i].Description);
                            row.RelativeColumn(1).AlignRight().Text(ReportGenerator.DistanceToString(_tourModel.Route.Steps[i].Distance));
                        });
                    }

                    stack.Item().Padding(5).AlignRight().Text("Total distance: " + ReportGenerator.DistanceToString(_tourModel.Route.TotalDistance), TextStyle.Default.Size(16));
                });
            });
        }

        private void ComposeLog(IContainer container)
        {
            if (!_tourModel.Log.Any())
                return;

            container.PaddingVertical(10).Decoration(section =>
            {
                section.Header().BorderBottom(1).Padding(5).Row(row =>
                {
                    row.RelativeColumn().AlignRight().Text("Date");
                    row.RelativeColumn().AlignRight().Text("Distance");
                    row.RelativeColumn().AlignRight().Text("Duration");
                    row.RelativeColumn().AlignRight().Text("Rating");
                    row.RelativeColumn().AlignRight().Text("Participants");
                    row.RelativeColumn().AlignRight().Text("Vehicle");
                });

                section.Content().Stack(stack =>
                {
                    foreach (var log in _tourModel.Log)
                    {
                        stack.Item().Padding(5).Row(row =>
                        {
                            row.RelativeColumn().AlignRight().Text(log.Date.ToShortDateString());
                            row.RelativeColumn().AlignRight().Text(log.Duration);
                            row.RelativeColumn().AlignRight().Text(ReportGenerator.DistanceToString(log.Distance));
                            row.RelativeColumn().AlignRight().Text(log.Rating.ToString("P0"));
                            row.RelativeColumn().AlignRight().Text(log.ParticipantCount);
                            row.RelativeColumn().AlignRight().Text(log.Vehicle);
                        });

                        if (!String.IsNullOrEmpty(log.Notes))
                            stack.Item().AlignCenter().Padding(5).PaddingTop(0).Text(log.Notes);

                        stack.Item().BorderBottom(1).BorderColor("CCC");
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
            => container.AlignRight().PageNumber("Page {number}");
    }
}
