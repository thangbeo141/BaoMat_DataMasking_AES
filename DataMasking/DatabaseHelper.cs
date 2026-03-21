using System;
using System.Data;
using System.Data.SqlClient;

namespace DataMasking
{
    public static class DatabaseHelper
    {
        private static readonly string connString = "workstation id=securitydb_thang.mssql.somee.com;packet size=4096;user id=thangbeo_SQLLogin_1;pwd=cd8x9wi9og;data source=securitydb_thang.mssql.somee.com;persist security info=False;initial catalog=securitydb_thang;TrustServerCertificate=True";

        public static void ExecuteInsert(string name, string phone, string email)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "INSERT INTO users (name, phone, email) VALUES (@n, @p, @e)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 200).Value = (object)name ?? string.Empty;
                    // Nới rộng độ dài để chứa chuỗi mã hóa AES
                    cmd.Parameters.Add("@p", SqlDbType.NVarChar, 255).Value = (object)phone ?? string.Empty;
                    cmd.Parameters.Add("@e", SqlDbType.NVarChar, 255).Value = (object)email ?? string.Empty;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ExecuteSelect(string keyword = "")
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT id, name, phone, email FROM users";
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