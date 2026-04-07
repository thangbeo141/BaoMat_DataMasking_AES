using System;
using System.Text;
using System.Collections.Generic;

namespace DataMasking
{
    public class CustomAES
    {
        // 1. MA TRẬN S-BOX (SubBytes)
        private static readonly byte[] Sbox = new byte[256] {
            0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76,
            0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0,
            0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15,
            0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75,
            0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84,
            0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf,
            0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8,
            0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2,
            0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73,
            0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb,
            0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79,
            0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08,
            0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a,
            0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e,
            0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf,
            0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16
        };

        // 2. MA TRẬN IS-BOX (InvSubBytes)
        private static readonly byte[] InvSbox = new byte[256] {
            0x52, 0x09, 0x6a, 0xd5, 0x30, 0x36, 0xa5, 0x38, 0xbf, 0x40, 0xa3, 0x9e, 0x81, 0xf3, 0xd7, 0xfb,
            0x7c, 0xe3, 0x39, 0x82, 0x9b, 0x2f, 0xff, 0x87, 0x34, 0x8e, 0x43, 0x44, 0xc4, 0xde, 0xe9, 0xcb,
            0x54, 0x7b, 0x94, 0x32, 0xa6, 0xc2, 0x23, 0x3d, 0xee, 0x4c, 0x95, 0x0b, 0x42, 0xfa, 0xc3, 0x4e,
            0x08, 0x2e, 0xa1, 0x66, 0x28, 0xd9, 0x24, 0xb2, 0x76, 0x5b, 0xa2, 0x49, 0x6d, 0x8b, 0xd1, 0x25,
            0x72, 0xf8, 0xf6, 0x64, 0x86, 0x68, 0x98, 0x16, 0xd4, 0xa4, 0x5c, 0xcc, 0x5d, 0x65, 0xb6, 0x92,
            0x6c, 0x70, 0x48, 0x50, 0xfd, 0xed, 0xb9, 0xda, 0x5e, 0x15, 0x46, 0x57, 0xa7, 0x8d, 0x9d, 0x84,
            0x90, 0xd8, 0xab, 0x00, 0x8c, 0xbc, 0xd3, 0x0a, 0xf7, 0xe4, 0x58, 0x05, 0xb8, 0xb3, 0x45, 0x06,
            0xd0, 0x2c, 0x1e, 0x8f, 0xca, 0x3f, 0x0f, 0x02, 0xc1, 0xaf, 0xbd, 0x03, 0x01, 0x13, 0x8a, 0x6b,
            0x3a, 0x91, 0x11, 0x41, 0x4f, 0x67, 0xdc, 0xea, 0x97, 0xf2, 0xcf, 0xce, 0xf0, 0xb4, 0xe6, 0x73,
            0x96, 0xac, 0x74, 0x22, 0xe7, 0xad, 0x35, 0x85, 0xe2, 0xf9, 0x37, 0xe8, 0x1c, 0x75, 0xdf, 0x6e,
            0x47, 0xf1, 0x1a, 0x71, 0x1d, 0x29, 0xc5, 0x89, 0x6f, 0xb7, 0x62, 0x0e, 0xaa, 0x18, 0xbe, 0x1b,
            0xfc, 0x56, 0x3e, 0x4b, 0xc6, 0xd2, 0x79, 0x20, 0x9a, 0xdb, 0xc0, 0xfe, 0x78, 0xcd, 0x5a, 0xf4,
            0x1f, 0xdd, 0xa8, 0x33, 0x88, 0x07, 0xc7, 0x31, 0xb1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xec, 0x5f,
            0x60, 0x51, 0x7f, 0xa9, 0x19, 0xb5, 0x4a, 0x0d, 0x2d, 0xe5, 0x7a, 0x9f, 0x93, 0xc9, 0x9c, 0xef,
            0xa0, 0xe0, 0x3b, 0x4d, 0xae, 0x2a, 0xf5, 0xb0, 0xc8, 0xeb, 0xbb, 0x3c, 0x83, 0x53, 0x99, 0x61,
            0x17, 0x2b, 0x04, 0x7e, 0xba, 0x77, 0xd6, 0x26, 0xe1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0c, 0x7d
        };

        // 3. Rcon
        private static readonly byte[] Rcon = new byte[11] { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36 };

        // 4. TOÁN HỌC TRÊN TRƯỜNG GALOIS
        private static byte GMul(byte a, byte b) //Nhân 2 số trong GF(2^8) 
        {
            byte p = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((b & 1) != 0) p ^= a;
                bool hiBitSet = (a & 0x80) != 0;
                a <<= 1;
                if (hiBitSet) a ^= 0x1b;
                b >>= 1;
            }
            return p;
        }

        private static void SubBytes(byte[,] state)
        {
            for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) state[i, j] = Sbox[state[i, j]];
        }

        private static void InvSubBytes(byte[,] state)
        {
            for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) state[i, j] = InvSbox[state[i, j]];
        }

        private static void ShiftRows(byte[,] state) 
        {
            byte[] temp = new byte[4];
            for (int i = 1; i < 4; i++) //dòng 0 giữ nguyên, dòng 1→3 dịch trái
            {
                for (int j = 0; j < 4; j++) temp[j] = state[i, (j + i) % 4];//dịch trái i bước
                for (int j = 0; j < 4; j++) state[i, j] = temp[j];
            }
        }

        private static void InvShiftRows(byte[,] state)
        {
            byte[] temp = new byte[4];
            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4; j++) temp[j] = state[i, (j - i + 4) % 4];//dịch phải i bước
                for (int j = 0; j < 4; j++) state[i, j] = temp[j];
            }
        }

        private static void MixColumns(byte[,] state)//Nhân + XOR trong GF(2^8)
        {
            byte[] temp = new byte[4];
            for (int c = 0; c < 4; c++)
            {
                temp[0] = (byte)(GMul(0x02, state[0, c]) ^ GMul(0x03, state[1, c]) ^ state[2, c] ^ state[3, c]);
                temp[1] = (byte)(state[0, c] ^ GMul(0x02, state[1, c]) ^ GMul(0x03, state[2, c]) ^ state[3, c]);
                temp[2] = (byte)(state[0, c] ^ state[1, c] ^ GMul(0x02, state[2, c]) ^ GMul(0x03, state[3, c]));
                temp[3] = (byte)(GMul(0x03, state[0, c]) ^ state[1, c] ^ state[2, c] ^ GMul(0x02, state[3, c]));
                for (int i = 0; i < 4; i++) state[i, c] = temp[i];
            }
        }

        private static void InvMixColumns(byte[,] state)//ma trận đảo của MixColumns
        {
            byte[] temp = new byte[4];
            for (int c = 0; c < 4; c++)
            {
                temp[0] = (byte)(GMul(0x0e, state[0, c]) ^ GMul(0x0b, state[1, c]) ^ GMul(0x0d, state[2, c]) ^ GMul(0x09, state[3, c]));
                temp[1] = (byte)(GMul(0x09, state[0, c]) ^ GMul(0x0e, state[1, c]) ^ GMul(0x0b, state[2, c]) ^ GMul(0x0d, state[3, c]));
                temp[2] = (byte)(GMul(0x0d, state[0, c]) ^ GMul(0x09, state[1, c]) ^ GMul(0x0e, state[2, c]) ^ GMul(0x0b, state[3, c]));
                temp[3] = (byte)(GMul(0x0b, state[0, c]) ^ GMul(0x0d, state[1, c]) ^ GMul(0x09, state[2, c]) ^ GMul(0x0e, state[3, c]));
                for (int i = 0; i < 4; i++) state[i, c] = temp[i];
            }
        }

        private static void AddRoundKey(byte[,] state, byte[,] roundKey)
        {
            for (int c = 0; c < 4; c++) for (int r = 0; r < 4; r++) state[r, c] ^= roundKey[r, c];//XOR với round key
        }

        private static byte[][,] KeyExpansion(byte[] key)//tạo key cho từng round
        {
            int Nk = key.Length / 4;
            int Nr = Nk + 6;
            byte[] w = new byte[4 * 4 * (Nr + 1)];
            for (int i = 0; i < key.Length; i++) w[i] = key[i];
            for (int i = Nk; i < 4 * (Nr + 1); i++)
            {
                byte[] temp = new byte[4];
                for (int j = 0; j < 4; j++) temp[j] = w[(i - 1) * 4 + j];
                if (i % Nk == 0)
                {
                    byte t = temp[0]; temp[0] = temp[1]; temp[1] = temp[2]; temp[2] = temp[3]; temp[3] = t;
                    for (int j = 0; j < 4; j++) temp[j] = Sbox[temp[j]];
                    temp[0] ^= Rcon[i / Nk];
                }
                else if (Nk > 6 && i % Nk == 4) for (int j = 0; j < 4; j++) temp[j] = Sbox[temp[j]];
                for (int j = 0; j < 4; j++) w[i * 4 + j] = (byte)(w[(i - Nk) * 4 + j] ^ temp[j]);
            }
            byte[][,] roundKeys = new byte[Nr + 1][,];
            for (int round = 0; round <= Nr; round++)
            {
                roundKeys[round] = new byte[4, 4];
                for (int c = 0; c < 4; c++) for (int r = 0; r < 4; r++) roundKeys[round][r, c] = w[(round * 4 + c) * 4 + r];
            }
            return roundKeys;
        }

        private static byte[] EncryptBlock(byte[] input, byte[][,] roundKeys)
        {
            byte[,] state = new byte[4, 4];//Chuyển input 16 byte thành ma trận 4x4
            for (int c = 0; c < 4; c++) for (int r = 0; r < 4; r++) state[r, c] = input[c * 4 + r];
            int Nr = roundKeys.Length - 1;
            AddRoundKey(state, roundKeys[0]);//
            for (int round = 1; round < Nr; round++)
            {
                SubBytes(state); //Thay thế mỗi byte bằng giá trị tương ứng trong S-box
                ShiftRows(state);//Dịch hàng 1→3 sang trái 1→3 bước
                MixColumns(state);//Trộn cột bằng cách nhân ma trận với ma trận hằng số
                AddRoundKey(state, roundKeys[round]);//XOR với round key
            }
            SubBytes(state);
            ShiftRows(state); 
            AddRoundKey(state, roundKeys[Nr]);
            byte[] output = new byte[16];
            for (int c = 0; c < 4; c++) for (int r = 0; r < 4; r++) output[c * 4 + r] = state[r, c];
            return output;
        }

        private static byte[] DecryptBlock(byte[] input, byte[][,] roundKeys)
        {
            byte[,] state = new byte[4, 4];
            for (int c = 0; c < 4; c++) for (int r = 0; r < 4; r++) state[r, c] = input[c * 4 + r];
            int Nr = roundKeys.Length - 1;

            // THỨ TỰ GIẢI MÃ CHUẨN:
            AddRoundKey(state, roundKeys[Nr]);
            InvShiftRows(state);
            InvSubBytes(state);

            for (int round = Nr - 1; round >= 1; round--)
            {
                AddRoundKey(state, roundKeys[round]);
                InvMixColumns(state);
                InvShiftRows(state);
                InvSubBytes(state);
            }
            AddRoundKey(state, roundKeys[0]);

            byte[] output = new byte[16];
            for (int c = 0; c < 4; c++) for (int r = 0; r < 4; r++) output[c * 4 + r] = state[r, c];
            return output;
        }

        public static string Encrypt(string plainText, string keyString)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            // Lấy chính xác số ký tự khóa khách hàng nhập vào (16, 24 hoặc 32)
            byte[] key = Encoding.UTF8.GetBytes(keyString);
            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                throw new ArgumentException("Khóa AES phải dài đúng 16, 24 hoặc 32 byte!");

            byte[][,] roundKeys = KeyExpansion(key);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            int paddingLength = 16 - (plainBytes.Length % 16);
            byte[] paddedBytes = new byte[plainBytes.Length + paddingLength];
            Array.Copy(plainBytes, paddedBytes, plainBytes.Length);
            for (int i = plainBytes.Length; i < paddedBytes.Length; i++) paddedBytes[i] = (byte)paddingLength;
            List<byte> cipherBytes = new List<byte>();
            for (int i = 0; i < paddedBytes.Length; i += 16)
            {
                byte[] block = new byte[16];
                Array.Copy(paddedBytes, i, block, 0, 16);
                cipherBytes.AddRange(EncryptBlock(block, roundKeys));
            }
            return Convert.ToBase64String(cipherBytes.ToArray());
        }

        public static string Decrypt(string cipherText, string keyString)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(keyString);
                // Bắt lỗi nếu Admin nhập sai độ dài khóa
                if (key.Length != 16 && key.Length != 24 && key.Length != 32) return cipherText;

                byte[][,] roundKeys = KeyExpansion(key);
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                List<byte> plainBytes = new List<byte>();
                for (int i = 0; i < cipherBytes.Length; i += 16)
                {
                    byte[] block = new byte[16];
                    Array.Copy(cipherBytes, i, block, 0, 16);
                    plainBytes.AddRange(DecryptBlock(block, roundKeys));
                }
                int paddingLength = plainBytes[plainBytes.Count - 1];
                plainBytes.RemoveRange(plainBytes.Count - paddingLength, paddingLength);
                return Encoding.UTF8.GetString(plainBytes.ToArray());
            }
            catch { return cipherText; }
        }
    }
}