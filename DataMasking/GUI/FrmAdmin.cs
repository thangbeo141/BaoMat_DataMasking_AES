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
        private TextBox txtSearch, txtName, txtPhone, txtEmail, txtCCCD;
        private MaskingService bll = new MaskingService();

        public FrmAdmin(Form loginForm)
        {
            frmLogin = loginForm;
            this.Text = "Quản lý Khách hàng Bảo mật - Học viện KTMM";
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();

            // Khởi tạo toàn bộ giao diện
            InitUI();

            // Áp dụng Style, Chữ mờ và tự động tải dữ liệu khi mở Form
            this.Load += (s, e) => {
                UIHelper.ApplyModernStyle(this);

                // PHỤC HỒI LẠI CÁC CHỮ MỜ (PLACEHOLDER) TẠI ĐÂY:
                UIHelper.SetPlaceholder(txtName, "Ví dụ: Nguyễn Văn A");
                UIHelper.SetPlaceholder(txtPhone, "09xxxx...");
                UIHelper.SetPlaceholder(txtEmail, "abc@gmail.com");
                UIHelper.SetPlaceholder(txtCCCD, "12 số căn cước...");
                UIHelper.SetPlaceholder(txtSearch, "Nhập họ tên cần tìm...");

                LoadData();
            };
        }

        private void InitUI()
        {
            // Hàng 1: Họ tên và Số điện thoại
            AddLabel("Họ tên:", 20, 20);
            txtName = AddTextBox(80, 17, 150);
            AddLabel("Số ĐT:", 250, 20);
            txtPhone = AddTextBox(310, 17, 130);

            // Hàng 2: Email và CCCD
            AddLabel("Email:", 20, 55);
            txtEmail = AddTextBox(80, 52, 150);
            AddLabel("CCCD:", 250, 55);
            txtCCCD = AddTextBox(310, 52, 130);

            // Cụm Nút Thêm, Xóa, Đăng xuất (Góc trên phải)
            Button btnAdd = CreateBtn("Thêm Khách Hàng", 460, 15, 140, 60, Color.DodgerBlue);
            Button btnDelete = CreateBtn("Xóa Khách Hàng", 610, 15, 120, 60, Color.Crimson);
            Button btnLogout = CreateBtn("Đăng Xuất", 740, 15, 80, 60, Color.Gray);

            // Cụm Tìm kiếm, Tải dữ liệu & Xuất file (Nằm giữa)
            txtSearch = AddTextBox(20, 110, 250);
            Button btnSearch = CreateBtn("Tìm Kiếm", 280, 107, 100, 30, Color.ForestGreen);
            Button btnExport = CreateBtn("Xuất CSV", 390, 107, 100, 30, Color.DarkOrange);
            Button btnLoad = CreateBtn("Tải Dữ Liệu", 500, 107, 100, 30, Color.MediumSeaGreen);

            // Bảng hiển thị dữ liệu
            dgv = new DataGridView()
            {
                Location = new Point(20, 150),
                Size = new Size(790, 380),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // ==========================================
            // GÁN SỰ KIỆN CHO CÁC NÚT BẤM
            // ==========================================

            btnAdd.Click += (s, e) => {
                try
                {
                    bll.AddNewUser(txtName.Text, txtPhone.Text, txtEmail.Text, txtCCCD.Text);
                    MessageBox.Show("Thêm khách hàng và mã hóa AES thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    txtName.Text = txtPhone.Text = txtEmail.Text = txtCCCD.Text = ""; // Xóa rỗng các ô nhập sau khi thêm
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            btnDelete.Click += (s, e) => {
                if (dgv.CurrentRow != null && dgv.CurrentRow.Index >= 0)
                {
                    if (MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        bll.DeleteUser((int)dgv.CurrentRow.Cells["id"].Value);
                        LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một dòng trong bảng để xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);

            btnLoad.Click += (s, e) => LoadData(); // Nút Refresh dữ liệu

            btnExport.Click += (s, e) => {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV File|*.csv", FileName = "BaoCao_Admin.csv" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        bll.ExportToFile((DataTable)dgv.DataSource, sfd.FileName);
                        MessageBox.Show("Đã xuất file thành công!", "Thông báo");
                    }
                }
            };

            btnLogout.Click += (s, e) => { this.Hide(); frmLogin.Show(); };

            // Đưa tất cả các control lên Form
            this.Controls.AddRange(new Control[] { dgv, btnAdd, btnDelete, btnLogout, txtSearch, btnSearch, btnExport, btnLoad });
        }

        // ==========================================
        // CÁC HÀM HỖ TRỢ VẼ GIAO DIỆN NHANH
        // ==========================================

        private void LoadData(string keyword = "") => dgv.DataSource = bll.GetAdminData(keyword);

        private void AddLabel(string text, int x, int y) => this.Controls.Add(new Label() { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });

        private TextBox AddTextBox(int x, int y, int width)
        {
            TextBox t = new TextBox() { Location = new Point(x, y), Width = width, Font = new Font("Segoe UI", 9) };
            this.Controls.Add(t);
            return t;
        }

        private Button CreateBtn(string text, int x, int y, int width, int height, Color bgColor)
        {
            Button b = new Button() { Text = text, Location = new Point(x, y), Width = width, Height = height, BackColor = bgColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            this.Controls.Add(b);
            return b;
        }
    }
}