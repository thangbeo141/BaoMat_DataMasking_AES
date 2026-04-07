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
            this.Text = "Hệ Thống Bảo Mật Zero-Knowledge";
            this.Size = new Size(380, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Label lblUser = new Label() { Text = "Tài khoản:", Location = new Point(30, 40), AutoSize = true };
            txtUser = new TextBox() { Location = new Point(120, 37), Width = 200 };

            Label lblPass = new Label() { Text = "Mật khẩu:", Location = new Point(30, 85), AutoSize = true };
            txtPass = new TextBox() { Location = new Point(120, 82), Width = 200, UseSystemPasswordChar = true };

            Button btnLogin = new Button() { Text = "Đăng Nhập", Location = new Point(120, 130), Width = 100, Height = 35 };
            Button btnRegister = new Button() { Text = "Đăng Ký", Location = new Point(230, 130), Width = 90, Height = 35, BackColor = Color.MediumPurple, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRegister.FlatAppearance.BorderSize = 0;

            // XỬ LÝ ĐĂNG NHẬP
            btnLogin.Click += (s, e) =>
            {
                string username = txtUser.Text.Trim();
                string password = txtPass.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string role = DatabaseHelper.CheckLoginAndGetRole(username, password);

                if (role == "Admin")
                {
                    this.Hide();
                    // TRUYỀN THÊM: Username và Password vào Form Admin
                    new FrmAdmin(this, username, password).Show();
                    txtPass.Clear();
                }
                else if (role == "Customer") // Chuyển luồng Tech thành Customer
                {
                    txtPass.Clear();
                    this.Hide();
                    // Phải truyền Username và Password vào để Customer còn có chìa khóa mở data
                    new FrmCustomer(this, username, password).Show();
                }
                else
                {
                    MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // XỬ LÝ ĐĂNG KÝ
            btnRegister.Click += (s, e) =>
            {
                this.Hide();
                new FrmRegister(this).Show();
            };

            this.Controls.AddRange(new Control[] { lblUser, txtUser, lblPass, txtPass, btnLogin, btnRegister });
            this.Load += (s, e) => UIHelper.ApplyModernStyle(this);
        }
    }
}