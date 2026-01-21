using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

namespace FreshCheck_CV
{
    public sealed class SplashForm : Form
    {
        private readonly PictureBox _picLogo;
        private readonly Label _lblMessage;
        private readonly ProgressBar _progressBar;

        public SplashForm()
        {
            Text = "FreshCheck_CV Loading";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 260);

            _picLogo = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(480, 140),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            //  프로젝트 리소스에 로고를 넣고 여기서 할당
             _picLogo.Image = Properties.Resources.Logo;

            _lblMessage = new Label
            {
                Location = new Point(20, 170),
                Size = new Size(480, 22),
                Text = "시작 중..."
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(20, 200),
                Size = new Size(480, 20),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            Controls.Add(_picLogo);
            Controls.Add(_lblMessage);
            Controls.Add(_progressBar);
        }

        internal void UpdateProgress(InitProgress progress)
        {
            int percent = progress.Percent;
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            _progressBar.Value = percent;

            // Stage에서 준 문구 그대로 표시
            string message = progress.Message;
            if (string.IsNullOrWhiteSpace(message))
                message = "초기화 중...";

            _lblMessage.Text = string.Format("{0}% - {1}", percent, message);
        }
    }
}
