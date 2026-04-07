using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace DataMasking
{
    public static class DatabaseHelper
    {
        private static readonly string connString = ConfigurationManager.ConnectionStrings["MySecurityDB"].ConnectionString;

        // 1. Hàm ĐĂNG KÝ: Nhét thêm biến dob (Ngày sinh)
        public static bool ExecuteInsert(string username, string hash, string salt, string role, string name, string dob, string phone, string email, string cccd, string encryptedKey)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "INSERT INTO users (Username, PasswordHash, PasswordSalt, Role, name, dob, phone, email, cccd, EncryptedKey) " +
                                   "VALUES (@u, @h, @s, @r, @n, @d, @p, @e, @c, @k)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@h", hash);
                        cmd.Parameters.AddWithValue("@s", salt);
                        cmd.Parameters.AddWithValue("@r", role);
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@d", dob); // <-- Thêm mới
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

        // 2. ADMIN: Load toàn bộ danh sách (Bao gồm cả Admin và Customer)
        public static DataTable LayDanhSachUsers(string keyword = "")
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    // BÍ KÍP ĐÂY: Đã xóa đoạn "AND Role = 'Customer'" 
                    // Mình lấy thêm cột "Role" để trên Grid Admin phân biệt được ai là Khách, ai là Admin
                    string query = "SELECT id, Username, Role, name, dob, phone, email, cccd, EncryptedKey FROM users WHERE IsActive = 1";

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

        // 3. KHÁCH HÀNG: Chỉ Load dữ liệu của chính họ (Thêm cột dob)
        public static DataTable LayDuLieuCuaToi(string username)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "SELECT name, dob, phone, email, cccd, EncryptedKey FROM users WHERE IsActive = 1 AND Username = @u";
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

        //  KH cập nhật thông tin của mình
        public static bool UpdateUser(string username, string name, string dob, string phone, string email, string cccd)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "UPDATE users SET name = @n, dob = @d, phone = @p, email = @e, cccd = @c WHERE Username = @u AND IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@n", name);
                        cmd.Parameters.AddWithValue("@d", dob); // Ngày sinh
                        cmd.Parameters.AddWithValue("@p", phone);
                        cmd.Parameters.AddWithValue("@e", email);
                        cmd.Parameters.AddWithValue("@c", cccd);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }
        public static string CheckLoginAndGetRole(string username, string rawPassword)
        {
            string dbHash = "", dbSalt = "", role = "";
            using (SqlConnection conn = new SqlConnection(connString))
            {
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

        // BỔ SUNG: Hàm cập nhật Mật khẩu và Bao thư (EncryptedKey) mới
        public static bool UpdatePasswordAndKey(string username, string newHash, string newSalt, string newEncryptedKey)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = "UPDATE users SET PasswordHash = @h, PasswordSalt = @s, EncryptedKey = @k WHERE Username = @u AND IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@h", newHash);
                        cmd.Parameters.AddWithValue("@s", newSalt);
                        cmd.Parameters.AddWithValue("@k", newEncryptedKey);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch { return false; }
        }

        // Lưu vết hành động của Admin
        public static void SaveAuditLog(string admin, string target, string action)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "INSERT INTO audit_logs (admin_user, target_user, action_desc) VALUES (@a, @t, @d)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@a", admin);
                        cmd.Parameters.AddWithValue("@t", target);
                        cmd.Parameters.AddWithValue("@d", action);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        // Lấy danh sách nhật ký
        public static DataTable GetAuditLogs()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "SELECT admin_user [Admin], target_user [Khách hàng], action_desc [Hành động], created_at [Thời gian] FROM audit_logs ORDER BY id DESC";
                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn)) da.Fill(dt);
                }
            }
            catch { }
            return dt;
        }
    }
}