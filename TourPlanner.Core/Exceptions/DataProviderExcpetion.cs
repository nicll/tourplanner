using System;
using TourPlanner.Core.Interfaces;

namespace TourPlanner.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur inside implementations of <see cref="IDataProviderFactory"/>,
    /// <see cref="IDirectionsProvider"/> and <see cref="IMapImageProvider"/>.
    /// </summary>
    public class DataProviderExcpetion : TourPlannerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataProviderExcpetion"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DataProviderExcpetion(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProviderExcpetion"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The excpetion that caused this exception.</param>
        public DataProviderExcpetion(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
