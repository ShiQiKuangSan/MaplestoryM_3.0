using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TblLib.Models;

namespace MaplestoryM_3.Models
{
    public class ModelManager
    {
        #region private

        private static List<TblModel> _models = new List<TblModel>();

        private static string _path = $@"{Environment.CurrentDirectory}\字库\";

        private static string _FanYi = $@"{Environment.CurrentDirectory}\汉化\";

        /// <summary>
        /// 文件后缀
        /// </summary>
        private const string fileExtension = ".mapM_3";

        private static ModelManager _instance = null;

        #endregion private


        /// <summary>
        /// 字库集合。
        /// </summary>
        public List<TblModel> Items
        {
            get => _models;
            set => _models = value;
        }

        /// <summary>
        /// 当前实例
        /// </summary>
        public static ModelManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ModelManager();

                return _instance;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }

            var dir = new DirectoryInfo(_path);

            var file = dir.GetFileSystemInfos().FirstOrDefault(x => x.Extension == fileExtension);

            if (file == null)
                return;

            //开始反序列化
            var path = file.FullName;

            var t = new Thread(DeSerializable) { IsBackground = true };

            t.Start(path);
            t.Join();
        }

        /// <summary>
        /// 保存字库
        /// </summary>
        public void Save()
        {
            var patch = $"{_path}zh_Ch{fileExtension}";

            using (var fStream = new FileStream(patch, FileMode.Create, FileAccess.ReadWrite))
            {
                var binFormat = new BinaryFormatter();//创建二进制序列化器

                binFormat.Serialize(fStream, _models);
            }
        }


        #region private

        /// <summary>
        /// 反序列化对象。
        /// </summary>
        private void DeSerializable(object patch)
        {
            var p = patch as string;

            using (var fStream = new FileStream(p ?? throw new InvalidOperationException(), FileMode.Open, FileAccess.Read))
            {
                var binFormat = new BinaryFormatter();//创建二进制序列化器

                //反序列化对象
                _models = (List<TblModel>)binFormat.Deserialize(fStream);
            }
        }

        #endregion private
    }
}
