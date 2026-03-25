using System;
using System.Data;
using System.Data.SqlClient;

namespace DataMasking
{
    public static class DatabaseHelper
    {
        private static readonly string connString = "workstation id=securitydb_thang.mssql.somee.com;packet size=4096;user id=thangbeo_SQLLogin_1;pwd=cd8x9wi9og;data source=securitydb_thang.mssql.somee.com;persist security info=False;initial catalog=securitydb_thang;TrustServerCertificate=True";

        public static void ExecuteInsert(string name, string phone, string email, string cccd)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Thêm cột cccd vào câu lệnh INSERT
                string query = "INSERT INTO users (name, phone, email, cccd) VALUES (@n, @p, @e, @c)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 200).Value = name ?? string.Empty;
                    cmd.Parameters.Add("@p", SqlDbType.NVarChar, 255).Value = phone ?? string.Empty;
                    cmd.Parameters.Add("@e", SqlDbType.NVarChar, 255).Value = email ?? string.Empty;
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 255).Value = cccd ?? string.Empty;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ExecuteSelect(string keyword = "")
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Lấy thêm cột cccd từ Database
                string query = "SELECT id, name, phone, email, cccd FROM users";
                if (!string.IsNullOrEmpty(keyword)) query += " WHERE name LIKE @kw";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(keyword))
                        cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public static void ExecuteDelete(int id)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "DELETE FROM users WHERE id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}