using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Models;
using log4net;
using TourPlanner.Converters.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TourPlanner.GUI
{
    internal static class OSInteraction
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(OSInteraction));
        private static readonly JsonConverter _jsonConverter = new();

        public static Config LoadConfig(string configFilePath)
        {
            _log.Debug("Now loading config file \"" + configFilePath + "\".");
            var configFile = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddXmlFile(configFilePath, false)
                .Build();

            var section = configFile.GetSection(nameof(TourPlanner));
            _log.Info("Loaded config file \"" + configFilePath + "\".");

            return new Config
            {
                DirectionsApiConfig = section.GetSection(nameof(Config.DirectionsApiConfig)).Get<ApiClientConfig>(),
                MapImageApiConfig = section.GetSection(nameof(Config.MapImageApiConfig)).Get<ApiClientConfig>(),
                DatabaseConfig = section.GetSection(nameof(Config.DatabaseConfig)).Get<DbClientConfig>()
            };
        }

        public static string GetSaveFilePath(string suggestedName, string extension, string filter)
        {
            var saveFileDlg = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = extension,
                FileName = suggestedName,
                Filter = filter,
                OverwritePrompt = true,
                ValidateNames = true
            };

            if (saveFileDlg.ShowDialog() is true)
                return saveFileDlg.FileName;

            return String.Empty;
        }

        public static string GetOpenFilePath(string extension, string filter)
        {
            var saveFileDlg = new OpenFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = extension,
                Filter = filter,
                ValidateNames = true
            };

            if (saveFileDlg.ShowDialog() is true)
                return saveFileDlg.FileName;

            return String.Empty;
        }

        public static async Task<Tour[]> ImportToursFromFile(string path)
        {
            using var file = File.OpenRead(path);
            return (await _jsonConverter.ReadTours(file)).ToArray();
        }

        public static async Task ExportToursToFile(string path, ICollection<Tour> tours)
        {
            using var file = File.OpenWrite(path);
            await _jsonConverter.WriteTours(file, tours);
        }

        public static void ShowFile(string filePath)
            => new Process { StartInfo = new() { FileName = filePath, UseShellExecute = true } }.Start();
    }
}
