using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TblLib.Models;
using TblLib.Util;

namespace TblLib
{
    /// <summary>
    /// tbl文件翻译。
    /// </summary>
    public class TblTransform
    {
        public long offset = 1;

        /// <summary>
        /// 文件的二进制头部
        /// </summary>
        private List<byte> headBytes;

        private byte[] keysBytes;
        private byte[] valuesBytes;

        /// <summary>
        /// 翻译后的文件二进制
        /// </summary>
        public List<byte> FileBytes;

        private TblBinaryReader reader;

        public EventHandler TransformEventHandler;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public TblTransform(string filePath)
        {
            if (filePath == null)
            {
                throw new Exception("file path is null");
            }

            this.reader = new TblBinaryReader(File.Open(filePath, FileMode.Open));
            FileBytes = new List<byte>();
            headBytes = new List<byte>();
            GetStartPosition();
            offset = this.reader.BaseStream.Position;
        }

        /// <summary>
        /// 翻译。
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public byte[] Transform(TblModel[] items)
        {
            if (!items.Any())
                return null;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var key = ParserKey();

                FileBytes.AddRange(keysBytes);

                TransformEventHandler?.Invoke(1, null);

                var value = ParserValue();

                if (value.Length <= 0)
                {
                    GetNextPosition(FileBytes);
                    continue;
                }

                var info = items.FirstOrDefault(x => x.Key == key && x.Transform.Length > 0);

                if (info == null)
                {
                    //如果当前的value没有汉化，这使用原来的
                    FileBytes.AddRange(valuesBytes);
                    GetNextPosition(FileBytes);
                    continue;
                }

                if (info.Value != value)
                {
                    //如果字库中的韩文不等于解析出来的话，还是使用解析的
                    FileBytes.AddRange(valuesBytes);
                    GetNextPosition(FileBytes);
                    continue;
                }

                if (info.Transform.Length > 0)
                {
                    var tranBytes = Encoding.UTF8.GetBytes(info.Transform);

                    if (tranBytes.Length > valuesBytes.Length)
                    {
                        //翻译后的字节数大于韩文字节数，跳过
                        FileBytes.AddRange(valuesBytes);
                        GetNextPosition(FileBytes);
                        continue;
                    }

                    if (tranBytes.Length < valuesBytes.Length)
                    {
                        var tranList = tranBytes.ToList();

                        while (tranList.Count < valuesBytes.Length)
                        {
                            tranList.Add(0);
                        }

                        tranBytes = tranList.ToArray();
                    }
                    FileBytes.AddRange(tranBytes);
                }
                else
                {
                    FileBytes.AddRange(valuesBytes);
                }

                GetNextPosition(FileBytes);

            }

            reader.Close();

            headBytes.AddRange(FileBytes);

            return headBytes.ToArray();
        }

        private string ParserKey()
        {
            var key = new StringBuilder();
            var keyBytes = new List<byte>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var bytes = reader.ReadByte();

                if (bytes >= 32 && bytes <= 127)
                {
                    keyBytes.Add(bytes);
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
                    //GetNextPosition(keyBytes);
                    break;
                }
                else
                {
                    keyBytes.Add(bytes);
                    //读取到key的最后一个标识0，所以得退出去读取value了
                    offset = reader.BaseStream.Position;
                    break;
                }
            }

            this.keysBytes = keyBytes.ToArray();
            return key.ToString();
        }

        private string ParserValue()
        {
            var value = new StringBuilder();
            var valueBytes = new List<byte>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var bytes = reader.ReadByte();

                if (bytes > 127 && bytes < 254 && bytes != 194)
                {
                    reader.BaseStream.Position = offset;
                    //读取3个字节后转为文字
                    var bys = reader.ReadBytes(3);
                    valueBytes.AddRange(bys);
                    var text = reader.ParseByte(bys);
                    value.Append(text);
                    offset = reader.BaseStream.Position;
                }
                else if (bytes <= 1)
                {
                    //读取到 key了，所以游标减1
                    reader.BaseStream.Position = offset;
                    //移动游标到下一个读取点
                    //GetNextPosition(valueBytes);
                    break;
                }
                else if (bytes == 194)
                {
                    reader.BaseStream.Position = offset;
                    //读取3个字节后转为文字
                    var bys = reader.ReadBytes(2);
                    valueBytes.AddRange(bys);
                    var text = reader.ParseByte(bys);
                    value.Append(text);
                    offset = reader.BaseStream.Position;
                }
                else
                {
                    //文字中也包含有其他字母符号需要转换。
                    var text = reader.ParseByte(bytes);
                    value.Append(text);
                    valueBytes.Add(bytes);
                    offset = reader.BaseStream.Position;
                }
            }

            this.valuesBytes = valueBytes.ToArray();
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

                headBytes.Add(bytes);

                index++;
            }
        }

        private void GetNextPosition(List<byte> byteList)
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

                byteList.Add(bytes);

                index++;
            }
        }
    }
}
