using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CCWin.SkinControl;
using MaplestoryM_3.Models;
using TblLib.Models;

namespace MaplestoryM_3.FileHelper
{
    public class UiHelper
    {
        private int pageIndex = 1;
        public int pageSize = 13;

        private Form mainForm;

        private List<TblModel> items;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="formMain"></param>
        public void init(Form formMain, List<TblModel> itemsList)
        {
            mainForm = formMain;
            items = itemsList;
        }

        /// <summary>
        /// 加载数据到列表
        /// </summary>
        /// <param name="dataGridView"></param>
        public void InitData(SkinDataGridView dataGridView, Action getPage, int index = 1, int size = 13)
        {
            pageIndex = (index - 1) * size;
            var pageSeze = pageIndex + size;

            if (pageSeze > items.Count)
            {
                pageSeze = pageIndex + (items.Count - pageIndex);
            }

            dataGridView.Rows.Clear();

            //正常的文本
            void AddRIGHT(TblModel x)
            {
                dataGridView.Rows.Add(
                        Resource.PROCESS_RIGHT,
                        x.Key,
                        x.Value,
                        x.Transform
                    );
            }

            void AddWRONG(TblModel x)
            {
                dataGridView.Rows.Add(
                    Resource.PROCESS_WRONG,
                    x.Key,
                    x.Value,
                    x.Transform
                );
            }

            var task = new Task(() =>
            {
                for (var i = pageIndex; i < pageSeze; i++)
                {
                    var x = items[i];
                    if (x.Transform.Length <= 0)
                    {
                        mainForm.Invoke((Action<TblModel>)AddRIGHT, x);
                    }
                    else
                    {
                        //原文的长度
                        var valueLength = EncodingHelp.GetStringLength(x.Value);
                        //翻译的长度
                        var transLength = EncodingHelp.GetStringLength(x.Transform);

                        if (transLength > valueLength)
                        {
                            mainForm.Invoke((Action<TblModel>)AddWRONG, x);
                        }
                        else
                        {
                            mainForm.Invoke((Action<TblModel>)AddRIGHT, x);
                        }
                    }
                }

                if (getPage != null)
                {
                    mainForm.Invoke(getPage);
                }
            });

            task.Start();
        }
    }
}

