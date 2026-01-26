using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreshCheck_CV.Core;
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
            SetButtonsEnabled(false); // ← 누른 순간 전부 잠금

            try
            {
                Global.Inst.InspStage.RunMoldInspectionTemp();
            }
            finally
            {
                SetButtonsEnabled(true); // ← 작업 끝나면 복구
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            SetButtonsEnabled(false);

            try
            {
                // Pause 처리 (현재는 없음)
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            SetButtonsEnabled(false);

            try
            {
                // Stop 처리 (현재는 없음)
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }
        
        private void SetButtonsEnabled(bool enabled)
        {
            btnStart.Enabled = enabled;
            btnPause.Enabled = enabled;
            btnStop.Enabled = enabled;
        }

    }
}