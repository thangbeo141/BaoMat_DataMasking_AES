using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmLogin : Form
    {
        TextBox txtUser, txtPass;

        public FrmLogin()
        {
            this.Text = "Hệ Thống Bảo Mật - Đăng Nhập";
            this.Size = new Size(380, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Label lblUser = new Label() { Text = "Tài khoản:", Location = new Point(30, 40), AutoSize = true };
            txtUser = new TextBox() { Location = new Point(120, 37), Width = 200 };

            Label lblPass = new Label() { Text = "Mật khẩu:", Location = new Point(30, 85), AutoSize = true };
            txtPass = new TextBox() { Location = new Point(120, 82), Width = 200, UseSystemPasswordChar = true };

            Button btnLogin = new Button() { Text = "Đăng Nhập", Location = new Point(120, 130), Width = 110, Height = 35 };

            // LOGIC ĐĂNG NHẬP MỚI (KIỂM TRA HASH TỪ DATABASE)
            btnLogin.Click += (s, e) =>
            {
                string username = txtUser.Text.Trim();
                string password = txtPass.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Chọc xuống DB để băm mật khẩu và lấy Role (Quyền) về
                string role = DatabaseHelper.CheckLoginAndGetRole(username, password);

                if (role == "Admin")
                {
                    txtPass.Clear();
                    this.Hide();
                    new FrmAdmin(this).Show(); // Truyền 'this' vào nếu form Admin của bạn có tham số
                }
                else if (role == "Tech")
                {
                    txtPass.Clear();
                    this.Hide();
                    new FrmTech(this).Show();  // Truyền 'this' vào nếu form Tech của bạn có tham số
                }
                else
                {
                    // Lỗi ngầu chuẩn An toàn thông tin
                    MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!", "Cảnh báo Bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            this.Controls.AddRange(new Control[] { lblUser, txtUser, lblPass, txtPass, btnLogin });
            this.Load += (s, e) => UIHelper.ApplyModernStyle(this);
        }
    }
}