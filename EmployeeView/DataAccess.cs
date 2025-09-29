using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace EmployeeViewer.Data
{
    public class Db : IDisposable
    {
        private readonly string _connectionString;
        private readonly SqlConnection _conn;

        public Db(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _conn = new SqlConnection(_connectionString);
            _conn.Open();
        }

        public void Dispose()
        {
            if (_conn.State != ConnectionState.Closed) _conn.Close();
            _conn.Dispose();
        }

        private SqlCommand CreateProc(string procName)
        {
            var cmd = _conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procName;
            return cmd;
        }

        public List<Models.LookupItem> GetStatuses()
        {
            using (var cmd = CreateProc("dbo.usp_Statuses_Get"))
            using (var rdr = cmd.ExecuteReader())
            {
                var list = new List<Models.LookupItem>();
                while (rdr.Read())
                {
                    list.Add(new Models.LookupItem
                    {
                        Id = rdr.GetInt32(0),
                        Name = rdr.GetString(1)
                    });
                }
                return list;
            }
        }

        public List<Models.LookupItem> GetDepartments()
        {
            using (var cmd = CreateProc("dbo.usp_Dependencies_Get"))
            using (var rdr = cmd.ExecuteReader())
            {
                var list = new List<Models.LookupItem>();
                while (rdr.Read())
                {
                    list.Add(new Models.LookupItem { Id = rdr.GetInt32(0), Name = rdr.GetString(1) });
                }
                return list;
            }
        }

        public List<Models.LookupItem> GetPosts()
        {
            using (var cmd = CreateProc("dbo.usp_Posts_Get"))
            using (var rdr = cmd.ExecuteReader())
            {
                var list = new List<Models.LookupItem>();
                while (rdr.Read())
                {
                    list.Add(new Models.LookupItem { Id = rdr.GetInt32(0), Name = rdr.GetString(1) });
                }
                return list;
            }
        }

        public List<Models.PersonRow> GetPersons(int? statusId, int? depId, int? postId, string lastNameLike, string sortColumn, string sortDir)
        {
            using (var cmd = CreateProc("dbo.usp_Persons_List_v2"))
            {
                cmd.Parameters.AddWithValue("@StatusId", (object)statusId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DepId", (object)depId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PostId", (object)postId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LastNameLike", string.IsNullOrWhiteSpace(lastNameLike) ? (object)DBNull.Value : lastNameLike);
                cmd.Parameters.AddWithValue("@SortColumn", sortColumn ?? "last_name");
                cmd.Parameters.AddWithValue("@SortDir", string.Equals(sortDir, "DESC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC");

                using (var rdr = cmd.ExecuteReader())
                {
                    var list = new List<Models.PersonRow>();
                    while (rdr.Read())
                    {
                        var row = new Models.PersonRow
                        {
                            Id = rdr.GetInt32(rdr.GetOrdinal("id")),
                            LastName = rdr.GetString(rdr.GetOrdinal("last_name")),
                            FirstName = rdr.GetString(rdr.GetOrdinal("first_name")),
                            SecondName = rdr.GetString(rdr.GetOrdinal("second_name")),
                            StatusName = rdr.GetString(rdr.GetOrdinal("status_name")),
                            DepName = rdr.GetString(rdr.GetOrdinal("dep_name")),
                            PostName = rdr.GetString(rdr.GetOrdinal("post_name")),
                            DateEmploy = rdr.IsDBNull(rdr.GetOrdinal("date_employ")) ? (DateTime?)null : rdr.GetDateTime(rdr.GetOrdinal("date_employ")),
                            DateUneploy = rdr.IsDBNull(rdr.GetOrdinal("date_uneploy")) ? (DateTime?)null : rdr.GetDateTime(rdr.GetOrdinal("date_uneploy"))
                        };
                        list.Add(row);
                    }
                    return list;
                }
            }
        }

        public List<Models.StatPoint> GetStats(int statusId, DateTime dateFrom, DateTime dateTo, bool hired)
        {
            using (var cmd = CreateProc("dbo.usp_Stats_ByDay"))
            {
                cmd.Parameters.AddWithValue("@StatusId", statusId);
                cmd.Parameters.AddWithValue("@DateFrom", dateFrom.Date);
                cmd.Parameters.AddWithValue("@DateTo", dateTo.Date);
                cmd.Parameters.AddWithValue("@IsHired", hired);

                using (var rdr = cmd.ExecuteReader())
                {
                    var list = new List<Models.StatPoint>();
                    while (rdr.Read())
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

        public bool TestConnection()
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT 1";
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }
    }
}