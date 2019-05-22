using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CCWin;
using CCWin.SkinControl;
using MaplestoryM_3.FileHelper;
using MaplestoryM_3.Models;
using MaplestoryM_3.Translate;
using TblLib;

namespace MaplestoryM_3
{
    public partial class FormMain : Skin_Color
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        private int pageIndex = 1;
        private int maxPageSize = 1;

        private FormProgress progress;
        private UiHelper uiHelper;
        public FormMain()
        {
            InitializeComponent();

            this.sdgv_items.AllowUserToAddRows = false;
            sdgv_items.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ModelManager.Instance.Init();

            uiHelper = new UiHelper();
            uiHelper.init(this, ModelManager.Instance.Items);

            statusStrip1.Items[1].Text = @"0";

            GetPage();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            uiHelper.InitData(sdgv_items, null, pageIndex);
        }

        /// <summary>
        /// 导入韩服文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbtn_improt_han_Click(object sender, EventArgs e)
        {
            var patch = OpenFile.OpenDialog("请选择韩文语言包", "原始韩文包(*.tbl)|*.tbl", "是否将文件导入到字库中");

            if (string.IsNullOrWhiteSpace(patch))
            {
                return;
            }
            var form = this;

            progress = new FormProgress();
            progress.SetFormText("正在解析tbl文件");

            var task = new Task(() =>
            {
                var tblParse = new TblParse(patch);

                var models = tblParse.Parser();

                form.Invoke((Action<string, int>)SetFormInfo, "导入韩文进度", models.Count);

                var items = ModelManager.Instance.Items;

                //过滤字库
                models.ForEach(x =>
                {
                    if (!items.Exists(item => item.Key == x.Key))
                    {
                        items.Add(x);
                    }

                    form.Invoke((Action)SetFormProgress);
                });

                //保存字库
                ModelManager.Instance.Save();

                uiHelper.InitData(sdgv_items, GetPage);

            });

            task.Start();

            progress.ShowDialog(form);

            task.Wait();

        }

        /// <summary>
        /// 导入翻译文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbtn_tai_Click(object sender, EventArgs e)
        {
            var patch = OpenFile.OpenDialog("请选择翻译后的语言包", "翻译后的包(*.tbl)|*.tbl", "是否将文件导入到字库中");

            if (string.IsNullOrWhiteSpace(patch))
            {
                return;
            }

            var form = this;
            progress = new FormProgress();
            progress.SetFormText("正在解析tbl文件");

            var task = new Task(() =>
            {
                var tblParse = new TblParse(patch);

                var models = tblParse.Parser();

                form.Invoke((Action<string, int>)SetFormInfo, "导入汉化的进度", models.Count);

                var items = ModelManager.Instance.Items;

                //遍历已翻译过的
                models = models.Where(x => x.Value.Length > 0).ToList();

                foreach (var x in models)
                {
                    //已form主线程的方式执行代码
                    form.Invoke((Action)SetFormProgress);

                    var item = items.FirstOrDefault(_ => _.Key == x.Key);
                    if (item == null)
                        continue;

                    var hanLen = EncodingHelp.GetStringLength(item.Value);
                    var tranLen = EncodingHelp.GetStringLength(x.Value);

                    if (tranLen > hanLen)
                        continue;

                    item.Transform = x.Value;
                }

                //保存字库
                ModelManager.Instance.Save();

                uiHelper.InitData(sdgv_items, GetPage);

            });

            task.Start();
            progress.ShowDialog(form);
        }

        /// <summary>
        /// 执行汉化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sbtn_transform_Click(object sender, EventArgs e)
        {
            var patch = OpenFile.OpenDialog("请选择需要翻译的语言包", "需要翻译的包(*.tbl)|*.tbl", "是否翻译此文件");

            if (string.IsNullOrWhiteSpace(patch))
            {
                return;
            }

            var form = this;


            void UpdateStatus(string text)
            {
                statusStrip1.Items[1].Text = text;
            }

            void ShowMessage(string message)
            {
                MessageBoxEx.Show(message, "提示");
            }

            UpdateStatus("正在解析tbl文件");

            var task = new Task(() =>
            {
                form.Invoke((Action<string>)UpdateStatus, "执行汉化中");

                var tblTransform = new TblTransform(patch);

                tblTransform.TransformEventHandler += OnTransformEventHandler;

                var bytes = tblTransform.Transform(ModelManager.Instance.Items.ToArray());

                var _path = $@"{Environment.CurrentDirectory }\汉化";

                if (!Directory.Exists(_path))
                {
                    Directory.CreateDirectory(_path);
                }

                var path = $@"{_path}\data.bin.lan.kor_{DateTime.Now:yyyyMMddss}.tbl";

                using (var stream = new FileStream(path, FileMode.CreateNew))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                form.Invoke((Action<string>)ShowMessage, "汉化完成");
            });

            task.Start();
        }

        /// <summary>
        /// 搜索按钮被点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkinButton1_Click(object sender, EventArgs e)
        {
            var seach = stxt_search.Text;

            if (string.IsNullOrWhiteSpace(seach))
                return;

            var items = ModelManager.Instance.Items
                .Where(x => x.Key.Contains(seach) || x.Value.Contains(seach) || x.Transform.Contains(seach))
                .ToList();

            var formSearch = new FormSearch(items);

            try
            {
                formSearch.ShowDialog(this);
                uiHelper.init(this, ModelManager.Instance.Items);
                uiHelper.InitData(sdgv_items, GetPage);
            }
            catch (Exception)
            {
                formSearch = null;
            }
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
        /// 设置进度框信息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="count"></param>
        void SetFormInfo(string text, int count)
        {
            progress.SetFormText(text);
            progress.SetMaxProgress(count);
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        void SetFormProgress()
        {
            progress.ProgressHandler?.Invoke(1, null);
        }

        /// <summary>
        /// 获得页码信息
        /// </summary>
        void GetPage()
        {
            var count = ModelManager.Instance.Items.Count;

            if (count > uiHelper.pageSize)
            {
                maxPageSize = (count / uiHelper.pageSize) + 1;
            }

            this.slbl_pageIndex.Text = pageIndex.ToString();
            this.slbl_maxPage.Text = maxPageSize.ToString();
        }

        /// <summary>
        /// 执行汉化时需要用到的进度条回调。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTransformEventHandler(object sender, EventArgs e)
        {
            var s = statusStrip1.Items[3].Text.Split('/')[0];

            long.TryParse(s, out var index);

            index++;

            void ActionDelegate(long i)
            {
                if (i > ModelManager.Instance.Items.Count)
                    i = ModelManager.Instance.Items.Count;

                index = i;
                statusStrip1.Items[3].Text = $@"{index}/ {ModelManager.Instance.Items.Count}";
            }

            statusStrip1.Invoke((Action<long>)ActionDelegate, index);
        }
    }
}
