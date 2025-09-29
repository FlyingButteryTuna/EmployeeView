using System.Data;
using Microsoft.Data.SqlClient;

using static EmployeeViewer.Data.DbNames;
using static EmployeeViewer.Data.SqlParams;

namespace EmployeeViewer.Data
{
    public sealed class Db
    {
        private readonly string _connectionString;
        public Db(string connectionString)
            => _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        private static SqlCommand CreateProc(SqlConnection conn, string procName)
            => new SqlCommand(procName, conn) { CommandType = CommandType.StoredProcedure };

        public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            var r = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(r) == 1;
        }

        public async Task<List<Models.LookupItem>> GetStatusesAsync(CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = CreateProc(conn, UspStatusesGet);
            await using var rdr = await cmd.ExecuteReaderAsync(ct);

            var list = new List<Models.LookupItem>();
            while (await rdr.ReadAsync(ct))
                list.Add(new Models.LookupItem { Id = rdr.GetInt32(0), Name = rdr.GetString(1) });
            return list;
        }

        public async Task<List<Models.LookupItem>> GetDepartmentsAsync(CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = CreateProc(conn, UspDepsGet);
            await using var rdr = await cmd.ExecuteReaderAsync(ct);

            var list = new List<Models.LookupItem>();
            while (await rdr.ReadAsync(ct))
                list.Add(new Models.LookupItem { Id = rdr.GetInt32(0), Name = rdr.GetString(1) });
            return list;
        }

        public async Task<List<Models.LookupItem>> GetPostsAsync(CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = CreateProc(conn, UspPostsGet);
            await using var rdr = await cmd.ExecuteReaderAsync(ct);

            var list = new List<Models.LookupItem>();
            while (await rdr.ReadAsync(ct))
                list.Add(new Models.LookupItem { Id = rdr.GetInt32(0), Name = rdr.GetString(1) });
            return list;
        }

        public async Task<List<Models.PersonRow>> GetPersonsAsync(
            int? statusId, int? depId, int? postId,
            string lastNameLike, string sortColumn, string sortDir,
            CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = CreateProc(conn, UspPersonsListV2);
            cmd.Parameters.Add(Int("@StatusId", statusId));
            cmd.Parameters.Add(Int("@DepId", depId));
            cmd.Parameters.Add(Int("@PostId", postId));
            cmd.Parameters.Add(NVarChar("@LastNameLike", 100, lastNameLike));
            cmd.Parameters.Add(NVarCharFixed("@SortColumn", 50, string.IsNullOrWhiteSpace(sortColumn) ? ColLastName : sortColumn!));
            cmd.Parameters.Add(NVarCharFixed("@SortDir", 4,
                string.Equals(sortDir, "DESC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC"));

            var list = new List<Models.PersonRow>();
            await using var rdr = await cmd.ExecuteReaderAsync(ct);

            int ordId = rdr.GetOrdinal(ColId);
            int ordLn = rdr.GetOrdinal(ColLastName);
            int ordFn = rdr.GetOrdinal(ColFirstName);
            int ordSn = rdr.GetOrdinal(ColSecondName);
            int ordStatus = rdr.GetOrdinal(ColStatusName);
            int ordDep = rdr.GetOrdinal(ColDepName);
            int ordPost = rdr.GetOrdinal(ColPostName);
            int ordDe = rdr.GetOrdinal(ColDateEmploy);
            int ordDu = rdr.GetOrdinal(ColDateUneploy);

            while (await rdr.ReadAsync(ct))
            {
                list.Add(new Models.PersonRow
                {
                    Id = rdr.GetInt32(ordId),
                    LastName = rdr.GetString(ordLn),
                    FirstName = rdr.IsDBNull(ordFn) ? null : rdr.GetString(ordFn),
                    SecondName = rdr.IsDBNull(ordSn) ? null : rdr.GetString(ordSn),
                    StatusName = rdr.GetString(ordStatus),
                    DepName = rdr.GetString(ordDep),
                    PostName = rdr.GetString(ordPost),
                    DateEmploy = rdr.IsDBNull(ordDe) ? (DateTime?)null : rdr.GetDateTime(ordDe),
                    DateUneploy = rdr.IsDBNull(ordDu) ? (DateTime?)null : rdr.GetDateTime(ordDu)
                });
            }
            return list;
        }

        public async Task<List<Models.StatPoint>> GetStatsAsync(int statusId, DateTime dateFrom, DateTime dateTo, bool hired, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = CreateProc(conn, UspStatsByDay);

            cmd.Parameters.Add(Int("@StatusId", statusId));
            cmd.Parameters.Add(Date("@DateFrom", dateFrom));
            cmd.Parameters.Add(Date("@DateTo", dateTo));
            cmd.Parameters.Add(Bit("@IsHired", hired));

            var list = new List<Models.StatPoint>();
            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct))
            {
                list.Add(new Models.StatPoint
                {
                    Day = rdr.GetDateTime(0).Date,
                    Count = rdr.GetInt32(1)
                });
            }
            return list;
        }
    }
}