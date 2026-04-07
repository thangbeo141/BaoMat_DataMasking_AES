using System;
using System.Data;
using System.IO;
using System.Text;

namespace DataMasking
{
    public class MaskingService
    {
        // 1. KHÁCH HÀNG ĐĂNG KÝ
        public string RegisterCustomer(string username, string password, string name, string dob, string phone, string email, string cccd, string customDEK)
        {
            string salt = Guid.NewGuid().ToString("N");
            string hash = CustomSHA256.ComputeHash(password + salt);
            string customerKEK = KeyManager.GenerateDynamicKey(password);
            string userDEK = customDEK;

            // Mã hóa thêm ngày sinh
            string encDob = CustomAES.Encrypt(dob, userDEK);
            string encPhone = CustomAES.Encrypt(phone, userDEK);
            string encEmail = CustomAES.Encrypt(email, userDEK);
            string encCCCD = CustomAES.Encrypt(cccd, userDEK);

            string encryptedDEK = CustomAES.Encrypt(userDEK, customerKEK);

            bool isOk = DatabaseHelper.ExecuteInsert(username, hash, salt, "Customer", name, encDob, encPhone, encEmail, encCCCD, encryptedDEK);

            if (isOk) return userDEK;
            return null;
        }

        // 2. ADMIN XEM DỮ LIỆU
        // CẬP NHẬT: Nhận thêm thông tin Admin để tự giải mã hàng tương ứng
        public DataTable GetAdminData(string adminUser, string adminPass, string keyword = "")
        {
            DataTable dt = DatabaseHelper.LayDanhSachUsers(keyword);
            foreach (DataRow row in dt.Rows)
            {
                // NẾU LÀ DÒNG CỦA CHÍNH ADMIN ĐANG ĐĂNG NHẬP -> GIẢI MÃ THẬT
                if (row["Username"].ToString() == adminUser)
                {
                    try
                    {
                        string customerKEK = KeyManager.GenerateDynamicKey(adminPass);
                        string encryptedDEK = row["EncryptedKey"].ToString();
                        string userDEK = CustomAES.Decrypt(encryptedDEK, customerKEK);

                        row["dob"] = CustomAES.Decrypt(row["dob"].ToString(), userDEK);
                        row["phone"] = CustomAES.Decrypt(row["phone"].ToString(), userDEK);
                        row["email"] = CustomAES.Decrypt(row["email"].ToString(), userDEK);
                        row["cccd"] = CustomAES.Decrypt(row["cccd"].ToString(), userDEK);
                    }
                    catch { /* Nếu lỗi thì để mặc định ******** */ }
                }
                else // NẾU LÀ KHÁCH HÀNG KHÁC -> ĐẮP MẶT NẠ ĐEN XÌ
                {
                    row["dob"] = "********";
                    row["phone"] = "********";
                    row["email"] = "********";
                    row["cccd"] = "********";
                }
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
                    string customerKEK = KeyManager.GenerateDynamicKey(myPassword);
                    string encryptedDEK = row["EncryptedKey"].ToString();
                    string userDEK = CustomAES.Decrypt(encryptedDEK, customerKEK);

                    row["dob"] = CustomAES.Decrypt(row["dob"].ToString(), userDEK);
                    row["phone"] = CustomAES.Decrypt(row["phone"].ToString(), userDEK);
                    row["email"] = CustomAES.Decrypt(row["email"].ToString(), userDEK);
                    row["cccd"] = CustomAES.Decrypt(row["cccd"].ToString(), userDEK);
                }
                catch
                {
                    row["dob"] = "LỖI GIẢI MÃ";
                    row["phone"] = "LỖI GIẢI MÃ";
                    row["email"] = "LỖI GIẢI MÃ";
                    row["cccd"] = "LỖI GIẢI MÃ";
                }
            }
            if (dt.Columns.Contains("EncryptedKey")) dt.Columns.Remove("EncryptedKey");
            return dt;
        }
        // BỔ SUNG: Xử lý mã hóa lại dữ liệu mới trước khi cập nhật
        public bool UpdateMyData(string username, string myPassword, string newName, string newDob, string newPhone, string newEmail, string newCccd)
        {
            DataTable dt = DatabaseHelper.LayDuLieuCuaToi(username);
            if (dt.Rows.Count > 0)
            {
                // 1. Tái tạo KEK và mở bao thư lấy DEK
                string customerKEK = KeyManager.GenerateDynamicKey(myPassword);
                string encryptedDEK = dt.Rows[0]["EncryptedKey"].ToString();
                string userDEK = CustomAES.Decrypt(encryptedDEK, customerKEK);

                // 2. Dùng DEK mã hóa toàn bộ dữ liệu mới mà khách vừa sửa
                string encDob = CustomAES.Encrypt(newDob, userDEK);
                string encPhone = CustomAES.Encrypt(newPhone, userDEK);
                string encEmail = CustomAES.Encrypt(newEmail, userDEK);
                string encCccd = CustomAES.Encrypt(newCccd, userDEK);

                // 3. Đẩy xuống Database lưu lại bản mã mới
                return DatabaseHelper.UpdateUser(username, newName, encDob, encPhone, encEmail, encCccd);
            }
            return false;
        }

     
        // 4: ADMIN MỞ KHÓA BẰNG KHÓA CỦA KHÁCH + GHI NHẬT KÝ (AUDIT LOG)
        public string[] UnlockWithCustomerKey(string adminUser, string targetUser, string delegationKey)
        {
            DataTable dt = DatabaseHelper.LayDuLieuCuaToi(targetUser);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string dobCipher = row["dob"].ToString();
                string phoneCipher = row["phone"].ToString();
                string emailCipher = row["email"].ToString();
                string cccdCipher = row["cccd"].ToString();

                string dob = CustomAES.Decrypt(dobCipher, delegationKey);
                string phone = CustomAES.Decrypt(phoneCipher, delegationKey);
                string email = CustomAES.Decrypt(emailCipher, delegationKey);
                string cccd = CustomAES.Decrypt(cccdCipher, delegationKey);

                bool isSuccess = false;
                if (!string.IsNullOrEmpty(dobCipher) && dob != dobCipher) isSuccess = true;
                if (!string.IsNullOrEmpty(phoneCipher) && phone != phoneCipher) isSuccess = true;
                if (!string.IsNullOrEmpty(emailCipher) && email != emailCipher) isSuccess = true;
                if (!string.IsNullOrEmpty(cccdCipher) && cccd != cccdCipher) isSuccess = true;

                if (string.IsNullOrEmpty(dobCipher) && string.IsNullOrEmpty(phoneCipher) && string.IsNullOrEmpty(emailCipher) && string.IsNullOrEmpty(cccdCipher))
                    isSuccess = true;

                if (isSuccess)
                {
                    // VỤT: Ghi lại dấu vết ngay lập tức trước khi trả kết quả!
                    DatabaseHelper.SaveAuditLog(adminUser, targetUser, "Đã giải mã thành công dữ liệu nhạy cảm");

                    // Trả về mảng 4 phần tử
                    return new string[] { dob, phone, email, cccd };
                }
            }
            return null; // Sai khóa
        }

        public void DeleteUser(int id) => DatabaseHelper.XoaUser(id.ToString());

        public void ExportToFile(DataTable dt, string fileName)
        {
            if (dt == null) return;
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                sw.WriteLine("Họ tên,Ngày sinh,Số điện thoại,Email,Số CCCD");
                foreach (DataRow row in dt.Rows)
                {
                    string name = row["name"].ToString();
                    string dob = $"=\"{row["dob"]}\"";
                    string phone = $"=\"{row["phone"]}\"";
                    string email = $"=\"{row["email"]}\"";
                    string cccd = $"=\"{row["cccd"]}\"";
                    sw.WriteLine($"\"{name}\",{dob},{phone},{email},{cccd}");
                }
            }
        }

        // BỔ SUNG: Cơ chế Đảo khóa (Key Rotation)
        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            // 1. Kiểm tra xem mật khẩu cũ gõ đúng chưa
            string role = DatabaseHelper.CheckLoginAndGetRole(username, oldPassword);
            if (role == null) return false; // Báo lỗi nếu sai pass cũ

            DataTable dt = DatabaseHelper.LayDuLieuCuaToi(username);
            if (dt.Rows.Count > 0)
            {
                // 2. Dùng Pass cũ tính ra KEK cũ, mở bao thư lấy Khóa gốc (DEK) ra ngoài
                string oldKEK = KeyManager.GenerateDynamicKey(oldPassword);
                string encryptedDEK = dt.Rows[0]["EncryptedKey"].ToString();
                string userDEK = CustomAES.Decrypt(encryptedDEK, oldKEK);

                // 3. Tính toán Salt, Hash và KEK mới từ Pass mới
                string newSalt = Guid.NewGuid().ToString("N");
                string newHash = CustomSHA256.ComputeHash(newPassword + newSalt);
                string newKEK = KeyManager.GenerateDynamicKey(newPassword);

                // 4. KIẾN TRÚC ENVELOPE: Dùng KEK mới khóa cái DEK lại thành Bao thư mới
                string newEncryptedKey = CustomAES.Encrypt(userDEK, newKEK);

                // 5. Lưu toàn bộ xuống DB (Lưu ý: Không hề phải mã hóa lại SĐT, Email)
                return DatabaseHelper.UpdatePasswordAndKey(username, newHash, newSalt, newEncryptedKey);
            }
            return false;
        }
    }
}