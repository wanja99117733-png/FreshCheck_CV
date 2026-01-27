using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreshCheck_CV.UIControl
{
    public class DarkTabControl : TabControl
    {
        // 테마 색상 정의
        private Color _backgroundColor = Color.FromArgb(30, 30, 30);
        private Color _tabColor = Color.FromArgb(45, 45, 48);
        private Color _selectedTabColor = Color.FromArgb(60, 60, 60);
        private Color _hoverTabColor = Color.FromArgb(75, 75, 80); // 마우스 오버 시 색상
        private Color _textColor = Color.FromArgb(240, 240, 240);
        private Color _borderColor = Color.FromArgb(60, 60, 60);

        // 현재 마우스가 올라가 있는 탭의 인덱스
        private int _hoverIndex = -1;

        public DarkTabControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.OptimizedDoubleBuffer, true);

            this.DoubleBuffered = true;
            this.SizeMode = TabSizeMode.Fixed;
            this.ItemSize = new Size(100, 30);
        }

        // 마우스가 움직일 때 어떤 탭 위에 있는지 확인
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int oldHoverIndex = _hoverIndex;
            _hoverIndex = -1;

            for (int i = 0; i < this.TabCount; i++)
            {
                if (this.GetTabRect(i).Contains(e.Location))
                {
                    _hoverIndex = i;
                    break;
                }
            }

            // 마우스가 올라간 탭이 바뀌었을 때만 다시 그리기 (성능 최적화)
            if (oldHoverIndex != _hoverIndex)
            {
                this.Invalidate();
            }
        }

        // 마우스가 컨트롤을 벗어나면 효과 제거
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 1. 배경 채우기
            using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // 2. 탭 헤더 그리기
            for (int i = 0; i < this.TabCount; i++)
            {
                Rectangle tabRect = this.GetTabRect(i);
                bool isSelected = (this.SelectedIndex == i);
                bool isHovered = (_hoverIndex == i);

                // 색상 결정 우선순위: 선택됨 > 마우스 오버 > 기본
                Color currentTabColor;
                if (isSelected) currentTabColor = _selectedTabColor;
                else if (isHovered) currentTabColor = _hoverTabColor;
                else currentTabColor = _tabColor;

                using (SolidBrush tabBrush = new SolidBrush(currentTabColor))
                {
                    g.FillRectangle(tabBrush, tabRect);
                }

                // 탭 텍스트
                TextRenderer.DrawText(g, this.TabPages[i].Text, this.Font, tabRect, _textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // 탭 간 구분선
                using (Pen p = new Pen(Color.FromArgb(20, 20, 20)))
                {
                    g.DrawRectangle(p, tabRect);
                }
            }

            // 3. 메인 영역 테두리
            if (this.TabCount > 0)
            {
                Rectangle pageRect = this.DisplayRectangle;
                pageRect.Inflate(1, 1);
                using (Pen p = new Pen(_borderColor, 1))
                {
                    g.DrawRectangle(p, pageRect);
                }
            }
        }
    }
}
