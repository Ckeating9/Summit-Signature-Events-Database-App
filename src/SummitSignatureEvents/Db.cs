using System;
using System.Data;
using System.Data.SqlClient;

namespace SummitSignatureEventsPart2
{
    internal static class Db
    {
        private const string DatabaseName = "SummitSignatureEventsPart1";

        // Add your exact SQL Server instance here if your computer uses a different name.
        private static readonly string[] CandidateDataSources =
        {
            @".\SQLEXPRESS",
            @".",
            @"(local)",
            @"localhost",
            @"(localdb)\MSSQLLocalDB"
        };

        public static string ConnectionString { get; private set; }
        public static string DataSourceUsed { get; private set; }

        static Db()
        {
            Initialize();
        }

        private static void Initialize()
        {
            foreach (string dataSource in CandidateDataSources)
            {
                string cs = BuildConnectionString(dataSource);
                if (CanOpen(cs))
                {
                    ConnectionString = cs;
                    DataSourceUsed = dataSource;
                    return;
                }
            }

            DataSourceUsed = CandidateDataSources[0];
            ConnectionString = BuildConnectionString(DataSourceUsed);
        }

        private static string BuildConnectionString(string dataSource)
        {
            return string.Format(
                "Data Source={0};Initial Catalog={1};Integrated Security=True;Encrypt=False;TrustServerCertificate=True",
                dataSource,
                DatabaseName);
        }

        private static bool CanOpen(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static DataTable GetTable(string query, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                da.Fill(dt);
            }

            return dt;
        }

        public static object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
