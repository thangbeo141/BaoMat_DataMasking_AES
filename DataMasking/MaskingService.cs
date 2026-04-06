using System;
using System.Data;
using System.IO;
using System.Text;

namespace DataMasking
{
    public class MaskingService
    {
        // 1. KHÁCH HÀNG ĐĂNG KÝ TÀI KHOẢN MỚI
        public bool RegisterCustomer(string username, string password, string name, string phone, string email, string cccd)
        {
            // --- PHẦN A: BẢO MẬT ĐĂNG NHẬP ---
            string salt = Guid.NewGuid().ToString("N");
            string hash = CustomSHA256.ComputeHash(password + salt); // Băm mật khẩu để login

            // --- PHẦN B: BẢO MẬT DỮ LIỆU (ZERO-KNOWLEDGE) ---
            string customerKEK = KeyManager.GenerateDynamicKey(password); // Khóa KEK sinh từ Pass
            string userDEK = Guid.NewGuid().ToString("N"); // Khóa DEK dùng để mã hóa Data

            string encPhone = CustomAES.Encrypt(phone, userDEK);
            string encEmail = CustomAES.Encrypt(email, userDEK);
            string encCCCD = CustomAES.Encrypt(cccd, userDEK);

            string encryptedDEK = CustomAES.Encrypt(userDEK, customerKEK); // Khóa DEK lại bằng KEK

            // Lưu tất cả xuống DB với quyền là 'Customer'
            return DatabaseHelper.ExecuteInsert(username, hash, salt, "Customer", name, encPhone, encEmail, encCCCD, encryptedDEK);
        }

        // 2. ADMIN XEM DỮ LIỆU (BỊ MÙ TRƯỚC DỮ LIỆU NHẠY CẢM)
        public DataTable GetAdminData(string keyword = "")
        {
            DataTable dt = DatabaseHelper.LayDanhSachUsers(keyword);
            foreach (DataRow row in dt.Rows)
            {
                // Admin không có mật khẩu của khách -> Không có KEK -> Bất lực toàn tập
                row["phone"] = "********";
                row["email"] = "********";
                row["cccd"] = "********";
            }

            if (dt.Columns.Contains("EncryptedKey")) dt.Columns.Remove("EncryptedKey");
            return dt;
        }

        // 3. KHÁCH HÀNG XEM DỮ LIỆU CỦA CHÍNH MÌNH
        public DataTable GetMyData(string username, string myPassword)
        {
            DataTable dt = DatabaseHelper.LayDuLieuCuaToi(username);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                try
                {
                    // Lấy mật khẩu khách vừa nhập để tái tạo KEK và mở bao thư
                    string customerKEK = KeyManager.GenerateDynamicKey(myPassword);
                    string encryptedDEK = row["EncryptedKey"].ToString();
                    string userDEK = CustomAES.Decrypt(encryptedDEK, customerKEK);

                    // Giải mã thành công
                    row["phone"] = CustomAES.Decrypt(row["phone"].ToString(), userDEK);
                    row["email"] = CustomAES.Decrypt(row["email"].ToString(), userDEK);
                    row["cccd"] = CustomAES.Decrypt(row["cccd"].ToString(), userDEK);
                }
                catch
                {
                    row["phone"] = "LỖI GIẢI MÃ";
                    row["email"] = "LỖI GIẢI MÃ";
                    row["cccd"] = "LỖI GIẢI MÃ";
                }
            }

            if (dt.Columns.Contains("EncryptedKey")) dt.Columns.Remove("EncryptedKey");
            return dt;
        }

        // =======================================================
        // 4. HAI HÀM NÀY BỊ THIẾU NÊN FRMADMIN BÁO LỖI NÀY THẮNG ƠI
        // =======================================================

        public void DeleteUser(int id) => DatabaseHelper.XoaUser(id.ToString());

        public void ExportToFile(DataTable dt, string fileName)
        {
            if (dt == null) return;
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                sw.WriteLine("Họ tên,Số điện thoại,Email,Số CCCD");
                foreach (DataRow row in dt.Rows)
                {
                    string name = row["name"].ToString();
                    string phone = $"=\"{row["phone"]}\"";
                    string email = $"=\"{row["email"]}\"";
                    string cccd = $"=\"{row["cccd"]}\"";
                    sw.WriteLine($"\"{name}\",{phone},{email},{cccd}");
                }
            }
        }
    }
}