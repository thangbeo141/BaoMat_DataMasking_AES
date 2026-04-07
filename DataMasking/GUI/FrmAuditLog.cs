using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataMasking
{
    public class FrmAuditLog : Form
    {
        public FrmAuditLog()
        {
            this.Text = "Nhật Ký Hệ Thống (Audit Logs) - Tính Minh Bạch";
            this.Size = new Size(650, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            DataGridView dgv = new DataGridView() { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgv.DataSource = DatabaseHelper.GetAuditLogs();
            this.Controls.Add(dgv);

            this.Load += (s, e) => UIHelper.ApplyModernStyle(this);
        }
    }
}