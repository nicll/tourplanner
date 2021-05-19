using log4net;
using log4net.Core;
using Npgsql.Logging;
using System;

namespace TourPlanner.DB.Postgres
{
    // from https://stackoverflow.com/a/43107110/13282284
    public class Log4NetNpgsqlLogger : NpgsqlLogger
    {
        private readonly ILog _log;

        public Log4NetNpgsqlLogger(string name)
            => _log = LogManager.GetLogger(name);

        public override bool IsEnabled(NpgsqlLogLevel level)
            => _log.Logger.IsEnabledFor(GetLog4NetLevelFromNpgsqlLogLevel(level));

        public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception exception = null)
            => _log.Logger.Log(typeof(NpgsqlLogger), GetLog4NetLevelFromNpgsqlLogLevel(level), connectorId + ": " + msg, exception);

        protected Level GetLog4NetLevelFromNpgsqlLogLevel(NpgsqlLogLevel level) => level switch
        {
            NpgsqlLogLevel.Trace or NpgsqlLogLevel.Debug => Level.Debug,
            NpgsqlLogLevel.Info => Level.Info,
            NpgsqlLogLevel.Warn => Level.Warn,
            NpgsqlLogLevel.Error => Level.Error,
            NpgsqlLogLevel.Fatal => Level.Fatal,
            _ => throw new Exception("Unknown Npgsql Log Level: " + level),
        };
    }
}
