using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FreshCheck_CV.Dialogs
{
    public partial class PasswordCheckForm : Form
    {
        // 제목 바 드래그 기능
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        // 버튼 색상 설정 (ExitConfirmForm과 동일하게 유지)
        private Color _confirmNormal = Color.FromArgb(60, 110, 180);
        private Color _confirmHover = Color.FromArgb(70, 120, 195);
        private Color _confirmPress = Color.FromArgb(45, 90, 150);

        private Color _cancelNormal = Color.FromArgb(85, 85, 85);
        private Color _cancelHover = Color.FromArgb(100, 100, 100);
        private Color _cancelPress = Color.FromArgb(65, 65, 65);

        public string Password => txtPassword.Text; // 입력된 비밀번호 반환

        public PasswordCheckForm()
        {
            InitializeComponent();
            btnConfirm.BackColor = _confirmNormal;
            btnCancel.BackColor = _cancelNormal;

            AcceptButton = btnConfirm;
            CancelButton = btnCancel;
        }

        private void panelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 확인 버튼 이벤트
        private void btnConfirm_MouseEnter(object sender, EventArgs e) => btnConfirm.BackColor = _confirmHover;
        private void btnConfirm_MouseLeave(object sender, EventArgs e) => btnConfirm.BackColor = _confirmNormal;
        private void btnConfirm_MouseDown(object sender, MouseEventArgs e) => btnConfirm.BackColor = _confirmPress;
        private void btnConfirm_MouseUp(object sender, MouseEventArgs e) => btnConfirm.BackColor = _confirmHover;

        // 취소 버튼 이벤트
        private void btnCancel_MouseEnter(object sender, EventArgs e) => btnCancel.BackColor = _cancelHover;
        private void btnCancel_MouseLeave(object sender, EventArgs e) => btnCancel.BackColor = _cancelNormal;
        private void btnCancel_MouseDown(object sender, MouseEventArgs e) => btnCancel.BackColor = _cancelPress;
        private void btnCancel_MouseUp(object sender, MouseEventArgs e) => btnCancel.BackColor = _cancelHover;
    }
}