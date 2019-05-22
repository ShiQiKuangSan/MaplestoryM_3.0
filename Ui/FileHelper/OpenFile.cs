using System.Windows.Forms;

namespace MaplestoryM_3.FileHelper
{
    public class OpenFile
    {
        /**
         * 打开文件
         */
        public static string OpenDialog(string title, string filter, string message)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = title,
                Filter = filter
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var result = MessageBox.Show(message, @"提示", MessageBoxButtons.YesNo,
                    MessageBoxIcon.None);

                if (result == DialogResult.Yes)
                {
                    return openFileDialog.FileName;
                }
            }

            return string.Empty;
        }
    }
}
