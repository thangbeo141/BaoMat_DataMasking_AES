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

        public static string CheckLoginAndGetRole(string username, string rawPassword)
        {
            string dbHash = "";
            string dbSalt = "";
            string role = "";

            // 1. Chọc xuống DB lấy Hash, Salt và Role lên trước
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT PasswordHash, PasswordSalt, Role FROM SystemAccounts WHERE Username = @u";
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
                        else
                        {
                            return null; // Không tìm thấy tài khoản
                        }
                    }
                }
            }

            // 2. BĂM MẬT KHẨU + MUỐI BẰNG THUẬT TOÁN SHA-256 DO MÌNH TỰ CODE
            
            string hashInput = CustomSHA256.ComputeHash(rawPassword + dbSalt);

            //// 🔴 ĐẶT BẪY DEBUG Ở ĐÂY 🔴
            //System.Windows.Forms.MessageBox.Show(
            //    $"1. Pass gõ vào: '{rawPassword}'\n" +
            //    $"2. Muối từ DB: '{dbSalt}'\n" +
            //    $"3. Chuỗi đem băm: '{rawPassword + dbSalt}'\n\n" +
            //    $"4. Mã Hash C# tính ra:\n{hashInput}\n\n" +
            //    $"5. Mã Hash đang lưu ở DB:\n{dbHash}",
            //    "Phân tích Lỗi Đăng nhập"
            //);
            // 3. So sánh Hash vừa tính với Hash lưu trong DB
            if (hashInput == dbHash)
            {
                return role; // Khớp 100%, trả về quyền
            }

            return null; // Sai mật khẩu
        }
    }
}