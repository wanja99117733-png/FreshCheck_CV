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
    public partial class DefectForm : DockContent
    {
        public DefectForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 디펙 이미지 썸네일 추가용 메서드
        /// </summary>
        public void AddDefectImage(Image image)
        {
            PictureBox pic = new PictureBox
            {
                Image = image,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 120,
                Height = 90,
                Margin = new Padding(6),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            flowDefectImages.Controls.Add(pic);
        }
    }
}