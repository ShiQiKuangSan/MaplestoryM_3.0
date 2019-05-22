using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CCWin;
using CCWin.SkinControl;
using MaplestoryM_3.FileHelper;
using MaplestoryM_3.Models;
using MaplestoryM_3.Translate;
using TblLib.Models;

namespace MaplestoryM_3
{
    public partial class FormSearch : Skin_Color
    {
        private List<TblModel> _items = new List<TblModel>();
        /// <summary>
        /// 当前页码
        /// </summary>
        private int pageIndex = 1;
        private int maxPageSize = 1;

        private UiHelper uiHelper;
        public FormSearch(List<TblModel> items)
        {
            InitializeComponent();
            _items = items;
            this.sdgv_items.AllowUserToAddRows = false;
            sdgv_items.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            uiHelper = new UiHelper();
            uiHelper.init(this,_items);
            GetPage();
        }

        private void FormSearch_Load(object sender, System.EventArgs e)
        {
            uiHelper.InitData(sdgv_items, null, pageIndex);
        }

        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbtn_page_up_Click(object sender, EventArgs e)
        {
            if (pageIndex == 1)
                return;

            pageIndex--;

            uiHelper.InitData(sdgv_items, null, pageIndex);
            this.slbl_pageIndex.Text = pageIndex.ToString();
        }

        /// <summary>
        /// 下一页。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbtn_page_down_Click(object sender, EventArgs e)
        {
            if (pageIndex >= maxPageSize)
                return;

            pageIndex++;

            uiHelper.InitData(sdgv_items, null, pageIndex);
            this.slbl_pageIndex.Text = pageIndex.ToString();
        }

        /// <summary>
        /// 跳转页到
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbtn_jump_Click(object sender, EventArgs e)
        {
            var page = txt_jump.Text;
            var status = int.TryParse(page, out var index);
            if (!status)
            {
                MessageBoxEx.Show("请输入一个正确的数字", " 提示");
                txt_jump.Focus();
                return;
            }

            if (index > maxPageSize)
            {
                MessageBoxEx.Show("跳转的页数不能大于最大页数", " 提示");
                txt_jump.Focus();
                return;
            }

            pageIndex = index;
            uiHelper.InitData(sdgv_items, null, index);
            this.slbl_pageIndex.Text = pageIndex.ToString();
        }

        /// <summary>
        /// 翻译选中行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbtn_fanyi_Click(object sender, EventArgs e)
        {
            if (sdgv_items.SelectedRows.Count <= 0)
            {
                return;
            }

            if (sdgv_items.SelectedRows.Count > 1)
            {
                MessageBoxEx.Show("只能选择一行进行翻译,你现在选择了多行", "提示");
                return;
            }

            var i = sdgv_items.SelectedRows.Count - 1;

            var han = this.sdgv_items.SelectedRows[i].Cells[2].Value.ToString();

            void SetFanYi(string value)
            {
                if (value.IsNullOrEmpty())
                    value = string.Empty;
                value = value.Replace("\\n", "\r\n");
                stxt_Transform.Text = value;
            }

            var task = new Task(() =>
            {
                var value = GooleFanYi.GoogleTranslate(han);

                this.Invoke((Action<string>)SetFanYi, value);
            });

            task.Start();
        }

        /// <summary>
        /// 获得页码信息
        /// </summary>
        void GetPage()
        {
            var count = _items.Count;

            if (count > uiHelper.pageSize)
            {
                maxPageSize = (count / uiHelper.pageSize) + 1;
            }

            this.slbl_pageIndex.Text = pageIndex.ToString();
            this.slbl_maxPage.Text = maxPageSize.ToString();
        }

        /// <summary>
        /// 行被选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sdgv_items_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                var value = this.sdgv_items.Rows[e.RowIndex].Cells[2].Value.ToString();
                var trans = this.sdgv_items.Rows[e.RowIndex].Cells[3].Value.ToString();

                stxt_han.Text = value;
                stxt_trans.Text = trans;

                lbl_otext.Text = EncodingHelp.GetStringLength(value).ToString();
                label2.Text = EncodingHelp.GetStringLength(trans).ToString();
            }
        }

        /// <summary>
        /// 按键盘的上下键给列表置入焦点时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sdgv_items_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (sdgv_items.SelectedRows.Count <= 0)
            {
                return;
            }

            if (sdgv_items.SelectedRows.Count > 1)
            {
                MessageBoxEx.Show("只能选择一行进行翻译,你现在选择了多行", "提示");
                return;
            }

            var i = sdgv_items.SelectedRows.Count - 1;

            var han = this.sdgv_items.SelectedRows[i].Cells[2].Value.ToString();
            var tran = this.sdgv_items.SelectedRows[i].Cells[3].Value.ToString();

            var hanLen = EncodingHelp.GetStringLength(han);
            var tranLen = EncodingHelp.GetStringLength(tran);

            lbl_otext.Text = hanLen.ToString();
            label2.Text = tranLen.ToString();

            stxt_han.Text = han;
            stxt_trans.Text = tran;
        }

        /// <summary>
        /// 翻译窗口失去焦点的时候。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Stxt_trans_Leave(object sender, EventArgs e)
        {
            if (sdgv_items.SelectedRows.Count <= 0)
            {
                return;
            }

            if (sdgv_items.SelectedRows.Count > 1)
            {
                MessageBoxEx.Show("只能选择一行进行翻译,你现在选择了多行", "提示");
                return;
            }

            var i = sdgv_items.SelectedRows.Count - 1;

            var key = this.sdgv_items.SelectedRows[i].Cells[1].Value.ToString();
            var han = stxt_han.Text;
            var tran = stxt_trans.Text;

            if (key.IsNullOrEmpty())
            {
                MessageBoxEx.Show("获取key失败", "提示");
                return;
            }

            var model = ModelManager.Instance.Items.FirstOrDefault(x => x.Key == key);

            if (model == null)
            {
                MessageBoxEx.Show("根据key没有找到数据", "提示");
                return;
            }

            if (model.Value != han)
            {
                MessageBoxEx.Show("搜索到的信息与选中的韩文不匹配", "提示");
                return;
            }

            var hanLen = EncodingHelp.GetStringLength(han);
            var tranLen = EncodingHelp.GetStringLength(tran);

            if (tranLen > hanLen)
            {
                //这个汉化不能转换，提示用户
                this.sdgv_items.SelectedRows[i].Cells[0].Value = Resource.PROCESS_WRONG;
            }
            else
            {
                this.sdgv_items.SelectedRows[i].Cells[0].Value = Resource.PROCESS_RIGHT;
            }

            this.sdgv_items.SelectedRows[i].Cells[3].Value = tran;

            lbl_otext.Text = hanLen.ToString();
            label2.Text = tranLen.ToString();
            model.Transform = tran;

            ModelManager.Instance.Save();
        }
    }
}
