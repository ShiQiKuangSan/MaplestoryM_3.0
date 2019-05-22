using System.IO;
using System.Text;

namespace TblLib.Util
{
    public class TblBinaryReader : BinaryReader
    {
        #region 属性

        public byte[] TblKey { get; set; }

        #endregion

        public TblBinaryReader(Stream input) : base(input)
        {
        }

        public string ReadString(int length)
        {
            var bytes= ReadBytes(length);
            var t = Encoding.UTF8.GetString(bytes);
            return t;
        }

        public string ParseByte(params byte[] bytes)
        {
            var t = Encoding.UTF8.GetString(bytes);
            return t;
        }
    }
}
