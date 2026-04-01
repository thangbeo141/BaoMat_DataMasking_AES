using System;
using System.Text;

namespace DataMasking
{
    public class KeyManager
    {
        // Hàm public và static để gọi từ bất kỳ đâu
        public static string GenerateDynamicKey(string masterPassword)
        {
            string salt = "ACTVN_Salt_BaoMat_2026";

            // Bước 1: Ghép Mật khẩu và Muối
            string derivedKey = masterPassword + salt;

            // Bước 2: KỸ THUẬT KEY STRETCHING (Chống Brute-force)
            // Chạy vòng lặp băm liên tục 10.000 lần bằng lõi SHA-256 tự code
            for (int i = 0; i < 10000; i++)
            {
                derivedKey = CustomSHA256.ComputeHash(derivedKey);
            }

            // Bước 3: Ép chuẩn đầu ra cho AES-256
            // Kết quả CustomSHA256 là chuỗi Hex dài 64 ký tự.
            // Thuật toán AES-256 yêu cầu khóa dài chính xác 256-bit (tương đương 32 bytes).
            // Ta cắt lấy đúng 32 ký tự đầu tiên để làm chìa khóa.
            return derivedKey.Substring(0, 32);
        }
    }
}