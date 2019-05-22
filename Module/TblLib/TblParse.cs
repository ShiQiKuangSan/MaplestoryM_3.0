using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TblLib.Models;
using TblLib.Util;

namespace TblLib
{
    /// <summary>
    /// tbl文件转换类
    /// </summary>
    public class TblParse
    {
        public long offset = 1;

        private TblBinaryReader reader;

        public TblParse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("file path is null");
            }

            this.reader = new TblBinaryReader(File.Open(filePath, FileMode.Open));
            GetStartPosition();
            offset = this.reader.BaseStream.Position;
        }

        /// <summary>
        /// 解析tbl文件。
        /// </summary>
        /// <returns></returns>
        public List<TblModel> Parser()
        {
            var dic = new List<TblModel>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var key = ParserKey();
                var value = ParserValue();

                if (value.Length <= 0)
                {
                    continue;
                }

                dic.Add(new TblModel { Key = key, Value = value });
            }

            reader.Close();

            return dic;
        }

        private string ParserKey()
        {
            var key = new StringBuilder();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var bytes = reader.ReadByte();

                if (bytes >= 32 && bytes <= 127)
                {
                    //get key
                    var text = reader.ParseByte(bytes);
                    key.Append(text);
                    offset = reader.BaseStream.Position;
                }
                else if (bytes > 127 && bytes < 254)
                {
                    //读取到文本了
                    //读取到 value了，所以游标减1
                    reader.BaseStream.Position = offset;
                    GetNextPosition();
                    break;
                }
                else
                {
                    //读取到key的最后一个标识0，所以得退出去读取value了
                    offset = reader.BaseStream.Position;
                    break;
                }
            }

            return key.ToString();
        }

        private string ParserValue()
        {
            var value = new StringBuilder();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var bytes = reader.ReadByte();

                if (bytes > 127 && bytes < 254 && bytes != 194)
                {
                    reader.BaseStream.Position = offset;
                    //读取3个字节后转为文字
                    var bys = reader.ReadBytes(3);
                    var text = reader.ParseByte(bys);
                    value.Append(text);
                    offset = reader.BaseStream.Position;
                }
                else if (bytes <= 1)
                {
                    //读取到 key了，所以游标减1
                    reader.BaseStream.Position = offset;
                    //移动游标到下一个读取点
                    GetNextPosition();
                    break;
                }
                else if (bytes == 194)
                {
                    reader.BaseStream.Position = offset;
                    //读取3个字节后转为文字
                    var bys = reader.ReadBytes(2);
                    var text = reader.ParseByte(bys);
                    value.Append(text);
                    offset = reader.BaseStream.Position;
                }
                else
                {
                    //文字中也包含有其他字母符号需要转换。
                    var text = reader.ParseByte(bytes);
                    value.Append(text);
                    offset = reader.BaseStream.Position;
                }
            }

            return value.ToString();
        }

        /// <summary>
        /// 获取tbl开始解析的位置
        /// </summary>
        /// <returns></returns>
        private void GetStartPosition()
        {
            var index = 0;
            while (index < reader.BaseStream.Length)
            {
                var bytes = reader.ReadByte();

                if (bytes >= 32 && bytes < 254)
                {
                    reader.BaseStream.Position = index;
                    break;
                }

                index++;
            }
        }

        private void GetNextPosition()
        {
            var index = offset;
            while (index < reader.BaseStream.Length)
            {
                var bytes = reader.ReadByte();

                if (bytes >= 32 && bytes < 254)
                {
                    reader.BaseStream.Position = index;
                    offset = reader.BaseStream.Position;
                    break;
                }

                index++;
            }
        }
    }
}
