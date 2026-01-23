using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;


namespace FreshCheck_CV.Dialogs
{
    public partial class ExitConfirmForm : Form

    {   //제목바 드래그로 창 이동되게 만들기
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        //종료 메세지 이벤트
        private Color exitNormal = Color.FromArgb(60, 110, 180);
        private Color exitHover = Color.FromArgb(70, 120, 195);
        private Color exitPress = Color.FromArgb(45, 90, 150);

        private Color cancelNormal = Color.FromArgb(85, 85, 85);
        private Color cancelHover = Color.FromArgb(100, 100, 100);
        private Color cancelPress = Color.FromArgb(65, 65, 65);


        public ExitConfirmForm()
        {
            InitializeComponent();

            btnCancel.BackColor = cancelNormal;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
        //panelTitle에 MouseDown 이벤트 연결
        private void panelTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        //종료메세지 확인버튼 마우스 이벤트 생성
        private void btnExit_MouseEnter(object sender, EventArgs e)
        {
            btnExit.BackColor = exitHover;
        }
        private void btnExit_MouseLeave(object sender, EventArgs e)
        {
            btnExit.BackColor = exitNormal;
        }
        private void btnExit_MouseDown(object sender, MouseEventArgs e)
        {
            btnExit.BackColor = exitPress;
        }
        private void btnExit_MouseUp(object sender, MouseEventArgs e)
        {
            btnExit.BackColor = exitHover;
        }
        //종료메세지 취소버튼 이벤트
        private void btnCancel_MouseEnter(object sender, EventArgs e)
        {
            btnCancel.BackColor = cancelHover;
        }

        private void btnCancel_MouseLeave(object sender, EventArgs e)
        {
            btnCancel.BackColor = cancelNormal;
        }

        private void btnCancel_MouseDown(object sender, MouseEventArgs e)
        {
            btnCancel.BackColor = cancelPress;
        }
        private void btnCancel_MouseUp(object sender, MouseEventArgs e)
        {
            btnCancel.BackColor = cancelHover;
        }
    }
}
