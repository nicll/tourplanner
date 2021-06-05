using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using log4net;
using TourPlanner.Core.Exceptions;

namespace TourPlanner.DataProviders.MapQuest
{
    internal abstract class MapQuestAPI
    {
        internal const string
            RelativeImageContainerPath = @".\TourPlanner_CachedImages\",
            RelativeMapImagesPath = RelativeImageContainerPath + @"Maps\",
            RelativeIconImagesPath = RelativeImageContainerPath + @"Icons\";
        protected static readonly ILog _log = LogManager.GetLogger(typeof(MapQuestAPI));
        protected readonly JsonSerializerOptions _jsonOpts = new() { IncludeFields = true, PropertyNameCaseInsensitive = true, Converters = { new RouteConverter() } };
        protected readonly HttpClient _client;
        protected readonly string _apiKey;
        protected readonly TimeSpan _timeout;

        internal MapQuestAPI(string apiKey, TimeSpan timeout, HttpClient client)
        {
            _client = client;
            _apiKey = apiKey;
            _timeout = timeout;
        }

        internal static void EnsureDirectoryExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    _log.Info("Created directory: " + path);
                }
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while creating a directory: " + path, ex);
                throw new DataProviderExcpetion("An error occured while creating a directory: " + path, ex);
            }
        }
    }
}
