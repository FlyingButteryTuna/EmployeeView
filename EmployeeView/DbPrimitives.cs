using System.Data;
using Microsoft.Data.SqlClient;

namespace EmployeeViewer.Data
{
    internal static class DbNames
    {
        public const string UspStatusesGet = "dbo.usp_Statuses_Get";
        public const string UspPostsGet = "dbo.usp_Posts_Get";
        public const string UspDepsGet = "dbo.usp_Dependencies_Get";
        public const string UspPersonsListV2 = "dbo.usp_Persons_List_v2";
        public const string UspStatsByDay = "dbo.usp_Stats_ByDay";

        public const string ColId = "id";
        public const string ColFirstName = "first_name";
        public const string ColSecondName = "second_name";
        public const string ColLastName = "last_name";
        public const string ColStatusName = "status_name";
        public const string ColDepName = "dep_name";
        public const string ColPostName = "post_name";
        public const string ColDateEmploy = "date_employ";
        public const string ColDateUneploy = "date_uneploy";
        public const string ColDay = "Day";
        public const string ColCount = "Cnt";
    }

    internal static class SqlParams
    {
        public static SqlParameter Int(string name, int? value)
        {
            var p = new SqlParameter(name, SqlDbType.Int);
            p.Value = (object?)value ?? DBNull.Value;
            return p;
        }

        public static SqlParameter Bit(string name, bool value) => new SqlParameter(name, SqlDbType.Bit) { Value = value };

        public static SqlParameter Date(string name, DateTime value) => new SqlParameter(name, SqlDbType.Date) { Value = value.Date };

        public static SqlParameter NVarChar(string name, int size, string? value)
        {
            var p = new SqlParameter(name, SqlDbType.NVarChar, size);
            p.Value = string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value!;
            return p;
        }

        public static SqlParameter NVarCharFixed(string name, int size, string value) =>
            new SqlParameter(name, SqlDbType.NVarChar, size) { Value = value };
    }
}