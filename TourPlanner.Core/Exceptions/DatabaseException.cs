using System;
using TourPlanner.Core.Interfaces;

namespace TourPlanner.Core.Exceptions
{
    /// <summary>
    /// Represents errors that occur inside implementations of <see cref="IDatabaseClient"/>
    /// and <see cref="IDatabaseClientFactory"/>.
    /// </summary>
    public class DatabaseException : TourPlannerException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DatabaseException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The excpetion that caused this exception.</param>
        public DatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
