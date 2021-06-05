using System;

namespace TourPlanner.Core.Exceptions
{
    /// <summary>
    /// Represents an abstract base type for all <see cref="TourPlanner"/> exceptions.
    /// </summary>
    public abstract class TourPlannerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TourPlannerException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TourPlannerException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TourPlannerException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The excpetion that caused this exception.</param>
        public TourPlannerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
