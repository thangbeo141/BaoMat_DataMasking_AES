using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmAdmin : Form
    {
        DataGridView dgv;
        Form frmLogin;
        TextBox txtSearch, txtName, txtPhone, txtEmail;
        MaskingService bll = new MaskingService();

        public FrmAdmin(Form loginForm)
        {
            frmLogin = loginForm;
            this.Text = "Quản lý Khách hàng (Quyền Admin) - Dữ liệu Gốc";
            this.Size = new Size(820, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();

            // 1. CỤM NHẬP LIỆU
            Label lblName = new Label() { Text = "Họ tên:", Location = new Point(20, 20), AutoSize = true };
            txtName = new TextBox() { Location = new Point(80, 17), Width = 150 };
            Label lblPhone = new Label() { Text = "Số ĐT:", Location = new Point(250, 20), AutoSize = true };
            txtPhone = new TextBox() { Location = new Point(310, 17), Width = 130 };
            Label lblEmail = new Label() { Text = "Email:", Location = new Point(460, 20), AutoSize = true };
            txtEmail = new TextBox() { Location = new Point(510, 17), Width = 180 };

            // 2. CỤM NÚT THAO TÁC (Đã thêm nút Xóa)
            Button btnAdd = new Button() { Text = "Thêm khách hàng", Location = new Point(20, 60), Width = 160, Height = 35 };
            Button btnLoad = new Button() { Text = "Tải Dữ Liệu", Location = new Point(190, 60), Width = 110, Height = 35 };
            Button btnDelete = new Button() { Text = "Xóa Khách Hàng", Location = new Point(310, 60), Width = 140, Height = 35 }; // NÚT XÓA NẰM ĐÂY

            Button btnLogout = new Button() { Text = "Đăng Xuất", Location = new Point(700, 15), Width = 90, Height = 35 };
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // 3. CỤM CẤU HÌNH & XUẤT FILE
            Label lblConfig = new Label() { Text = "Cấu hình che giấu:", Location = new Point(20, 115), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            RadioButton radLight = new RadioButton() { Text = "Che nhẹ", Location = new Point(160, 110), Width = 90, Checked = (AppConfig.MaskLevel == 1) };
            RadioButton radHeavy = new RadioButton() { Text = "Che nặng", Location = new Point(260, 110), Width = 90, Checked = (AppConfig.MaskLevel == 2) };
            Button btnExport = new Button() { Text = "Xuất Trang Tính", Location = new Point(380, 107), Width = 140, Height = 35 };

            // 4. CỤM TÌM KIẾM 
            txtSearch = new TextBox() { Location = new Point(530, 112), Width = 160 };
            Button btnSearch = new Button() { Text = "Tìm Kiếm", Location = new Point(700, 107), Width = 90, Height = 35 };
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // 5. BẢNG DỮ LIỆU 
            dgv = new DataGridView()
            {
                Location = new Point(20, 160),
                Size = new Size(770, 380),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToResizeColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, // Bấm vào 1 ô là bôi đen cả hàng để dễ xóa
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // 6. SỰ KIỆN NÚT BẤM
            radLight.CheckedChanged += (s, e) => { if (radLight.Checked) AppConfig.MaskLevel = 1; };
            radHeavy.CheckedChanged += (s, e) => { if (radHeavy.Checked) AppConfig.MaskLevel = 2; };

            btnAdd.Click += (s, e) =>
            {
                try
                {
                    bll.AddNewUser(txtName.Text, txtPhone.Text, txtEmail.Text);
                    MessageBox.Show("Đã thêm thành công!");
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            };

            // LOGIC NÚT XÓA Ở ĐÂY
            btnDelete.Click += (s, e) =>
            {
                if (dgv.CurrentRow != null && dgv.CurrentRow.Index >= 0)
                {
                    int selectedId = Convert.ToInt32(dgv.CurrentRow.Cells["id"].Value);
                    string selectedName = dgv.CurrentRow.Cells["name"].Value.ToString();

                    // Cảnh báo trước khi chém
                    var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{selectedName}' không?",
                                                  "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (confirm == DialogResult.Yes)
                    {
                        try
                        {
                            bll.DeleteUser(selectedId);
                            MessageBox.Show("Đã xóa thành công!");
                            LoadData();
                        }
                        catch (Exception ex) { MessageBox.Show("Lỗi khi xóa: " + ex.Message); }
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một khách hàng trong bảng để xóa!", "Thông báo");
                }
            };

            btnLoad.Click += (s, e) => LoadData();
            btnSearch.Click += (s, e) => LoadData(txtSearch.Text);
            btnLogout.Click += (s, e) => { this.Hide(); frmLogin.Show(); };

            btnExport.Click += (s, e) =>
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Bảng tính CSV (*.csv)|*.csv", FileName = "BaoCao_Admin.csv" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            bll.ExportToFile((DataTable)dgv.DataSource, sfd.FileName);
                            MessageBox.Show("Đã xuất file thành công tại:\n" + sfd.FileName);
                        }
                        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                    }
                }
            };

            // NHỚ THÊM btnDelete VÀO DÒNG NÀY ĐỂ NÓ HIỆN LÊN MÀN HÌNH
            this.Controls.AddRange(new Control[] { lblName, txtName, lblPhone, txtPhone, lblEmail, txtEmail, btnAdd, btnLoad, btnDelete, btnLogout, lblConfig, radLight, radHeavy, btnExport, txtSearch, btnSearch, dgv });

            // 7. SỰ KIỆN LOAD 
            this.Load += (s, e) =>
            {
                UIHelper.ApplyModernStyle(this);
                UIHelper.SetPlaceholder(txtName, "Ví dụ: Nguyễn Văn A");
                UIHelper.SetPlaceholder(txtPhone, "09xxxx...");
                UIHelper.SetPlaceholder(txtEmail, "abc@gmail.com");
                UIHelper.SetPlaceholder(txtSearch, "Nhập họ tên cần tìm...");
            };
        }

        private void LoadData(string keyword = "")
        {
            dgv.DataSource = bll.GetAdminData(keyword);
        }
    }
}