using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DataProviders.MapQuest
{
    public class MapQuestApiFactory : IDataProviderFactory
    {
        private static HttpClient _client = new(); // currently used HttpClient
        private readonly Dictionary<ApiClientConfig, Directions> _cachedDirections = new();
        private readonly Dictionary<ApiClientConfig, StaticMap> _cachedStaticMaps = new();

        public ValueTask<IDirectionsProvider> CreateDirectionsProvider(ApiClientConfig config)
        {
            if (_cachedDirections.TryGetValue(config, out var provider))
                return ValueTask.FromResult((IDirectionsProvider)provider);

            provider = new Directions(config.ApiKey, config.Timeout, _client);
            _cachedDirections.Add(config, provider);

            return ValueTask.FromResult((IDirectionsProvider)provider);
        }

        public ValueTask<IMapImageProvider> CreateMapImageProvider(ApiClientConfig config)
        {
            if (_cachedStaticMaps.TryGetValue(config, out var provider))
                return ValueTask.FromResult((IMapImageProvider)provider);

            provider = new StaticMap(config.ApiKey, config.Timeout, _client);
            _cachedStaticMaps.Add(config, provider);

            return ValueTask.FromResult((IMapImageProvider)provider);
        }

        public void ResetConnection()
        {
            var oldClient = _client;
            _client = new();

            // cached providers still use the old instance
            _cachedDirections.Clear();
            _cachedStaticMaps.Clear();

            // previously created providers may no longer be used
            oldClient.Dispose();
        }
    }
}
