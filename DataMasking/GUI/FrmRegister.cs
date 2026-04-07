using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmRegister : Form
    {
        Form frmLogin;
        MaskingService bll = new MaskingService();
        TextBox txtUser, txtPass, txtName, txtPhone, txtEmail, txtCCCD, txtCustomKey;
        DateTimePicker dtpDob; // Biến chọn ngày sinh
        ComboBox cboAesType;

        public FrmRegister(Form loginForm)
        {
            frmLogin = loginForm;
            this.Text = "Đăng Ký - Tùy Chọn Cấp Độ AES";
            this.Size = new Size(400, 560); // Nới form dài ra chút
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

            // THÊM Ô CHỌN NGÀY SINH VÀO ĐÂY
            this.Controls.Add(new Label() { Text = "Ngày sinh:", Location = new Point(20, (y += gap) + 3), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            dtpDob = new DateTimePicker() { Location = new Point(140, y), Width = 220, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
            this.Controls.Add(dtpDob);

            txtPhone = AddField("Số điện thoại:", 20, y += gap);
            txtEmail = AddField("Email:", 20, y += gap);
            txtCCCD = AddField("Số CCCD:", 20, y += gap);

            // BỘ CHỌN CẤP ĐỘ AES
            this.Controls.Add(new Label() { Text = "Loại mã hóa:", Location = new Point(20, y += gap + 3), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            cboAesType = new ComboBox() { Location = new Point(140, y), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cboAesType.Items.AddRange(new string[] { "AES-128 (Đòi hỏi 16 ký tự)", "AES-192 (Đòi hỏi 24 ký tự)", "AES-256 (Đòi hỏi 32 ký tự)" });
            cboAesType.SelectedIndex = 2; // Mặc định AES-256
            this.Controls.Add(cboAesType);

            // Ô NHẬP KHÓA & NÚT SINH NGẪU NHIÊN
            txtCustomKey = AddField("Nhập Khóa:", 20, y += gap);
            Button btnRandomKey = new Button() { Text = "Sinh Ngẫu Nhiên Nhanh", Location = new Point(140, y += gap), Width = 220, Height = 30, BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRandomKey.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnRandomKey);

            btnRandomKey.Click += (s, e) => {
                int length = cboAesType.SelectedIndex == 0 ? 16 : (cboAesType.SelectedIndex == 1 ? 24 : 32);
                txtCustomKey.Text = Guid.NewGuid().ToString("N").Substring(0, length);
            };

            Button btnRegister = new Button() { Text = "Đăng Ký & Mã Hóa", Location = new Point(140, y += gap + 15), Width = 150, Height = 40 };

            btnRegister.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPass.Text))
                {
                    MessageBox.Show("Tài khoản và mật khẩu không được để trống!"); return;
                }

                string customKey = txtCustomKey.Text;
                int requiredLength = cboAesType.SelectedIndex == 0 ? 16 : (cboAesType.SelectedIndex == 1 ? 24 : 32);

                if (customKey.Length != requiredLength)
                {
                    MessageBox.Show($"CẢNH BÁO: Thuật toán {cboAesType.Text.Split(' ')[0]} bắt buộc khóa dài {requiredLength} ký tự!", "Vi phạm độ dài", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // GỌI HÀM VÀ TRUYỀN THÊM NGÀY SINH (Ép về chuỗi dd/MM/yyyy)
                string delegationKey = bll.RegisterCustomer(txtUser.Text, txtPass.Text, txtName.Text, dtpDob.Value.ToString("dd/MM/yyyy"), txtPhone.Text, txtEmail.Text, txtCCCD.Text, customKey);

                if (delegationKey != null)
                {
                    Clipboard.SetText(delegationKey);
                    MessageBox.Show("Đăng ký thành công!\n\nLƯU Ý QUAN TRỌNG:\nĐây là Khóa Cấp Quyền của bạn:\n" + delegationKey + "\n\n👉 HỆ THỐNG ĐÃ TỰ ĐỘNG COPY KHÓA NÀY VÀO BỘ NHỚ TẠM!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
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
            TextBox t = new TextBox() { Location = new Point(x + 140, y), Width = 220, UseSystemPasswordChar = isPassword };
            this.Controls.Add(t);
            return t;
        }
    }
}