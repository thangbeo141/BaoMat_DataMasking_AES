using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices; // Dòng này sẽ sáng lên vì ở dưới đã xài tới nó!

namespace DataMasking
{
    public static class UIHelper
    {
        // === 1. PHÉP THUẬT TẠO CHỮ MỜ (PLACEHOLDER) CHÍNH LÀ Ở ĐÂY ===
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public static void SetPlaceholder(TextBox textBox, string text)
        {
            SendMessage(textBox.Handle, 0x1501, (IntPtr)1, text);
        }
        // ==============================================================

        // === 2. CSS TRANG ĐIỂM GIAO DIỆN ===
        public static void ApplyModernStyle(Form form)
        {
            form.Font = new Font("Segoe UI", 10);
            form.BackColor = Color.FromArgb(240, 244, 248);

            foreach (Control ctrl in form.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.ForeColor = Color.White;
                    btn.Cursor = Cursors.Hand;
                    btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                    if (btn.Text.Contains("Thêm") || btn.Text.Contains("Đăng Nhập") || btn.Text == "Tìm Kiếm")
                        btn.BackColor = Color.FromArgb(0, 120, 215);
                    else if (btn.Text.Contains("Đăng Xuất") || btn.Text == "Thoát"|| btn.Text.Contains("Xóa"))
                        btn.BackColor = Color.FromArgb(220, 53, 69);
                    else if (btn.Text.Contains("Xuất"))
                        btn.BackColor = Color.FromArgb(255, 152, 0);
                    else
                        btn.BackColor = Color.FromArgb(40, 167, 69);
                }
                else if (ctrl is TextBox txt)
                {
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (ctrl is DataGridView dgv)
                {
                    dgv.BackgroundColor = Color.White;
                    dgv.BorderStyle = BorderStyle.None;
                    dgv.EnableHeadersVisualStyles = false;

                    // Kẻ dọc tiêu đề
                    dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 57, 85);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    dgv.RowTemplate.Height = 35;

                    // Kẻ dọc thân bảng
                    dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;
                    dgv.GridColor = Color.LightGray;

                    dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
                }
            }
        }
    }
}