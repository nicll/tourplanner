using System;
using System.Threading.Tasks;
using TourPlanner.Core.Configuration;
using TourPlanner.Core.Exceptions;

namespace TourPlanner.Core.Interfaces
{
    public interface IDatabaseClientFactory
    {
        /// <summary>
        /// Get an instance of <see cref="IDatabaseClient"/>.
        /// This instance may be cached per connection string.
        /// </summary>
        /// <param name="config">The configuration that should be used.</param>
        /// <returns>A task resulting in the provider.</returns>
        /// <exception cref="DatabaseException">An error occured during creation of a
        /// <see cref="IDatabaseClient"/>.</exception>
        ValueTask<IDatabaseClient> CreateDatabaseClient(DbClientConfig config);
    }
}
