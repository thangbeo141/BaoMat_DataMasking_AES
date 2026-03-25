using System;
using System.Data;
using System.IO;
using System.Text;

namespace DataMasking
{
    public class MaskingService
    {
        // Khóa bí mật dùng cho thuật toán AES tự chế của bạn
        private readonly string myKey = "HocVienKyThuatMatMa_SuperKey123";

        private string EncryptAES(string plainText) => CustomAES.Encrypt(plainText, myKey);
        private string DecryptAES(string cipherText) => CustomAES.Decrypt(cipherText, myKey);

        public void AddNewUser(string name, string phone, string email, string cccd)
        {
            // MÃ HÓA TẤT CẢ TRƯỚC KHI LƯU XUỐNG DB (Data-at-Rest)
            DatabaseHelper.ExecuteInsert(name, EncryptAES(phone), EncryptAES(email), EncryptAES(cccd));
        }

        public DataTable GetAdminData(string keyword = "")
        {
            DataTable dt = DatabaseHelper.ExecuteSelect(keyword);
            foreach (DataRow row in dt.Rows)
            {
                // ADMIN: Giải mã để xem dữ liệu gốc cho cả 3 trường
                row["phone"] = DecryptAES(row["phone"].ToString());
                row["email"] = DecryptAES(row["email"].ToString());
                row["cccd"] = DecryptAES(row["cccd"].ToString());
            }
            return dt;
        }

        public DataTable GetTechData(string keyword = "")
        {
            DataTable dt = DatabaseHelper.ExecuteSelect(keyword);
            foreach (DataRow row in dt.Rows)
            {
                // NHÂN VIÊN: Giải mã xong đắp mặt nạ ngay (Data-in-Use)
                row["phone"] = MaskPhone(DecryptAES(row["phone"].ToString()));
                row["email"] = MaskEmail(DecryptAES(row["email"].ToString()));
                row["cccd"] = MaskCCCD(DecryptAES(row["cccd"].ToString()));
            }
            return dt;
        }

        // Quy tắc mặt nạ cho 3 loại dữ liệu nhạy cảm
        private string MaskPhone(string p) => (p.Length > 6) ? p.Substring(0, 3) + "***" + p.Substring(p.Length - 3) : p;
        private string MaskEmail(string e) => e.Contains("@") ? e.Split('@')[0].Substring(0, 1) + "***@" + e.Split('@')[1] : e;
        private string MaskCCCD(string c) => (c.Length > 6) ? c.Substring(0, 6) + "******" : c;

        public void DeleteUser(int id) => DatabaseHelper.ExecuteDelete(id);

        public void ExportToFile(DataTable dt, string fileName)
        {
            if (dt == null) return;
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                sw.WriteLine("Họ tên,Số điện thoại,Email,Số CCCD");
                foreach (DataRow row in dt.Rows)
                    sw.WriteLine($"\"{row["name"]}\",\"{row["phone"]}\",\"{row["email"]}\",\"{row["cccd"]}\"");
            }
        }
    }
}