using System;
using System.Windows.Forms;

namespace DataMasking
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmLogin()); // Khởi động Form Đăng nhập đầu tiên
        }
    }
}