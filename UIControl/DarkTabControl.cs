using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreshCheck_CV.UIControl
{
    public class DarkTabControl : TabControl
    {
        // 다크 테마 색상 정의
        private Color _backgroundColor = Color.FromArgb(30, 30, 30);
        private Color _tabColor = Color.FromArgb(45, 45, 48);
        private Color _selectedTabColor = Color.FromArgb(60, 60, 60);
        private Color _textColor = Color.FromArgb(240, 240, 240);

        public DarkTabControl()
        {
            // 1. 핵심 설정: 컨트롤의 모든 영역을 직접 그리겠다고 선언합니다.
            // UserPaint를 true로 설정해야 배경 영역(하얀 부분)을 제어할 수 있습니다.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw |
                          ControlStyles.OptimizedDoubleBuffer, true);

            this.DoubleBuffered = true;
            this.SizeMode = TabSizeMode.Fixed;
            this.ItemSize = new Size(100, 30); // 탭 버튼 크기
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // --- 1. 배경색 칠하기 (하얀색 여백 제거) ---
            // 전체 배경을 어두운 색으로 채웁니다.
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // --- 2. 탭 버튼(헤더) 그리기 ---
            for (int i = 0; i < this.TabCount; i++)
            {
                Rectangle tabRect = this.GetTabRect(i);
                bool isSelected = (this.SelectedIndex == i);

                // 선택된 탭과 선택되지 않은 탭의 색상 구분
                Color tabColor = isSelected ? Color.FromArgb(60, 60, 60) : Color.FromArgb(45, 45, 48);

                using (SolidBrush tabBrush = new SolidBrush(tabColor))
                {
                    g.FillRectangle(tabBrush, tabRect);
                }

                // 탭 텍스트 그리기
                TextRenderer.DrawText(g, this.TabPages[i].Text, this.Font, tabRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // 탭 테두리 (버튼 사이의 아주 얇은 구분선)
                using (Pen p = new Pen(Color.FromArgb(20, 20, 20)))
                {
                    g.DrawRectangle(p, tabRect);
                }
            }

            // --- 3. 메인 콘텐츠 영역 테두리 그리기 (중요!) ---
            // TabPage가 들어가는 영역의 하얀 테두리를 다크한 색상으로 덮어버립니다.
            if (this.TabCount > 0)
            {
                Rectangle pageRect = this.DisplayRectangle;
                // 테두리를 1픽셀 정도 바깥으로 확장하여 깔끔하게 그립니다.
                pageRect.Inflate(1, 1);
                using (Pen p = new Pen(Color.FromArgb(60, 60, 60), 1))
                {
                    g.DrawRectangle(p, pageRect);
                }
            }
        }

    }
}
