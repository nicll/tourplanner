using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using TourPlanner.Core.Models;

namespace TourPlanner.Reporting.PDF
{
    public static class ReportGenerator
    {
        public static void GenerateTourReport(Tour tour, string savePath)
        {
            var document = new TourReportDocument(tour);
            document.GeneratePdf(savePath);
        }

        public static void GenerateSummaryReport(ICollection<Tour> tours, string savePath)
        {
            var document = new SummaryReport(tours);
            document.GeneratePdf(savePath);
        }
    }
}
