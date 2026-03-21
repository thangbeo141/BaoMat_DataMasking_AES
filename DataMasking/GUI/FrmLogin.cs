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
           

            btnLogin.Click += (s, e) =>
            {
                if (txtUser.Text == "admin" && txtPass.Text == "123")
                {
                    txtPass.Clear(); this.Hide(); new FrmAdmin(this).Show();
                }
                else if (txtUser.Text == "nhanvien" && txtPass.Text == "123")
                {
                    txtPass.Clear(); this.Hide(); new FrmTech(this).Show();
                }
                else MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
            };

        

            this.Controls.AddRange(new Control[] { lblUser, txtUser, lblPass, txtPass, btnLogin });
            this.Load += (s, e) => UIHelper.ApplyModernStyle(this);
        }
    }
}