using DocumentFormat.OpenXml.Drawing.Charts;
using FreshCheck_CV.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV.Property
{
    public sealed class TrendChartControl : Control
    {
        private TrendPoint[] _points = Array.Empty<TrendPoint>();

        public TrendChartControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(30, 34, 40);
        }

        public void SetPoints(TrendPoint[] points)
        {
            _points = points ?? Array.Empty<TrendPoint>();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.Clear(BackColor);

            if (_points.Length < 2)
                return;

            int padL = 10;
            int padT = 10;
            int padR = 10;
            int padB = 28; // 아래 범례 공간

            var rect = new Rectangle(padL, padT, Width - padL - padR, Height - padT - padB);
            if (rect.Width <= 10 || rect.Height <= 10) return;

            // 축/그리드
            using (var gridPen = new Pen(Color.FromArgb(60, 60, 60), 1f))
            using (var axisPen = new Pen(Color.FromArgb(80, 80, 80), 1f))
            {
                // 25/50/75% 라인
                for (int i = 1; i <= 3; i++)
                {
                    float y = rect.Top + rect.Height * (i / 4f);
                    g.DrawLine(gridPen, rect.Left, y, rect.Right, y);
                }

                // 박스
                g.DrawRectangle(axisPen, rect);
            }

            int minX = _points.First().X;
            int maxX = _points.Last().X;
            int spanX = Math.Max(1, maxX - minX);

            PointF Map(int x, double v)
            {
                double x01 = (double)(x - minX) / spanX;    // 0~1
                double y01 = 1.0 - Clamp01(v);              // 위로 갈수록 1

                float px = rect.Left + (float)(rect.Width * x01);
                float py = rect.Top + (float)(rect.Height * y01);
                return new PointF(px, py);
            }

            using (var okPen = new Pen(Color.FromArgb(120, 200, 80), 2f))
            using (var moldPen = new Pen(Color.FromArgb(220, 60, 60), 2f))
            using (var scratchPen = new Pen(Color.FromArgb(240, 160, 60), 2f))
            {
                DrawLine(g, okPen, _points, p => p.OkRate, Map);
                DrawLine(g, moldPen, _points, p => p.MoldRate, Map);
                DrawLine(g, scratchPen, _points, p => p.ScratchRate, Map);

                // 범례(하단)
                int y = rect.Bottom + 6;
                DrawLegend(g, okPen.Color, "OK", rect.Left, y);
                DrawLegend(g, moldPen.Color, "Mold", rect.Left + 70, y);
                DrawLegend(g, scratchPen.Color, "Scratch", rect.Left + 160, y);
            }
        }

        private static void DrawLine(Graphics g, Pen pen, TrendPoint[] pts, Func<TrendPoint, double> sel, Func<int, double, PointF> map)
        {
            for (int i = 1; i < pts.Length; i++)
            {
                var p0 = map(pts[i - 1].X, sel(pts[i - 1]));
                var p1 = map(pts[i].X, sel(pts[i]));
                g.DrawLine(pen, p0, p1);
            }
        }

        private static void DrawLegend(Graphics g, Color c, string text, int x, int y)
        {
            using (var b = new SolidBrush(c))
            using (var f = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var w = new SolidBrush(Color.Gainsboro))
            {
                g.FillRectangle(b, x, y + 3, 12, 12);
                g.DrawString(text, f, w, x + 16, y);
            }
        }

        private static double Clamp01(double v)
        {
            if (v < 0) return 0;
            if (v > 1) return 1;
            return v;
        }
    }
}
