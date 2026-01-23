using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV
{// 다크 테마용 MenuStrip 렌더러
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
            Color backColor;

            if (e.Item.Pressed)
            {
                // 클릭했을 때
                backColor = Color.FromArgb(0, 84, 153);
            }
            else if (e.Item.Selected)
            {
                // 마우스 올렸을 때
                backColor = Color.FromArgb(0, 122, 204);
            }
            else
            {
                // 기본 배경
                backColor = Color.FromArgb(45, 45, 48);
            }

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        // 메뉴 아이콘 색 통일 (흰색)
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Image == null)
                return;

            Bitmap bmp = new Bitmap(e.Image.Width, e.Image.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.DrawImage(e.Image, 0, 0);
                }
            }

            e.Graphics.DrawImage(bmp, e.ImageRectangle);
        }

        // 선택 테두리 제거 (흰 박스 방지)
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // 아무 것도 그리지 않음
        }
    }
}
