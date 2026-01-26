using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreshCheck_CV.UIControl
{
    public class DarkTabControl : TabControl
    {
        [Category("DarkTheme")]
        public Color HeaderBackColor { get; set; } = Color.FromArgb(30, 34, 40);

        [Category("DarkTheme")]
        public Color BorderColor { get; set; } = Color.Black;

        [Category("DarkTheme")]
        public Color TabBackColor { get; set; } = Color.FromArgb(30, 34, 40);

        [Category("DarkTheme")]
        public Color TabSelectedBackColor { get; set; } = Color.FromArgb(45, 45, 48);

        [Category("DarkTheme")]
        public Color TabForeColor { get; set; } = Color.LightGray;

        [Category("DarkTheme")]
        public Color TabSelectedForeColor { get; set; } = Color.White;

        [Category("DarkTheme")]
        public Color TabBorderColor { get; set; } = Color.FromArgb(70, 70, 70);

        public DarkTabControl()
        {
            //SetStyle(ControlStyles.UserPaint |
            //         ControlStyles.AllPaintingInWmPaint |
            //         ControlStyles.OptimizedDoubleBuffer |
            //         ControlStyles.ResizeRedraw, true);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;


        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control is TabPage page)
            {
                page.BackColor = TabBackColor;
                page.ForeColor = TabForeColor;
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var page = TabPages[e.Index];
            var r = GetTabRect(e.Index);

            bool selected = (e.Index == SelectedIndex);

            Color back = selected ? TabSelectedBackColor : TabBackColor;
            Color fore = selected ? TabSelectedForeColor : TabForeColor;

            using (var b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, r);

            using (var p = new Pen(TabBorderColor))
                e.Graphics.DrawRectangle(p, r);

            TextRenderer.DrawText(
                e.Graphics,
                page.Text,
                Font,
                r,
                fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    // 기본 배경 처리 먼저(페이지/컨트롤 영역)
        //    base.OnPaintBackground(e);

        //    // 그 다음 헤더 스트립만 칠하기 (탭은 이후 OnDrawItem에서 그려짐)
        //    int headerHeight = DisplayRectangle.Y;
        //    if (headerHeight > 0)
        //    {
        //        var headerRect = new Rectangle(0, 0, Width, headerHeight);
        //        using (var b = new SolidBrush(HeaderBackColor))
        //            e.Graphics.FillRectangle(b, headerRect);
        //    }
        //}

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    // 여기서 헤더를 다시 칠하면 탭이 가려짐 → base만 호출
        //    base.OnPaint(e);
        //}

        private const int WM_PAINT = 0x000F;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_PAINT)
                PaintHeaderStripExcludeTabs();
        }

        private void PaintHeaderStripExcludeTabs()
        {
            int headerHeight = DisplayRectangle.Y;
            if (headerHeight <= 0) return;

            var headerRect = new Rectangle(0, 0, Width, headerHeight);

            using (var g = Graphics.FromHwnd(Handle))
            using (var brush = new SolidBrush(HeaderBackColor))
            using (var region = new Region(headerRect))
            {
                // 탭 버튼 영역은 절대 덮지 않음
                for (int i = 0; i < TabCount; i++)
                    region.Exclude(GetTabRect(i));

                g.FillRegion(brush, region);
            }
        }

    }
}
