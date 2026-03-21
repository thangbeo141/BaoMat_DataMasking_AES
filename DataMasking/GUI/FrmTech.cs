using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmTech : Form
    {
        DataGridView dgv;
        Form frmLogin;
        TextBox txtSearch;
        MaskingService bll = new MaskingService();

        public FrmTech(Form loginForm)
        {
            frmLogin = loginForm;
            this.Text = "Hệ Thống Nhân Viên (Đã Che Giấu Dữ Liệu An Toàn)";
            this.Size = new Size(820, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();

            // 1. CỤM NÚT TẢI & TÌM KIẾM
            Button btnLoad = new Button() { Text = "Tải Danh Sách", Location = new Point(20, 20), Width = 120, Height = 35 };

            txtSearch = new TextBox() { Location = new Point(160, 25), Width = 200 };
            Button btnSearch = new Button() { Text = "Tìm Kiếm", Location = new Point(370, 20), Width = 90, Height = 35 };

            // 2. CỤM NÚT XUẤT & ĐĂNG XUẤT
            Button btnExport = new Button() { Text = "Xuất Trang Tính", Location = new Point(480, 20), Width = 140, Height = 35 };
            Button btnLogout = new Button() { Text = "Đăng Xuất", Location = new Point(700, 20), Width = 90, Height = 35 };
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // 3. BẢNG DỮ LIỆU 
            dgv = new DataGridView()
            {
                Location = new Point(20, 80),
                Size = new Size(770, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToResizeColumns = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // 4. SỰ KIỆN NÚT BẤM
            btnLoad.Click += (s, e) => LoadData();
            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnLogout.Click += (s, e) => { this.Hide(); frmLogin.Show(); };

            btnExport.Click += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Bảng tính CSV (*.csv)|*.csv", FileName = "Tech_DataCheGiau.csv" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            bll.ExportToFile((DataTable)dgv.DataSource, sfd.FileName);
                            MessageBox.Show("Đã bảo mật và xuất file thành công tại:\n" + sfd.FileName);
                        }
                        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                    }
                }
            };

            this.Controls.AddRange(new Control[] { btnLoad, txtSearch, btnSearch, btnExport, btnLogout, dgv });

            // 5. SỰ KIỆN LOAD (Đổ CSS và chữ mờ tìm kiếm)
            this.Load += (s, e) =>
            {
                UIHelper.ApplyModernStyle(this);
                UIHelper.SetPlaceholder(txtSearch, "Nhập họ tên cần tìm...");
            };
        }

        private void LoadData(string keyword = "")
        {
            dgv.DataSource = bll.GetTechData(keyword);
        }
    }
}