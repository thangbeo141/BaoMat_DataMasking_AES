using System;
using System.Text;
using System.Collections.Generic;

namespace DataMasking
{
    public class CustomSHA256
    {
        // 64 hằng số K chuẩn của SHA-256 (Cấp bởi NSA)
        private static readonly uint[] K = {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };

        // Các phép toán dịch bit (Bitwise Operations) cốt lõi
        private static uint ROTR(uint x, int n) => (x >> n) | (x << (32 - n));
        private static uint Ch(uint x, uint y, uint z) => (x & y) ^ (~x & z);
        private static uint Maj(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);
        private static uint Sigma0(uint x) => ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);
        private static uint Sigma1(uint x) => ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);
        private static uint sigma0(uint x) => ROTR(x, 7) ^ ROTR(x, 18) ^ (x >> 3);
        private static uint sigma1(uint x) => ROTR(x, 17) ^ ROTR(x, 19) ^ (x >> 10);

        public static string ComputeHash(string input)
        {
            byte[] message = Encoding.UTF8.GetBytes(input);
            ulong originalBits = (ulong)message.Length * 8;

            // 1. PADDING (Đệm dữ liệu cho tròn block 512 bits)
            List<byte> padded = new List<byte>(message);
            padded.Add(0x80); // Thêm bit 1 (tương đương 10000000 trong nhị phân)
            while ((padded.Count * 8) % 512 != 448) padded.Add(0x00); // Thêm các bit 0

            // Gắn chiều dài gốc (64-bit) vào cuối cùng (chuẩn Big Endian)
            byte[] lengthBytes = BitConverter.GetBytes(originalBits);
            if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
            padded.AddRange(lengthBytes);

            // 2. GIÁ TRỊ KHỞI TẠO (Initial Hash Values)
            uint[] H = {
                0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
                0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
            };

            // 3. XỬ LÝ TỪNG CHUNK 512 BITS (64 BYTES)
            for (int chunkStart = 0; chunkStart < padded.Count; chunkStart += 64)
            {
                uint[] W = new uint[64]; // Message Schedule (64 words)

                // Nạp 16 words đầu tiên
                for (int t = 0; t < 16; t++)
                {
                    W[t] = (uint)((padded[chunkStart + t * 4] << 24) |
                                  (padded[chunkStart + t * 4 + 1] << 16) |
                                  (padded[chunkStart + t * 4 + 2] << 8) |
                                  (padded[chunkStart + t * 4 + 3]));
                }

                // Tính toán 48 words còn lại
                for (int t = 16; t < 64; t++)
                    W[t] = sigma1(W[t - 2]) + W[t - 7] + sigma0(W[t - 15]) + W[t - 16];

                // Khởi tạo Working Variables
                uint a = H[0], b = H[1], c = H[2], d = H[3], e = H[4], f = H[5], g = H[6], h = H[7];

                // Vòng lặp nén (Compression loop) 64 vòng
                for (int t = 0; t < 64; t++)
                {
                    uint T1 = h + Sigma1(e) + Ch(e, f, g) + K[t] + W[t];
                    uint T2 = Sigma0(a) + Maj(a, b, c);
                    h = g; g = f; f = e; e = d + T1;
                    d = c; c = b; b = a; a = T1 + T2;
                }

                // Cộng dồn vào Hash chính
                H[0] += a; H[1] += b; H[2] += c; H[3] += d;
                H[4] += e; H[5] += f; H[6] += g; H[7] += h;
            }

            // 4. GHÉP CHUỖI KẾT QUẢ (Dạng Hex)
            StringBuilder sb = new StringBuilder();
            foreach (uint hVal in H) sb.Append(hVal.ToString("x8"));
            return sb.ToString();
        }
    }
}