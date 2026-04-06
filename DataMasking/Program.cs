using System;
using System.Configuration; // Bắt buộc phải có dòng này
using System.Windows.Forms;

namespace DataMasking
{
    static class Program
    {
        // Viết bùa chú mã hóa bằng DPAPI của Windows
        private static void ProtectConfigFile()
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConfigurationSection section = config.GetSection("connectionStrings");

                // Nếu chưa bị mã hóa thì khóa lại ngay lập tức
                if (section != null && !section.SectionInformation.IsProtected)
                {
                    // Gọi hàm mã hóa cấp thấp của hệ điều hành Windows
                    section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    section.SectionInformation.ForceSave = true;
                    config.Save(ConfigurationSaveMode.Modified);
                }
            }
            catch (Exception)
            {
                // Bỏ qua lỗi nếu hệ thống không cho phép ghi đè
            }
        }

        [STAThread]
        static void Main()
        {
            // GỌI HÀM MÃ HÓA NGAY KHI VỪA BẬT PHẦN MỀM LÊN
            ProtectConfigFile();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Khởi động vào màn hình Đăng nhập
            Application.Run(new FrmLogin());
        }
    }
}