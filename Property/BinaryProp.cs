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
    public partial class BinaryProp : UserControl
    {
        public BinaryProp()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(30, 34, 40);

            // 버튼 다크화
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;
            button1.BackColor = Color.FromArgb(50, 55, 65);
            button1.ForeColor = Color.White;

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void BinaryProp_Load(object sender, EventArgs e)
        {

        }
    }
}
