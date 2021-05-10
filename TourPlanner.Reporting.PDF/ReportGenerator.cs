using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.Reporting.PDF
{
    public class ReportGenerator : IReportGenerator
    {
        public Task GenerateTourReport(Tour tour, string savePath)
        {
            var document = new TourReportDocument(tour);
            using var fileStream = File.Create(savePath);
            document.GeneratePdf(fileStream);
            return Task.CompletedTask;
        }

        public Task GenerateSummaryReport(ICollection<Tour> tours, string savePath)
        {
            var document = new SummaryReport(tours);
            using var fileStream = File.Create(savePath);
            document.GeneratePdf(fileStream);
            return Task.CompletedTask;
        }
    }
}
