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
        private TextBox txtSearch, txtUnlockKey;
        private MaskingService bll = new MaskingService();
        private string adminUser, adminPass;

        public FrmAdmin(Form loginForm, string user, string pass)
        {
            frmLogin = loginForm;
            this.adminUser = user;
            this.adminPass = pass;

            this.Text = "Quản Trị Viên - Dữ Liệu Ẩn Danh (Zero-Knowledge)";
            this.Size = new Size(900, 520); // Nới form rộng ra chút để chứa thêm nút
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
            // Ép nhỏ lại các nút một xíu để nhét vừa nút Thêm KH
            txtSearch = new TextBox() { Location = new Point(20, 20), Width = 230, Font = new Font("Segoe UI", 10) };
            Button btnSearch = CreateBtn("Tìm Kiếm", 260, 18, 90, 30, Color.ForestGreen);
            Button btnLoad = CreateBtn("Tải Lại", 360, 18, 90, 30, Color.MediumSeaGreen);

            // BỔ SUNG NÚT THÊM KHÁCH HÀNG
            Button btnAdd = CreateBtn("Thêm KH", 460, 18, 90, 30, Color.SteelBlue);

            Button btnDelete = CreateBtn("Xóa KH", 560, 18, 90, 30, Color.Crimson);
            Button btnExport = CreateBtn("Xuất CSV", 660, 18, 90, 30, Color.DarkOrange);
            Button btnLogout = CreateBtn("Đăng Xuất", 760, 18, 90, 30, Color.Gray);
            
            // CỤM CHỨC NĂNG MỞ KHÓA BẰNG KHÓA CỦA KHÁCH
            Label lblUnlock = new Label() { Text = "Khóa do khách cung cấp:", Location = new Point(20, 65), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            txtUnlockKey = new TextBox() { Location = new Point(190, 62), Width = 240 };
            Button btnUnlock = CreateBtn("Mở Khóa Dòng Chọn", 440, 60, 150, 30, Color.Purple);

            // ĐÃ CHUYỂN NÚT NHẬT KÝ XUỐNG ĐỨNG CẠNH NÚT MỞ KHÓA (Tọa độ X = 600, Y = 60)
            Button btnLogs = CreateBtn("Xem Nhật Ký", 600, 60, 120, 30, Color.DarkSlateBlue);
            btnLogs.Click += (s, e) => new FrmAuditLog().ShowDialog();

            dgv = new DataGridView()
            {
                Location = new Point(20, 105),
                Size = new Size(840, 350),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnLoad.Click += (s, e) => LoadData();

            // Đăng xuất thì về form Login thật
            btnLogout.Click += (s, e) => { this.Hide(); frmLogin.Show(); };

            // ==============================================================
            // SỰ KIỆN: NÚT THÊM KHÁCH HÀNG (Tái sử dụng FrmRegister cực kỳ ảo diệu)
            // ==============================================================
            btnAdd.Click += (s, e) => {
                this.Hide(); // Tạm ẩn Form Admin

                // Truyền 'this' (tức là FrmAdmin) vào làm Form cha của FrmRegister.
                // Khi bạn thêm khách xong và Form Register đóng lại, nó sẽ gọi lệnh Show() bật lại Admin!
                new FrmRegister(this).Show();
            };

            // SỰ KIỆN MỞ KHÓA DỮ LIỆU BẰNG KHÓA CẤP QUYỀN
            btnUnlock.Click += (s, e) => {
                if (dgv.CurrentRow != null && dgv.CurrentRow.Index >= 0)
                {
                    string username = dgv.CurrentRow.Cells["Username"].Value.ToString();
                    string key = txtUnlockKey.Text.Trim();

                    if (string.IsNullOrEmpty(key))
                    {
                        MessageBox.Show("Vui lòng nhập khóa ủy quyền do khách hàng cung cấp!", "Yêu cầu khóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Truyền thêm adminUser (biến toàn cục của form) vào để ghi log
                    string[] decryptedData = bll.UnlockWithCustomerKey(adminUser, username, key);

                    if (decryptedData != null)
                    {
                        dgv.CurrentRow.Cells["dob"].Value = decryptedData[0];
                        dgv.CurrentRow.Cells["phone"].Value = decryptedData[1];
                        dgv.CurrentRow.Cells["email"].Value = decryptedData[2];
                        dgv.CurrentRow.Cells["cccd"].Value = decryptedData[3];

                        MessageBox.Show("Mở khóa thành công!\n(Dữ liệu sẽ tự động ẩn đi nếu bạn bấm Tải Lại Dữ Liệu)", "Ủy Quyền Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtUnlockKey.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Khóa không hợp lệ! Không thể giải mã dữ liệu của khách hàng này.", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

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

            // Đừng quên thêm biến btnAdd vào danh sách hiển thị lên màn hình
            this.Controls.AddRange(new Control[] { dgv, txtSearch, btnSearch, btnLoad, btnAdd, btnDelete, btnExport, btnLogout, lblUnlock, txtUnlockKey, btnUnlock,btnLogs});
        }

        private void LoadData(string keyword = "") => dgv.DataSource = bll.GetAdminData(adminUser, adminPass, keyword);

        private Button CreateBtn(string text, int x, int y, int width, int height, Color bgColor)
        {
            Button b = new Button() { Text = text, Location = new Point(x, y), Width = width, Height = height, BackColor = bgColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
    }
}