using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV.Property
{
    //검사 완료 메세지창 
    public partial class DarkMessageForm : Form
    {
        public DarkMessageForm(string message)
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(28, 32, 38);
            this.ForeColor = Color.White;
            this.ShowInTaskbar = false;

            lblMessage.Text = message;

            this.AcceptButton = btnOk;
            this.CancelButton = btnOk;

            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.FlatAppearance.BorderSize = 1;
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            btnOk.FlatAppearance.MouseOverBackColor = btnOk.BackColor;
            btnOk.FlatAppearance.MouseDownBackColor = btnOk.BackColor;
            btnOk.TabStop = false;
        }
        //창닫기
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}