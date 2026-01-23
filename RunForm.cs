using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FreshCheck_CV
{
    public partial class RunForm : DockContent
    {
        public RunForm()
        {
            InitializeComponent();
            ApplyDarkTheme();
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(28, 32, 38);
            this.ForeColor = Color.White;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // TODO: 검사 시작 로직 연결
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            // TODO: 검사 일시정지 로직
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // TODO: 검사 종료 로직
        }
    }
}