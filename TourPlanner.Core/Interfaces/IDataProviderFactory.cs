using System;
using System.Net.Http;
using System.Threading.Tasks;
using TourPlanner.Core.Exceptions;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    public interface IDataProviderFactory
    {
        /// <summary>
        /// Get an instance of <see cref="IDirectionsProvider"/>.
        /// This instance may be cached.
        /// </summary>
        /// <param name="config">The configuration that should be used.</param>
        /// <returns>A task resulting in the provider.</returns>
        /// <exception cref="DataProviderExcpetion">An error occured during creation of a
        /// <see cref="IDirectionsProvider"/>.</exception>
        ValueTask<IDirectionsProvider> CreateDirectionsProvider(ApiClientConfig config);

        /// <summary>
        /// Get an instance of <see cref="IMapImageProvider"/>.
        /// This instance may be cached.
        /// </summary>
        /// <param name="config">The configuration that should be used.</param>
        /// <returns>A task resulting in the provider.</returns>
        /// <exception cref="DataProviderExcpetion">An error occured during creation of a
        /// <see cref="IMapImageProvider"/>.</exception>
        ValueTask<IMapImageProvider> CreateMapImageProvider(ApiClientConfig config);

        /// <summary>
        /// If a shared <see cref="HttpClient"/> is used this method disposes of the instance and creates a new one.
        /// Data providers may need to be recreated.
        /// </summary>
        /// <exception cref="DataProviderExcpetion">An error occured while resetting the connection.</exception>
        void ResetConnection();
    }
}
