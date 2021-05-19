using Npgsql;
using Npgsql.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourPlanner.Core.Interfaces;
using TourPlanner.Core.Models;

namespace TourPlanner.DB.Postgres
{
    public class NpgsqlClient : IDatabaseClient
    {
        private record DbTourInfo(string Name, string Image, string Description, string Start, string End, string RouteId);

        private class DbTour { public DbTourInfo Tour; public List<Step> Steps = new(); public List<LogEntry> Log = new(); }

        private readonly string _connectionString;

        static NpgsqlClient()
            => NpgsqlLogManager.Provider = new Log4NetNpgsqlLoggingProvider(nameof(NpgsqlClient) + "Logger") { CommandLoggerName = nameof(NpgsqlClient) + "CommandLogger" };

        public NpgsqlClient(string connectionString)
            => _connectionString = connectionString;

        public async Task<ICollection<Tour>> QueryTours()
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

            using (var cmd = new NpgsqlCommand("SELECT tourid, distance, description FROM steps", conn))
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
                        Description = reader.GetString(2)
                    });
                }
            }

            using (var cmd = new NpgsqlCommand("SELECT tourid, date, distance, duration, rating FROM log_entries", conn))
            {
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    currentTourId = reader.GetGuid(0);

                    if (!dict.TryGetValue(currentTourId, out var tour))
                        throw null;

                    tour.Log.Add(new()
                    {
                        Date = reader.GetDateTime(1),
                        Distance = reader.GetDouble(2),
                        Duration = reader.GetTimeSpan(3),
                        Rating = reader.GetFloat(4)
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

            return ret;
        }

        public async Task BatchSynchronize(IReadOnlyCollection<Tour> newTours, IReadOnlyCollection<Tour> removedTours, IReadOnlyCollection<Tour> changedTours)
        {
            using var conn = await OpenConnection();
            using var trans = await conn.BeginTransactionAsync();

            await InsertTours(conn, trans, newTours);
            await RemoveTours(conn, trans, removedTours);
            await UpdateTours(conn, trans, changedTours);

            await trans.CommitAsync();
        }

        private async Task<NpgsqlConnection> OpenConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            // manual closing is not required as long as the using keyword is used
            // also, Npgsql automatically pools connections, see https://www.npgsql.org/doc/connection-string-parameters.html
            return conn;
        }

        private static async Task InsertTours(NpgsqlConnection conn, NpgsqlTransaction trans, IReadOnlyCollection<Tour> tours)
        {
            if (!tours.Any())
                return;

            using (var cmd = new NpgsqlCommand("INSERT INTO tours VALUES ", conn, trans))
            {
                cmd.CommandText += DataToNpgsqlCommand(cmd, tours, t => t.TourId, t => t.Name, t => t.ImagePath, t => t.CustomDescription, t => t.Route.StartLocation, t => t.Route.EndLocation, t => t.Route.RouteId);

                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmd = new NpgsqlCommand("INSERT INTO steps VALUES ", conn, trans))
            {
                cmd.CommandText += ManyDataToNpgsqlCommand<Tour, IEnumerable<(Guid, double, string)>, (Guid, double, string)>
                    (cmd, tours, t => t.Route.Steps.Select(s => (t.TourId, s.Distance, s.Description)), x => x.Item1, x => x.Item2, x => x.Item3);

                await cmd.ExecuteNonQueryAsync();
            }

            if (!tours.Any(t => t.Log.Any()))
                return;

            using (var cmd = new NpgsqlCommand("INSERT INTO log_entries VALUES ", conn, trans))
            {
                cmd.CommandText += ManyDataToNpgsqlCommand<Tour, IEnumerable<(Guid, DateTime, double, TimeSpan, float)>, (Guid, DateTime, double, TimeSpan, float)>
                    (cmd, tours, t => t.Log.Select(l => (t.TourId, l.Date, l.Distance, l.Duration, l.Rating)), x => x.Item1, x => x.Item2, x => x.Item3, x => x.Item4, x => x.Item5);

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

            using var cmd = new NpgsqlCommand(String.Empty, conn, trans);
        }

        private static string DataToNpgsqlCommand<T>(NpgsqlCommand cmd, IReadOnlyCollection<T> data, params Func<T, object>[] selectors)
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

        private static string ManyDataToNpgsqlCommand<T, U, V>(NpgsqlCommand cmd, IReadOnlyCollection<T> data, Func<T, U> dataTransformer, params Func<V, object>[] selectors) where U : IEnumerable<V>
        {
            var sb = new StringBuilder(" ");
            int counter = 0;

            foreach (var rowGenerator in data)
            {
                var rows = dataTransformer(rowGenerator);

                if (!rows.Any())
                    continue;

                foreach (var row in rows)
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
            }

            --sb.Length;
            return sb.ToString();
        }
    }
}
