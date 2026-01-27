using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV.UIControl
{
    public sealed class OverallRateBarControl : Control
    {
        private double _ok;
        private double _mold;
        private double _scratch;

        public OverallRateBarControl()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(30, 34, 40);
            ForeColor = Color.Gainsboro;
        }

        public void SetRates(double okRate, double moldRate, double scratchRate)
        {
            _ok = Clamp01(okRate);
            _mold = Clamp01(moldRate);
            _scratch = Clamp01(scratchRate);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.Clear(BackColor);

            int pad = 10;
            int rowH = 22;
            int gap = 10;

            int labelW = 70;
            int barW = Math.Max(10, Width - pad * 2 - labelW - 70);
            int xLabel = pad;
            int xBar = pad + labelW;
            int xPct = xBar + barW + 8;

            DrawRow(g, 0, "OK", _ok, Color.FromArgb(120, 200, 80), xLabel, xBar, xPct, pad, rowH, gap);
            DrawRow(g, 1, "Mold", _mold, Color.FromArgb(220, 60, 60), xLabel, xBar, xPct, pad, rowH, gap);
            DrawRow(g, 2, "Scratch", _scratch, Color.FromArgb(240, 160, 60), xLabel, xBar, xPct, pad, rowH, gap);
        }

        private void DrawRow(Graphics g, int idx, string name, double rate, Color c,
            int xLabel, int xBar, int xPct, int pad, int rowH, int gap)
        {
            int y = pad + idx * (rowH + gap);

            using (var f = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var bText = new SolidBrush(Color.Gainsboro))
            using (var bBar = new SolidBrush(c))
            using (var bBack = new SolidBrush(Color.FromArgb(45, 50, 58)))
            using (var pen = new Pen(Color.FromArgb(70, 70, 70)))
            {
                g.DrawString(name, f, bText, xLabel, y + 2);

                // bar background
                g.FillRectangle(bBack, xBar, y + 4, Math.Max(10, Width - xBar - pad - 70), rowH - 8);

                int bw = Math.Max(10, Width - xBar - pad - 70);
                int filled = (int)Math.Round(bw * rate);
                g.FillRectangle(bBar, xBar, y + 4, Math.Max(0, filled), rowH - 8);

                g.DrawRectangle(pen, xBar, y + 4, bw, rowH - 8);

                int pct = (int)Math.Round(rate * 100);
                g.DrawString($"{pct}%", f, bText, xPct, y + 2);
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