using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmRegister : Form
    {
        Form frmLogin;
        MaskingService bll = new MaskingService();
        TextBox txtUser, txtPass, txtName, txtPhone, txtEmail, txtCCCD;

        public FrmRegister(Form loginForm)
        {
            frmLogin = loginForm;
            this.Text = "Đăng Ký Khách Hàng Mới";
            this.Size = new Size(400, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => { frmLogin.Show(); };

            InitUI();
            this.Load += (s, e) => UIHelper.ApplyModernStyle(this);
        }

        private void InitUI()
        {
            int y = 20, gap = 40;
            txtUser = AddField("Tài khoản ĐN:", 20, y);
            txtPass = AddField("Mật khẩu:", 20, y += gap, true);
            txtName = AddField("Họ Tên thật:", 20, y += gap);
            txtPhone = AddField("Số điện thoại:", 20, y += gap);
            txtEmail = AddField("Email:", 20, y += gap);
            txtCCCD = AddField("Số CCCD:", 20, y += gap);

            Button btnRegister = new Button() { Text = "Đăng Ký & Mã Hóa", Location = new Point(140, y += gap + 10), Width = 150, Height = 40 };

            btnRegister.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPass.Text))
                {
                    MessageBox.Show("Tài khoản và mật khẩu không được để trống!"); return;
                }

                bool isSuccess = bll.RegisterCustomer(txtUser.Text, txtPass.Text, txtName.Text, txtPhone.Text, txtEmail.Text, txtCCCD.Text);

                if (isSuccess)
                {
                    MessageBox.Show("Đăng ký thành công! Dữ liệu của bạn đã được khóa bằng mật khẩu cá nhân.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Đóng form quay lại Login
                }
                else
                {
                    MessageBox.Show("Lỗi đăng ký! Có thể tài khoản đã tồn tại.", "Lỗi");
                }
            };

            this.Controls.Add(btnRegister);
        }

        private TextBox AddField(string lblText, int x, int y, bool isPassword = false)
        {
            this.Controls.Add(new Label() { Text = lblText, Location = new Point(x, y + 3), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            TextBox t = new TextBox() { Location = new Point(x + 120, y), Width = 220, UseSystemPasswordChar = isPassword };
            this.Controls.Add(t);
            return t;
        }
    }
}