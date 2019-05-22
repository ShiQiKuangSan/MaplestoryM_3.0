using System.Text;
using CCWin.SkinControl;

namespace MaplestoryM_3.FileHelper
{
    public class EncodingHelp
    {
        /// <summary>
        /// 获得文本真实长度。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetStringLength(string str)
        {
            return str.IsNullOrEmpty() ? 0 : Encoding.UTF8.GetBytes(str).Length;
        }
    }
}
