using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmAdmin : Form
    {
        private DataGridView dgv;
        private Form frmLogin;
        private TextBox txtSearch;
        private MaskingService bll = new MaskingService();

        public FrmAdmin(Form loginForm)
        {
            frmLogin = loginForm;
            this.Text = "Quản Trị Viên - Dữ Liệu Ẩn Danh (Zero-Knowledge)";
            this.Size = new Size(850, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();

            InitUI();

            this.Load += (s, e) => {
                UIHelper.ApplyModernStyle(this);
                UIHelper.SetPlaceholder(txtSearch, "Nhập họ tên cần tìm...");
                LoadData();
            };
        }

        private void InitUI()
        {
            txtSearch = new TextBox() { Location = new Point(20, 20), Width = 250, Font = new Font("Segoe UI", 10) };
            Button btnSearch = CreateBtn("Tìm Kiếm", 280, 18, 100, 30, Color.ForestGreen);
            Button btnLoad = CreateBtn("Tải Dữ Liệu", 390, 18, 100, 30, Color.MediumSeaGreen);
            Button btnDelete = CreateBtn("Xóa KH", 500, 18, 100, 30, Color.Crimson);
            Button btnExport = CreateBtn("Xuất CSV", 610, 18, 100, 30, Color.DarkOrange);
            Button btnLogout = CreateBtn("Đăng Xuất", 720, 18, 90, 30, Color.Gray);

            dgv = new DataGridView()
            {
                Location = new Point(20, 60),
                Size = new Size(790, 380),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnLoad.Click += (s, e) => LoadData();
            btnLogout.Click += (s, e) => { this.Hide(); frmLogin.Show(); };

            btnDelete.Click += (s, e) => {
                if (dgv.CurrentRow != null && dgv.CurrentRow.Index >= 0)
                {
                    if (MessageBox.Show("Xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        bll.DeleteUser((int)dgv.CurrentRow.Cells["id"].Value);
                        LoadData();
                    }
                }
            };

            btnExport.Click += (s, e) => {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV|*.csv", FileName = "Admin_AnDanh.csv" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        bll.ExportToFile((DataTable)dgv.DataSource, sfd.FileName);
                        MessageBox.Show("Xuất file thành công!");
                    }
                }
            };

            this.Controls.AddRange(new Control[] { dgv, txtSearch, btnSearch, btnLoad, btnDelete, btnExport, btnLogout });
        }

        private void LoadData(string keyword = "") => dgv.DataSource = bll.GetAdminData(keyword);

        private Button CreateBtn(string text, int x, int y, int width, int height, Color bgColor)
        {
            Button b = new Button() { Text = text, Location = new Point(x, y), Width = width, Height = height, BackColor = bgColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}