using Npgsql.Logging;
using System;

namespace TourPlanner.DB.Postgres
{
    // from https://stackoverflow.com/a/43107110/13282284
    public class Log4NetNpgsqlLoggingProvider : INpgsqlLoggingProvider
    {
        public string DefaultLoggerName { get; }

        public string CommandLoggerName { get; set; }

        public Log4NetNpgsqlLoggingProvider(string defaultLoggerName)
            => DefaultLoggerName = defaultLoggerName ?? throw new ArgumentNullException(nameof(defaultLoggerName));

        public NpgsqlLogger CreateLogger(string name) => name switch
        {
            "Npgsql.NpgsqlCommand" => new Log4NetNpgsqlLogger(CommandLoggerName ?? DefaultLoggerName),
            _ => new Log4NetNpgsqlLogger(DefaultLoggerName),
        };
    }
}
