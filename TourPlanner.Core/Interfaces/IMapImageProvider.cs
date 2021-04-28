using System;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
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
        /// Empties the cache of downloaded image files.
        /// </summary>
        ValueTask ClearCache();
    }
}
