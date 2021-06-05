using System;
using System.Threading.Tasks;
using TourPlanner.Core.Models;
using TourPlanner.Core.Exceptions;

namespace TourPlanner.Core.Interfaces
{
    /// <summary>
    /// An interface for querying visual representations for routes.
    /// </summary>
    public interface IMapImageProvider
    {
        /// <summary>
        /// Loads an image for the given <paramref name="route"/>.
        /// The image file type may be JPEG, PNG or BMP.
        /// The image file may be cached.
        /// </summary>
        /// <param name="route">The route for which to load the image.</param>
        /// <returns>Absolute path to the locally stored image file.</returns>
        /// <exception cref="DataProviderExcpetion">An error occured while querying a map image.</exception>
        ValueTask<string> GetImage(Route route);

        /// <summary>
        /// Clears any unnecessary information from the cache.
        /// </summary>
        /// <param name="dataManager">The data manager.</param>
        /// <exception cref="DataProviderExcpetion">An error occured while cleaning the map cache.</exception>
        ValueTask CleanCache(IDataManager dataManager);
    }
}
