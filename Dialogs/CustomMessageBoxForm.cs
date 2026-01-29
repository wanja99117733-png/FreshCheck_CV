using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FreshCheck_CV.Dialogs
{
    public partial class CustomMessageBoxForm : Form
    {
        
        /* 다크모드 메시지 폼
            MessageBox.Show("등록되지 않은 관리자입니다.", "인증 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            > CustomMessageBoxForm.Show("등록되지 않은 관리자입니다.", "인증 실패");
        */

        // 드래그 기능용 Win32 API
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public CustomMessageBoxForm(string message, string title)
        {
            InitializeComponent();
            lblMessage.Text = message;
            labelTitle.Text = title;
        }

        // 어디서든 한 줄로 호출하기 위한 정적 메서드
        public static DialogResult Show(string message, string title = "알림", Form owner = null)
        {
            using (var msgBox = new CustomMessageBoxForm(message, title))
            {
                // 부모 창이 지정되면 중앙에 띄움
                return msgBox.ShowDialog(owner);
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void panelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, 0xA1, 0x2, 0);
            }
        }
    }
}