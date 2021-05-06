using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Models;
using log4net;

namespace TourPlanner.GUI
{
    internal static class Utils
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Utils));

        public static Configuration LoadConfig(string configFilePath)
        {
            _log.Debug("Now loading config file \"" + configFilePath + "\".");
            var configFile = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddXmlFile(configFilePath)
                .Build();

            var section = configFile.GetSection(nameof(TourPlanner));
            _log.Info("Loaded config file \"" + configFilePath + "\".");

            return new Configuration
            {
                DirectionsApiConfig = section.GetSection(nameof(Configuration.DirectionsApiConfig)).Get<ApiClientConfig>(),
                MapImageApiConfig = section.GetSection(nameof(Configuration.MapImageApiConfig)).Get<ApiClientConfig>(),
                DatabaseConfig = section.GetSection(nameof(Configuration.DatabaseConfig)).Get<DbClientConfig>()
            };
        }

        public static string GetReportSavePath(string suggestedName)
        {
            var saveFileDlg = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = "pdf",
                FileName = suggestedName,
                Filter = "Portable document files (*.pdf)|*.pdf",
                OverwritePrompt = true,
                ValidateNames = true
            };

            if (saveFileDlg.ShowDialog() is true)
                return saveFileDlg.FileName;

            return String.Empty;
        }

        public static void ShowFile(string filePath)
            => new Process { StartInfo = new() { FileName = filePath, UseShellExecute = true } }.Start();
    }
}
