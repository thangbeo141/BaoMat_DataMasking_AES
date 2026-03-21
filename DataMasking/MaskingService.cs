using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DataMasking
{
    public static class AppConfig
    {
        public static int MaskLevel = 1; // 1 = Che nhẹ, 2 = Che nặng
    }

    public class MaskingService
    {
        // =========================================================
        // KHỐI THUẬT TOÁN MÃ HÓA AES-256
        // =========================================================
        private readonly string aesKey = "TruongKyThuatMatMa_AES256Key1234"; // Khóa bí mật 32 bytes
        private readonly string aesIV = "InitVector123456";              // Véc-tơ khởi tạo 16 bytes

        private string EncryptAES(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(aesKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(aesIV);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private string DecryptAES(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(aesKey);
                    aesAlg.IV = Encoding.UTF8.GetBytes(aesIV);
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            // Nếu dữ liệu cũ trong DB không phải AES (như các khách hàng bạn thêm từ trước), trả về nguyên gốc
            catch { return cipherText; }
        }

        // =========================================================
        // KHỐI XỬ LÝ NGHIỆP VỤ & MẶT NẠ (MASKING)
        // =========================================================
        public void AddNewUser(string name, string phone, string email)
        {
            // MÃ HÓA BẢO MẬT TRƯỚC KHI ĐƯA XUỐNG CSDL
            string encryptedPhone = EncryptAES(phone);
            string encryptedEmail = EncryptAES(email);
            DatabaseHelper.ExecuteInsert(name, encryptedPhone, encryptedEmail);
        }

        public void DeleteUser(int id)
        {
            DatabaseHelper.ExecuteDelete(id);
        }

        public DataTable GetAdminData(string keyword = "")
        {
            DataTable dt = DatabaseHelper.ExecuteSelect(keyword);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    // ADMIN: Chỉ Giải mã (Decrypt) để xem thông tin gốc, KHÔNG che mặt nạ
                    row["phone"] = DecryptAES(row["phone"].ToString());
                    row["email"] = DecryptAES(row["email"].ToString());
                }
            }
            return dt;
        }

        public DataTable GetTechData(string keyword = "")
        {
            DataTable dt = DatabaseHelper.ExecuteSelect(keyword);
            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    // KỸ THUẬT VIÊN: Bước 1 - Giải mã dữ liệu từ CSDL
                    string rawPhone = DecryptAES(row["phone"].ToString());
                    string rawEmail = DecryptAES(row["email"].ToString());

                    // Bước 2 - Đeo mặt nạ dữ liệu (Data Masking) trước khi hiển thị
                    string noisedPhone = InjectNoise(rawPhone);
                    row["phone"] = MaskPhone(noisedPhone);
                    row["email"] = MaskEmail(rawEmail);
                }
            }
            return dt;
        }

        private string InjectNoise(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            char[] oldChars = input.ToCharArray();
            char[] newChars = new char[oldChars.Length + 3];
            for (int i = 0; i < oldChars.Length; i++) newChars[i] = oldChars[i];
            newChars[oldChars.Length] = '#'; newChars[oldChars.Length + 1] = '#'; newChars[oldChars.Length + 2] = '#';
            return new string(newChars);
        }

        private string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 6) return phone;
            char[] chars = phone.ToCharArray();
            if (AppConfig.MaskLevel == 1)
                for (int i = 3; i < chars.Length - 3; i++) chars[i] = '*';
            else
                for (int i = 0; i < chars.Length - 2; i++) chars[i] = '*';
            return new string(chars);
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return email;
            char[] chars = email.ToCharArray();
            int atIndex = -1;
            for (int i = 0; i < chars.Length; i++) { if (chars[i] == '@') { atIndex = i; break; } }
            if (atIndex <= 1) return email;
            for (int i = 1; i < atIndex; i++) chars[i] = '*';
            return new string(chars);
        }

        public void ExportToFile(DataTable dt, string fileName)
        {
            if (dt == null) return;
            using (StreamWriter sw = new StreamWriter(fileName, false, new System.Text.UTF8Encoding(true)))
            {
                sw.WriteLine("Họ tên,Số điện thoại,Email");
                foreach (DataRow row in dt.Rows)
                {
                    string name = $"\"{row["name"]}\"";
                    string phone = $"\"{row["phone"]}\"";
                    string email = $"\"{row["email"]}\"";
                    sw.WriteLine($"{name},{phone},{email}");
                }
            }
        }
    }
}