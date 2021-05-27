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
        public async Task GenerateTourReport(Tour tour, string savePath)
        {
            var document = new TourReportDocument(tour);
            await document.LoadImage();
            using var fileStream = File.Create(savePath);
            document.GeneratePdf(fileStream);
        }

        public Task GenerateSummaryReport(ICollection<Tour> tours, string savePath)
        {
            var document = new SummaryReportDocument(tours);
            using var fileStream = File.Create(savePath);
            document.GeneratePdf(fileStream);
            return Task.CompletedTask;
        }

        internal static string DistanceToString(double distance)
            => distance < 1 ? (distance * 1000).ToString("0") + " m" : distance.ToString("0.00") + " km";
    }
}
