using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TourPlanner.Core.Models;

namespace TourPlanner.Core.Interfaces
{
    public interface IDataConverter
    {
        /// <summary>
        /// The preferred extension of the file when saved to disk.
        /// </summary>
        string PreferredFileExtension { get; }

        /// <summary>
        /// Display name of the converter.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Read all tours from <paramref name="inputStream"/>.
        /// </summary>
        /// <param name="inputStream">The input data stream.</param>
        /// <returns>All tours that could be read.</returns>
        Task<ICollection<Tour>> ReadTours(Stream inputStream);

        /// <summary>
        /// Write all given tours into <paramref name="outputStream"/>.
        /// </summary>
        /// <param name="outputStream">The output data stream.</param>
        /// <param name="tours">The given tours.</param>
        Task WriteTours(Stream outputStream, ICollection<Tour> tours);
    }
}
