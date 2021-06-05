using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TourPlanner.Core.Exceptions;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DataProviders.MapQuest
{
    internal class StaticMap : MapQuestAPI, IMapImageProvider
    {
        private const string ImageRequestFormat = "https://www.mapquestapi.com/staticmap/v5/map?key={0}&session={1}&size=1280,720&format=png";

        internal StaticMap(string apiKey, TimeSpan timeout, HttpClient client) : base(apiKey, timeout, client)
        { }

        public async ValueTask<string> GetImage(Route route)
        {
            EnsureDirectoryExists(RelativeMapImagesPath);

            var imagePath = GetRelativeRouteImagePath(route);

            if (!File.Exists(imagePath))
                await DownloadImage(route).ConfigureAwait(false);

            return imagePath;
        }

        public ValueTask CleanCache(IDataManager dataManager)
        {
            try
            {
                EnsureDirectoryExists(RelativeMapImagesPath);

                foreach (var filePath in Directory.EnumerateFiles(RelativeMapImagesPath))
                {
                    var fileRouteId = Path.GetFileNameWithoutExtension(filePath);

                    if (dataManager.AllTours.Any(t => t.Route.RouteId == fileRouteId))
                        continue; // skip if still exists

                    File.Delete(filePath);
                    _log.Debug("Deleted file in image cache: " + filePath);
                }

                _log.Info("Cleared map image cache.");
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while cleaning the map image cache.", ex);
                throw new DataProviderExcpetion("An error occured while cleaning the map image cache.", ex);
            }

            return ValueTask.CompletedTask;
        }

        private async Task DownloadImage(Route route)
        {
            try
            {
                _log.Debug("Requesting image for route from API: RouteId=\"" + route.RouteId + "\"");
                var imagePath = GetRelativeRouteImagePath(route);
                _log.Debug("Image will be saved to: " + imagePath);

                using var cts = new CancellationTokenSource(_timeout);
                var response = await _client.GetAsync(String.Format(ImageRequestFormat, _apiKey, route.RouteId), cts.Token);
                _log.Debug("Received response from API.");
                response.EnsureSuccessStatusCode();
                var image = await response.Content.ReadAsByteArrayAsync();

                await File.WriteAllBytesAsync(imagePath, image);
                _log.Info("Saved image for route \"" + route.RouteId + "\" in \"" + imagePath + "\".");
            }
            catch (Exception ex)
            {
                _log.Error("An error occured while fetching a map image.", ex);
                throw new DataProviderExcpetion("An error occured while fetching a map image.", ex);
            }
        }

        private static string GetRouteImageFilename(Route route)
        {
            // to keep the implementation simple we will use the session id as the filename
            // however we could also use info such as source and target destinations...

            // for some reason the session id is not a Guid...
            return route.RouteId + ".png";
        }

        private static string GetRelativeRouteImagePath(Route route)
            => RelativeMapImagesPath + GetRouteImageFilename(route);
    }
}
