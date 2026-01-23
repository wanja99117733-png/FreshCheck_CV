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
    public partial class ImageFilterProp : UserControl
    {
        public ImageFilterProp()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(30, 34, 40);

            cbImageFilter.BackColor = Color.FromArgb(45, 45, 48);
            cbImageFilter.ForeColor = Color.White;
            cbImageFilter.FlatStyle = FlatStyle.Flat;

        }
    }
}
