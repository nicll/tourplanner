using System;
using System.IO;
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
                    stack.Element().Text(_tourModel.Name, TextStyle.Default.Size(20));
                    stack.Element().Text("TourId: " + _tourModel.TourId);
                    stack.Element().Text("RouteId: " + _tourModel.Route.RouteId);
                    stack.Element().Text("Total distance: " + ReportGenerator.DistanceToString(_tourModel.Route.TotalDistance));
                });

                row.ConstantColumn(160).Height(80).Image(_imageData, ImageScaling.FitArea);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).PageableStack(stack =>
            {
                stack.Spacing(5);
                stack.Element(ComposeDescription);
                stack.Element(ComposeSteps);
            });
        }

        private void ComposeDescription(IContainer container)
        {
            if (String.IsNullOrEmpty(_tourModel.CustomDescription))
                return;

            container.Background("EEE").Padding(10).Stack(stack =>
            {
                stack.Spacing(5);
                stack.Element().Text("Description", TextStyle.Default.Size(16));
                stack.Element().Text(_tourModel.CustomDescription);
            });
        }

        private void ComposeSteps(IContainer container)
        {
            container.PaddingVertical(10).Section(section =>
            {
                section.Header().BorderBottom(1).Padding(5).Row(row =>
                {
                    row.ConstantColumn(25).Text("#");
                    row.RelativeColumn(3).AlignLeft().Text("Description");
                    row.RelativeColumn(1).AlignRight().Text("Distance");
                });

                section.Content().PageableStack(stack =>
                {
                    for (int i = 0; i < _tourModel.Route.Steps.Count; ++i)
                    {
                        stack.Element().BorderBottom(1).BorderColor("CCC").Padding(5).Row(row =>
                        {
                            row.ConstantColumn(25).Text(i + 1);
                            row.RelativeColumn(3).AlignLeft().Text(_tourModel.Route.Steps[i].Description);
                            row.RelativeColumn(1).AlignRight().Text(ReportGenerator.DistanceToString(_tourModel.Route.Steps[i].Distance));
                        });
                    }

                    stack.Element().Padding(5).AlignRight().Text("Total distance: " + ReportGenerator.DistanceToString(_tourModel.Route.TotalDistance), TextStyle.Default.Size(16));
                });
            });
        }

        private void ComposeFooter(IContainer container)
            => container.AlignRight().PageNumber("Page {number}");
    }
}
