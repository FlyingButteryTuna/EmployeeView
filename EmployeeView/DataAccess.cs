using System.Data;
using Microsoft.Data.SqlClient;

namespace EmployeeViewer.Data
{
    public sealed class Db : IDisposable
    {
        private readonly string _connectionString;
        public Db(string connectionString)
            => _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        public void Dispose() { }

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
            await using var cmd = CreateProc(conn, "dbo.usp_Statuses_Get");
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
            await using var cmd = CreateProc(conn, "dbo.usp_Dependencies_Get");
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
            await using var cmd = CreateProc(conn, "dbo.usp_Posts_Get");
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

            await using var cmd = CreateProc(conn, "dbo.usp_Persons_List_v2");
            cmd.Parameters.Add("@StatusId", SqlDbType.Int).Value = (object?)statusId ?? DBNull.Value;
            cmd.Parameters.Add("@DepId", SqlDbType.Int).Value = (object?)depId ?? DBNull.Value;
            cmd.Parameters.Add("@PostId", SqlDbType.Int).Value = (object?)postId ?? DBNull.Value;
            cmd.Parameters.Add("@LastNameLike", SqlDbType.NVarChar, 100).Value =
                string.IsNullOrWhiteSpace(lastNameLike) ? (object)DBNull.Value : lastNameLike;
            cmd.Parameters.Add("@SortColumn", SqlDbType.NVarChar, 50).Value = sortColumn ?? "last_name";
            cmd.Parameters.Add("@SortDir", SqlDbType.NVarChar, 4).Value =
                string.Equals(sortDir, "DESC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            var list = new List<Models.PersonRow>();
            await using var rdr = await cmd.ExecuteReaderAsync(ct);

            int ordId = rdr.GetOrdinal("id");
            int ordLn = rdr.GetOrdinal("last_name");
            int ordFn = rdr.GetOrdinal("first_name");
            int ordSn = rdr.GetOrdinal("second_name");
            int ordStatus = rdr.GetOrdinal("status_name");
            int ordDep = rdr.GetOrdinal("dep_name");
            int ordPost = rdr.GetOrdinal("post_name");
            int ordDe = rdr.GetOrdinal("date_employ");
            int ordDu = rdr.GetOrdinal("date_uneploy");

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
            await using var cmd = CreateProc(conn, "dbo.usp_Stats_ByDay");
            cmd.Parameters.Add("@StatusId", SqlDbType.Int).Value = statusId;
            cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = dateFrom.Date;
            cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = dateTo.Date;
            cmd.Parameters.Add("@IsHired", SqlDbType.Bit).Value = hired;

            var list = new List<Models.StatPoint>();
            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct))
                list.Add(new Models.StatPoint { Day = rdr.GetDateTime(0).Date, Count = rdr.GetInt32(1) });
            return list;
        }
    }
}