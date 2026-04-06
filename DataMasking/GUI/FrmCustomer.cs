using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmCustomer : Form
    {
        DataGridView dgv;
        Form frmLogin;
        MaskingService bll = new MaskingService();

        string currentUser;
        string currentPass;

        // Bắt buộc phải nhận User và Pass từ FrmLogin truyền sang để mở bao thư
        public FrmCustomer(Form loginForm, string username, string password)
        {
            frmLogin = loginForm;
            currentUser = username;
            currentPass = password;

            this.Text = $"Khu vực cá nhân - Xin chào: {username}";
            this.Size = new Size(800, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosed += (s, e) => Application.Exit();

            Button btnLoad = new Button() { Text = "Lấy Dữ Liệu Giải Mã", Location = new Point(20, 20), Width = 180, Height = 35 };
            Button btnLogout = new Button() { Text = "Đăng Xuất", Location = new Point(660, 20), Width = 100, Height = 35, Anchor = AnchorStyles.Top | AnchorStyles.Right };

            dgv = new DataGridView()
            {
                Location = new Point(20, 80),
                Size = new Size(740, 100),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnLoad.Click += (s, e) => LoadMyData();
            btnLogout.Click += (s, e) => { this.Hide(); frmLogin.Show(); };

            this.Controls.AddRange(new Control[] { btnLoad, btnLogout, dgv });
            this.Load += (s, e) => { UIHelper.ApplyModernStyle(this); LoadMyData(); };
        }

        private void LoadMyData()
        {
            // Truyền User và Pass xuống Backend để giải mã
            dgv.DataSource = bll.GetMyData(currentUser, currentPass);
        }
    }
}