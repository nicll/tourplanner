using System;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

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
        ValueTask<string> GetImage(Route route);

        /// <summary>
        /// Clears any unnecessary information from the cache.
        /// </summary>
        /// <param name="dataManager">The data manager.</param>
        ValueTask CleanCache(IDataManager dataManager);
    }
}
