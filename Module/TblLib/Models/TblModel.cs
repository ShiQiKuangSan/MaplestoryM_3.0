using System;

namespace TblLib.Models
{
    [Serializable]
    public class TblModel
    {
        /// <summary>
        /// key
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 原文
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 译文
        /// </summary>
        public string Transform { get; set; } = string.Empty;
    }
}
