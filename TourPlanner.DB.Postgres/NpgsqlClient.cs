using log4net;
using Npgsql;
using Npgsql.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Core.Exceptions;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DB.Postgres
{
    internal class NpgsqlClient : IDatabaseClient
    {
        private record DbTourInfo(string Name, string Image, string Description, string Start, string End, string RouteId);

        private class DbTour { public DbTourInfo Tour; public List<Step> Steps = new(); public List<LogEntry> Log = new(); }

        private readonly string _connectionString;
        private readonly ILog _log = LogManager.GetLogger(typeof(NpgsqlClient));

        static NpgsqlClient()
            => NpgsqlLogManager.Provider = new Log4NetNpgsqlLoggingProvider(nameof(NpgsqlClient) + "Logger") { CommandLoggerName = nameof(NpgsqlClient) + "CommandLogger" };

        public NpgsqlClient(string connectionString)
            => _connectionString = connectionString;

        public async Task<ICollection<Tour>> QueryTours()
        {
            try { return await FetchTours(); }
            catch (NpgsqlException ex)
            {
                _log.Error("An error occured while querying the database.", ex);
                throw new DatabaseException("An error occured while querying the database.", ex);
            }
        }

        public async Task BatchSynchronize(IChangeTrackingCollection<Tour> tours)
        {
            try
            {
                using var conn = await OpenConnection();
                using var trans = await conn.BeginTransactionAsync();

                await InsertTours(conn, trans, tours.NewItems);
                await RemoveTours(conn, trans, tours.RemovedItems);
                await UpdateTours(conn, trans, tours.ChangedItems);

                await trans.CommitAsync();
            }
            catch (NpgsqlException ex)
            {
                _log.Error("An error occured while synchronizing the database with local changes.", ex);
                throw new DatabaseException("An error occured while synchronizing the database with local changes.", ex);
            }
        }

        private async Task<NpgsqlConnection> OpenConnection()
        {
            _log.Debug("Opening connection to database.");
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            // manual closing is not required as long as the using keyword is used
            // also, Npgsql automatically pools connections, see https://www.npgsql.org/doc/connection-string-parameters.html
            return conn;
        }

        private async Task<ICollection<Tour>> FetchTours()
        {
            using var conn = await OpenConnection();
            var dict = new Dictionary<Guid, DbTour>();
            Guid currentTourId;

            using (var cmd = new NpgsqlCommand("SELECT tourid, name, image, description, start, \"end\", routeid FROM tours", conn))
            {
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    currentTourId = reader.GetGuid(0);
                    var tour = new DbTour();

                    tour.Tour = new(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));
                    dict.Add(currentTourId, tour);
                }
            }

            using (var cmd = new NpgsqlCommand("SELECT tourid, distance, description, icon FROM steps", conn))
            {
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    currentTourId = reader.GetGuid(0);

                    if (!dict.TryGetValue(currentTourId, out var tour))
                        throw null;

                    tour.Steps.Add(new()
                    {
                        Distance = reader.GetDouble(1),
                        Description = reader.GetString(2),
                        IconPath = reader.GetString(3)
                    });
                }
            }

            using (var cmd = new NpgsqlCommand("SELECT logid, tourid, date, distance, duration, rating, notes, vehicle, participants FROM log_entries", conn))
            {
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    currentTourId = reader.GetGuid(1);

                    if (!dict.TryGetValue(currentTourId, out var tour))
                        throw null;

                    tour.Log.Add(new()
                    {
                        LogId = reader.GetGuid(0),
                        Date = reader.GetDateTime(2),
                        Distance = reader.GetDouble(3),
                        Duration = reader.GetTimeSpan(4),
                        Rating = reader.GetFloat(5),
                        Notes = reader.GetString(6),
                        Vehicle = reader.GetString(7),
                        ParticipantCount = reader.GetInt32(8)
                    });
                }
            }

            var ret = new List<Tour>();
            foreach (var tour in dict)
            {
                ret.Add(new(tour.Value.Log)
                {
                    TourId = tour.Key,
                    Name = tour.Value.Tour.Name,
                    ImagePath = tour.Value.Tour.Image,
                    CustomDescription = tour.Value.Tour.Description,
                    Route = new() { RouteId = tour.Value.Tour.RouteId, StartLocation = tour.Value.Tour.Start, EndLocation = tour.Value.Tour.End, Steps = tour.Value.Steps }
                });
            }

            _log.Info("Loaded tours from database.");
            return ret;
        }

        private static async Task InsertTours(NpgsqlConnection conn, NpgsqlTransaction trans, IReadOnlyCollection<Tour> tours)
        {
            if (!tours.Any())
                return;

            using (var cmd = new NpgsqlCommand("INSERT INTO tours VALUES ", conn, trans))
            {
                cmd.CommandText += FeedDataToNpgsqlCommand(cmd, tours,
                    t => t.TourId, t => t.Name, t => t.ImagePath, t => t.CustomDescription, t => t.Route.StartLocation, t => t.Route.EndLocation, t => t.Route.RouteId);

                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmd = new NpgsqlCommand("INSERT INTO steps VALUES ", conn, trans))
            {
                cmd.CommandText += FeedDataToNpgsqlCommand(cmd,
                    tours.SelectMany(t => t.Route.Steps.Select(s => (t.TourId, s.Distance, s.Description, s.IconPath))),
                    x => x.TourId, x => x.Distance, x => x.Description, x => x.IconPath);

                await cmd.ExecuteNonQueryAsync();
            }

            // log entries may not exist for new tours
            if (!tours.Any(t => t.Log.Any()))
                return;

            using (var cmd = new NpgsqlCommand("INSERT INTO log_entries VALUES ", conn, trans))
            {
                cmd.CommandText += FeedDataToNpgsqlCommand(cmd,
                    tours.SelectMany(t => t.Log.Select(l => (l.LogId, t.TourId, l.Date, l.Distance, l.Duration, l.Rating, l.Notes, l.Vehicle, l.ParticipantCount))),
                    x => x.LogId, x => x.TourId, x => x.Date, x => x.Distance, x => x.Duration, x => x.Rating, x => x.Notes, x => x.Vehicle, x => x.ParticipantCount);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task RemoveTours(NpgsqlConnection conn, NpgsqlTransaction trans, IReadOnlyCollection<Tour> tours)
        {
            if (!tours.Any())
                return;

            using var cmd = new NpgsqlCommand("DELETE FROM tours WHERE tourid = @tid", conn, trans);
            cmd.Parameters.Add("@tid", NpgsqlTypes.NpgsqlDbType.Uuid);

            foreach (var tour in tours)
            {
                cmd.Parameters["@tid"].Value = tour.TourId;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task UpdateTours(NpgsqlConnection conn, NpgsqlTransaction trans, IReadOnlyCollection<Tour> tours)
        {
            if (!tours.Any())
                return;

            // certain properties are readonly, we only need to update:
            // name, description, log_entries

            using var cmd = new NpgsqlCommand("UPDATE tours SET name = @name, description = @desc WHERE tourid = @tid", conn, trans);
            cmd.Parameters.Add("@name", NpgsqlTypes.NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@desc", NpgsqlTypes.NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@tid", NpgsqlTypes.NpgsqlDbType.Uuid);

            foreach (var tour in tours)
            {
                if (tour.IsTourChanged)
                {
                    cmd.Parameters["@name"].Value = tour.Name;
                    cmd.Parameters["@desc"].Value = tour.CustomDescription;
                    cmd.Parameters["@tid"].Value = tour.TourId;
                    await cmd.ExecuteNonQueryAsync();
                }

                if (tour.Log.IsChanged)
                {
                    await InsertLogs(conn, trans, tour.TourId, tour.Log.NewItems);
                    await RemoveLogs(conn, trans, tour.Log.RemovedItems);
                    await UpdateLogs(conn, trans, tour.Log.ChangedItems);
                }
            }
        }

        private static async Task InsertLogs(NpgsqlConnection conn, NpgsqlTransaction trans, Guid tourId, IReadOnlyCollection<LogEntry> log)
        {
            if (!log.Any())
                return;

            using var cmd = new NpgsqlCommand("INSERT INTO log_entries VALUES ", conn, trans);
            cmd.CommandText += FeedDataToNpgsqlCommand(cmd, log,
                l => l.LogId, _ => tourId, l => l.Date, l => l.Distance, l => l.Duration, l => l.Rating, l => l.Notes, l => l.Vehicle, l => l.ParticipantCount);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task RemoveLogs(NpgsqlConnection conn, NpgsqlTransaction trans, IReadOnlyCollection<LogEntry> log)
        {
            if (!log.Any())
                return;

            using var cmd = new NpgsqlCommand("DELETE FROM log_entries WHERE logid = @lid", conn, trans);
            cmd.Parameters.Add("@lid", NpgsqlTypes.NpgsqlDbType.Uuid);

            foreach (var entry in log)
            {
                cmd.Parameters["@lid"].Value = entry.LogId;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task UpdateLogs(NpgsqlConnection conn, NpgsqlTransaction trans, IReadOnlyCollection<LogEntry> log)
        {
            if (!log.Any())
                return;

            using var cmd = new NpgsqlCommand("UPDATE log_entries SET date = @date, distance = @dist, duration = @drtn, rating = @rtng, notes = @note, vehicle = @vhcl, participants = @ptcp WHERE logid = @lid", conn, trans);
            cmd.Parameters.Add("@date", NpgsqlTypes.NpgsqlDbType.Date);
            cmd.Parameters.Add("@dist", NpgsqlTypes.NpgsqlDbType.Double);
            cmd.Parameters.Add("@drtn", NpgsqlTypes.NpgsqlDbType.Interval);
            cmd.Parameters.Add("@rtng", NpgsqlTypes.NpgsqlDbType.Real);
            cmd.Parameters.Add("@note", NpgsqlTypes.NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@vhcl", NpgsqlTypes.NpgsqlDbType.Varchar);
            cmd.Parameters.Add("@ptcp", NpgsqlTypes.NpgsqlDbType.Integer);
            cmd.Parameters.Add("@lid", NpgsqlTypes.NpgsqlDbType.Uuid);

            foreach (var entry in log)
            {
                cmd.Parameters["@date"].Value = entry.Date;
                cmd.Parameters["@dist"].Value = entry.Distance;
                cmd.Parameters["@drtn"].Value = entry.Duration;
                cmd.Parameters["@rtng"].Value = entry.Rating;
                cmd.Parameters["@note"].Value = entry.Notes;
                cmd.Parameters["@vhcl"].Value = entry.Vehicle;
                cmd.Parameters["@ptcp"].Value = entry.ParticipantCount;
                cmd.Parameters["@lid"].Value = entry.LogId;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static string FeedDataToNpgsqlCommand<T>(NpgsqlCommand cmd, IEnumerable<T> data, params Func<T, object>[] selectors)
        {
            var sb = new StringBuilder(" ");
            int counter = 0;

            foreach (var row in data)
            {
                sb.Append('(');

                foreach (var selector in selectors)
                {
                    var currentParameter = "@p_" + ++counter;
                    sb.Append(currentParameter).Append(',');
                    cmd.Parameters.AddWithValue(currentParameter, selector(row));
                }

                --sb.Length;
                sb.Append("),");
            }

            --sb.Length;
            return sb.ToString();
        }
    }
}
