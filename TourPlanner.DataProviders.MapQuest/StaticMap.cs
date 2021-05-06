using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DataProviders.MapQuest
{
    internal class StaticMap : MapQuestAPI, IMapImageProvider
    {
        private const string ImageRequestFormat = "https://www.mapquestapi.com/staticmap/v5/map?key={0}&session={1}&size=1280,720&format=png";
        private const string RelativeImageContainerPath = @".\TourPlanner_CachedImages\";

        internal StaticMap(string apiKey, TimeSpan timeout, HttpClient client) : base(apiKey, timeout, client)
        {
        }

        public async ValueTask<string> GetImage(Route route)
        {
            EnsureDirectoryExists(RelativeImageContainerPath);

            var imagePath = GetAbsoluteRouteImagePath(route);

            if (!File.Exists(imagePath))
                await DownloadImage(route).ConfigureAwait(false);

            return imagePath;
        }

        public ValueTask ClearCache()
        {
            foreach (var filePath in Directory.EnumerateFiles(RelativeImageContainerPath))
            {
                File.Delete(filePath);
                _log.Debug("Deleted file in image cache: " + filePath);
            }

            _log.Info("Cleared image cache.");
            return ValueTask.CompletedTask;
        }

        private async Task DownloadImage(Route route)
        {
            _log.Debug("Requesting image for route from API: RouteId=\"" + route.RouteId + "\"");
            var imagePath = GetAbsoluteRouteImagePath(route);
            _log.Debug("Image will be saved to: " + imagePath);

            using var cts = new CancellationTokenSource(_timeout);
            var image = await _client.GetByteArrayAsync(String.Format(ImageRequestFormat, _apiKey, route.RouteId), cts.Token);
            _log.Debug("Received response from API.");

            await File.WriteAllBytesAsync(imagePath, image);
            _log.Info("Saved image for route \"" + route.RouteId + "\" in \"" + imagePath + "\".");
        }

        private static string GetRouteImageFilename(Route route)
        {
            // to keep the implementation simple we will use the session id as the filename
            // however we could also use info such as source and target destinations...

            // for some reason the session id is not a Guid...
            return route.RouteId + ".png";
        }

        private static string GetAbsoluteRouteImagePath(Route route)
            => Path.GetFullPath(RelativeImageContainerPath + GetRouteImageFilename(route));

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
