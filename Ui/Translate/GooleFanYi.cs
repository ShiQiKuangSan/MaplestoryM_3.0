using System;
using System.IO;
using System.Net;
using System.Text;
using MSScriptControl;

namespace MaplestoryM_3.Translate
{
    public class GooleFanYi
    {
        /// <summary>
        /// 谷歌翻译
        /// </summary>
        /// <param name="text">待翻译文本</param>
        /// <param name="fromLanguage">自动检测：auto</param>
        /// <param name="toLanguage">中文：zh-CN，英文：en</param>
        /// <returns>翻译后文本</returns>
        public static string GoogleTranslate(string text, string fromLanguage = "auto", string toLanguage = "zh-CN")
        {
            text = text.Replace("[", "(").Replace("]", ")").Replace(":", ":>").Replace("：", ":>");
            CookieContainer cc = new CookieContainer();

            string GoogleTransBaseUrl = "https://translate.google.cn/";

            var BaseResultHtml = GetResultHtml(GoogleTransBaseUrl, cc, "");

            if (string.IsNullOrWhiteSpace(BaseResultHtml))
            {
                return "网络连接不上";
            }

            var startIndex = BaseResultHtml.IndexOf("tkk:") + 5;

            var endIndex = BaseResultHtml.IndexOf("',experiment_ids");

            var TKK = BaseResultHtml.Substring(startIndex, endIndex - startIndex);

            var GetTkkJS = Resource.gettk;

            var tk = ExecuteScript("tk(\"" + String2Unicode(text) + "\",\"" + TKK + "\")", GetTkkJS);

            if (tk == null)
                throw new Exception("tk值不能为空");

            var googleTransUrl = "https://translate.google.cn/translate_a/t?client=t&text=" + text + "&hl=zh-CN&sl=" + fromLanguage + "&tl=" + toLanguage + "&ie=UTF-8&oe=UTF-8&tk=" + tk;

            var ResultHtml = GetResultHtml(googleTransUrl, cc, "https://translate.google.cn/");
            var value = ResultHtml.Replace("\"", "").Replace(" ", "").Replace(@"\u003e", "");

            value = value.Replace("(", "[").Replace(")", "]").Replace("（", "[").Replace("）", "]").Replace("[ - ]", "[-]");

            //[每日任务,ko]
            value = value.Replace("[", "").Replace("]", "").Split(',')[0];

            return value;
        }

        private static string GetResultHtml(string url, CookieContainer cc, string refer)
        {
            var html = "";

            var webRequest = WebRequest.Create(url) as HttpWebRequest;

            if (webRequest != null)
            {
                webRequest.Method = "GET";

                webRequest.CookieContainer = cc;

                webRequest.Referer = refer;

                webRequest.Timeout = 20000;

                webRequest.Headers.Add("X-Requested-With:XMLHttpRequest");

                webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

                //webRequest.UserAgent = useragent;

                try
                {
                    using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                        {
                            html = reader.ReadToEnd();
                            reader.Close();
                            webResponse.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    html = string.Empty;
                }
            }
            return html;
        }

        /// <summary>
        /// 执行JS
        /// </summary>
        /// <param name="sExpression">参数体</param>
        /// <param name="sCode">JavaScript代码的字符串</param>
        /// <returns></returns>
        private static string ExecuteScript(string sExpression, string sCode)
        {
            var scriptControl = new ScriptControl
            {
                UseSafeSubset = true,
                Language = "JScript"
            };
            scriptControl.AddCode(sCode);
            try
            {
                string str = scriptControl.Eval(sExpression).ToString();
                return str;
            }
            catch (Exception ex)
            {
                var str = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        internal static string String2Unicode(string source)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }
    }
}
