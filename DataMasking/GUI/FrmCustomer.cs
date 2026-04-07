using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmCustomer : Form
    {
        Form frmLogin;
        MaskingService bll = new MaskingService();
        string currentUser, currentPass;

        TextBox txtName, txtPhone, txtEmail, txtCccd;
        DateTimePicker dtpDob;

        // Thêm 2 ô nhập mật khẩu
        TextBox txtOldPass, txtNewPass;

        public FrmCustomer(Form loginForm, string username, string password)
        {
            frmLogin = loginForm;
            currentUser = username;
            currentPass = password;

            this.Text = $"Khu vực cá nhân - Xin chào: {username}";
            this.Size = new Size(450, 680); // Nới form dài ra thêm để chứa khu Đổi mật khẩu
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();

            InitUI();
            this.Load += (s, e) => { UIHelper.ApplyModernStyle(this); LoadMyData(); };
        }

        private void InitUI()
        {
            int y = 20, gap = 45;

            // --- KHU VỰC 1: THÔNG TIN CÁ NHÂN ---
            this.Controls.Add(new Label() { Text = "THÔNG TIN CÁ NHÂN", Location = new Point(130, y), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Teal, AutoSize = true });

            txtName = AddField("Họ Tên thật:", 20, y += gap);

            this.Controls.Add(new Label() { Text = "Ngày sinh:", Location = new Point(20, (y += gap) + 3), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            dtpDob = new DateTimePicker() { Location = new Point(140, y), Width = 250, Font = new Font("Segoe UI", 10), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
            this.Controls.Add(dtpDob);

            txtPhone = AddField("Số điện thoại:", 20, y += gap);
            txtEmail = AddField("Email:", 20, y += gap);
            txtCccd = AddField("Số CCCD:", 20, y += gap);

            Button btnUpdate = new Button() { Text = "Cập Nhật Thông Tin", Location = new Point(140, y += gap + 15), Width = 150, Height = 40, BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnUpdate);

            // --- KHU VỰC 2: ĐỔI MẬT KHẨU (ĐẢO KHÓA) ---
            y += 80; // Nhảy xuống một đoạn
            this.Controls.Add(new Label() { Text = "ĐỔI MẬT KHẨU (ĐẢO KHÓA)", Location = new Point(110, y), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Crimson, AutoSize = true });

            txtOldPass = AddField("Mật khẩu cũ:", 20, y += gap, true);
            txtNewPass = AddField("Mật khẩu mới:", 20, y += gap, true);

            Button btnChangePass = new Button() { Text = "Thực hiện Đảo Khóa", Location = new Point(140, y += gap + 15), Width = 150, Height = 40, BackColor = Color.Crimson, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnChangePass.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnChangePass);

            // Nút Đăng xuất đưa xuống góc
            Button btnLogout = new Button() { Text = "Đăng Xuất", Location = new Point(310, y + 15), Width = 100, Height = 40, BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnLogout);

            
            // SỰ KIỆN LƯU THÔNG TIN ĐÃ SỬA (CÓ XÁC NHẬN)
            btnUpdate.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtName.Text)) { MessageBox.Show("Tên không được để trống!"); return; }

                // HIỆN BẢNG HỎI XÁC NHẬN
                DialogResult dialogResult = MessageBox.Show("Bạn có chắc chắn muốn cập nhật và mã hóa lại toàn bộ thông tin cá nhân này không?", "Xác nhận cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    bool ok = bll.UpdateMyData(currentUser, currentPass, txtName.Text, dtpDob.Value.ToString("dd/MM/yyyy"), txtPhone.Text, txtEmail.Text, txtCccd.Text);

                    if (ok) MessageBox.Show("Cập nhật dữ liệu và Mã hóa an toàn thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else MessageBox.Show("Lỗi cập nhật!");
                }
            };

            // SỰ KIỆN ĐỔI MẬT KHẨU (CÓ XÁC NHẬN KÉP)
            btnChangePass.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtOldPass.Text) || string.IsNullOrEmpty(txtNewPass.Text))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu cũ và mới!"); return;
                }

                if (txtOldPass.Text != currentPass)
                {
                    MessageBox.Show("Mật khẩu cũ không chính xác!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
                }

                // HIỆN BẢNG HỎI XÁC NHẬN (Nhấn mạnh rủi ro)
                DialogResult dialogResult = MessageBox.Show("CẢNH BÁO: Quá trình này sẽ thực hiện Đảo khóa (Key Rotation) toàn bộ dữ liệu của bạn.\n\nHãy chắc chắn bạn đã nhớ kỹ mật khẩu mới. Bấm YES để tiếp tục!", "Xác nhận Đảo khóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.Yes)
                {
                    bool ok = bll.ChangePassword(currentUser, txtOldPass.Text, txtNewPass.Text);

                    if (ok)
                    {
                        MessageBox.Show("Đảo khóa (Key Rotation) thành công!\nVui lòng đăng nhập lại bằng mật khẩu mới.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();
                        frmLogin.Show(); // Bắt đăng nhập lại ngay lập tức
                    }
                    else
                    {
                        MessageBox.Show("Lỗi trong quá trình đảo khóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            btnLogout.Click += (s, e) => { this.Hide(); frmLogin.Show(); };
        }

        private TextBox AddField(string lblText, int x, int y, bool isPassword = false)
        {
            this.Controls.Add(new Label() { Text = lblText, Location = new Point(x, y + 3), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            TextBox t = new TextBox() { Location = new Point(140, y), Width = 250, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = isPassword };
            this.Controls.Add(t);
            return t;
        }

        private void LoadMyData()
        {
            DataTable dt = bll.GetMyData(currentUser, currentPass);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtName.Text = row["name"].ToString();
                try
                {
                    dtpDob.Value = DateTime.ParseExact(row["dob"].ToString(), "dd/MM/yyyy", null);
                }
                catch
                {
                    dtpDob.Value = DateTime.Now;
                }
                txtPhone.Text = row["phone"].ToString();
                txtEmail.Text = row["email"].ToString();
                txtCccd.Text = row["cccd"].ToString();
            }
        }
    }
}