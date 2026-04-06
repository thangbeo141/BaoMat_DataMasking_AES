using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DataMasking
{
    public static class DatabaseHelper
    {
        private static readonly string connString = ConfigurationManager.ConnectionStrings["MySecurityDB"].ConnectionString;

        // 1. Hàm ĐĂNG KÝ: Lưu cả Tài khoản, Pass băm và Dữ liệu cá nhân (đã mã hóa)
        public static bool ExecuteInsert(string username, string hash, string salt, string role, string name, string phone, string email, string cccd, string encryptedKey)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "INSERT INTO users (Username, PasswordHash, PasswordSalt, Role, name, phone, email, cccd, EncryptedKey) " +
                                   "VALUES (@u, @h, @s, @r, @n, @p, @e, @c, @k)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@h", hash);
                        cmd.Parameters.AddWithValue("@s", salt);
                        cmd.Parameters.AddWithValue("@r", role);
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@p", phone);
                        cmd.Parameters.AddWithValue("@e", email);
                        cmd.Parameters.AddWithValue("@c", cccd);
                        cmd.Parameters.AddWithValue("@k", encryptedKey);
                        conn.Open();
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        // 2. ADMIN: Load toàn bộ danh sách khách hàng
        public static DataTable LayDanhSachUsers(string keyword = "")
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT id, Username, name, phone, email, cccd, EncryptedKey FROM users WHERE IsActive = 1 AND Role = 'Customer'";
                    if (!string.IsNullOrEmpty(keyword)) query += " AND name LIKE @keyword";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                    }
                }
            }
            catch { }
            return dt;
        }

        // 3. KHÁCH HÀNG: Chỉ Load đúng 1 dòng dữ liệu của chính họ
        public static DataTable LayDuLieuCuaToi(string username)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT name, phone, email, cccd, EncryptedKey FROM users WHERE IsActive = 1 AND Username = @u";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
                    }
                }
            }
            catch { }
            return dt;
        }

        // 4. KIỂM TRA ĐĂNG NHẬP
        public static string CheckLoginAndGetRole(string username, string rawPassword)
        {
            string dbHash = "", dbSalt = "", role = "";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Giờ ta check luôn trong bảng users
                string query = "SELECT PasswordHash, PasswordSalt, Role FROM users WHERE Username = @u AND IsActive = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dbHash = reader["PasswordHash"].ToString();
                            dbSalt = reader["PasswordSalt"].ToString();
                            role = reader["Role"].ToString();
                        }
                        else return null;
                    }
                }
            }

            string hashInput = CustomSHA256.ComputeHash(rawPassword + dbSalt);
            if (hashInput == dbHash) return role;
            return null;
        }

        public static bool XoaUser(string idUser)
        {
            // (Giữ nguyên như cũ)
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "UPDATE users SET IsActive = 0 WHERE ID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", idUser);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }
    }
}