using System;
using System.Threading.Tasks;
using CCWin;

namespace MaplestoryM_3
{
    public partial class FormProgress : Skin_Color
    {
        /// <summary>
        /// 进度更新事件。
        /// </summary>
        public EventHandler ProgressHandler;

        /// <summary>
        /// 当前进度。
        /// </summary>
        private int cuProgress = 0;

        /// <summary>
        /// 最大进度
        /// </summary>
        private int maxProgress;

        public FormProgress()
        {
            InitializeComponent();
            this.ProgressHandler += OnProgressHandler;
            this.skinProgressBar1.BarGlass = true;
            this.skinProgressBar1.Value = cuProgress;
            this.skinProgressBar1.Minimum = cuProgress;
        }

        public void SetMaxProgress(int maxProgress)
        {
            this.maxProgress = maxProgress;
            this.skinProgressBar1.Maximum = this.maxProgress;
        }

        public void SetFormText(string t)
        {
            this.Text = t;
        }

        private void OnProgressHandler(object sender, EventArgs e)
        {
            cuProgress += (int)sender;
            this.skinProgressBar1.Value = cuProgress;

            if (cuProgress >= maxProgress)
            {
                MessageBoxEx.Show("导入完成", "提示");

                this.Close();
            }
        }

        private void FormProgress_Load(object sender, EventArgs e)
        {

        }
    }
}
