using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV
{
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);

            if (e.Item.Selected)
            {
                // 마우스 올렸을 때
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(60, 60, 65)))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
            else
            {
                // 기본 배경
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            //글자 색 흰색 고정
            e.TextColor = Color.White;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(45, 45, 48)))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }
    }
}