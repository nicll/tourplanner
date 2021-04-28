using System;

namespace TourPlanner.Core.Models
{
    /// <summary>
    /// Defines multiple properties which may be used for configuring API clients.
    /// Usually, not all of these are used.
    /// </summary>
    public class ApiClientConfig
    {
        public string ServiceEndpoint { get; init; }

        public string ApiKey { get; init; }

        public string Username { get; init; }

        public string Password { get; init; }

        public TimeSpan Timeout { get; init; }
    }
}
