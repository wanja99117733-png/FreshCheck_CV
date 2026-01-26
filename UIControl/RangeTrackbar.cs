using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

    namespace FreshCheck_CV.UIControl
    {
        public partial class RangeTrackbar : UserControl
        {
            private int _minimum = 0;
            private int _maximum = 255;
            private int _valueLeft = 80;
            private int _valueRight = 120;

            private bool _dragLeft;
            private bool _dragRight;
            private bool _hoverLeft;
            private bool _hoverRight;

            // Style (Designer에서 조절 가능)
            [Category("Style")] public int TrackThickness { get; set; } = 6;
            [Category("Style")] public int ThumbDiameter { get; set; } = 16;
            [Category("Style")] public int CornerRadius { get; set; } = 6;

            [Category("Style")] public Color TrackBackColor { get; set; } = Color.FromArgb(60, 66, 74);
            [Category("Style")] public Color RangeColor { get; set; } = Color.FromArgb(90, 140, 220);
            [Category("Style")] public Color ThumbColor { get; set; } = Color.FromArgb(235, 235, 235);
            [Category("Style")] public Color ThumbBorderColor { get; set; } = Color.FromArgb(120, 120, 120);
            [Category("Style")] public Color BubbleBackColor { get; set; } = Color.FromArgb(35, 38, 44);

            public event EventHandler RangeChanged;
            protected virtual void OnRangeChanged() => RangeChanged?.Invoke(this, EventArgs.Empty);

            [DefaultValue(0)]
            public int Minimum { get => _minimum; set { _minimum = value; Invalidate(); } }

            [DefaultValue(255)]
            public int Maximum { get => _maximum; set { _maximum = value; Invalidate(); } }

            public int ValueLeft
            {
                get => _valueLeft;
                set
                {
                    _valueLeft = Clamp(value);
                    OnRangeChanged();
                    Invalidate();
                }
            }

            public int ValueRight
            {
                get => _valueRight;
                set
                {
                    _valueRight = Clamp(value);
                    OnRangeChanged();
                    Invalidate();
                }
            }

            public RangeTrackbar()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw, true);

                TabStop = true;
                Height = 54;
                BackColor = Color.Transparent;
            }

            public void SetThreshold(int left, int right)
            {
                ValueLeft = left;
                ValueRight = right;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                RectangleF track = GetTrackRect();

                // Track
                using (GraphicsPath trackPath = CreateRoundRect(track, CornerRadius))
                using (SolidBrush b = new SolidBrush(TrackBackColor))
                {
                    g.FillPath(b, trackPath);
                }

                // Range
                float leftPx = ValueToPixel(_valueLeft, track);
                float rightPx = ValueToPixel(_valueRight, track);

                float rangeL = Math.Min(leftPx, rightPx);
                float rangeR = Math.Max(leftPx, rightPx);

                RectangleF rangeRect = new RectangleF(rangeL, track.Y, Math.Max(1f, rangeR - rangeL), track.Height);

                using (GraphicsPath rangePath = CreateRoundRect(rangeRect, CornerRadius))
                using (SolidBrush rb = new SolidBrush(RangeColor))
                {
                    g.FillPath(rb, rangePath);
                }

                // Thumbs
                DrawThumb(g, leftPx, track, _hoverLeft || _dragLeft);
                DrawThumb(g, rightPx, track, _hoverRight || _dragRight);

                // Bubble while dragging
                if (_dragLeft) DrawBubble(g, leftPx, track, $"−{_valueLeft}");
                if (_dragRight) DrawBubble(g, rightPx, track, $"+{_valueRight}");
            }

            private void DrawThumb(Graphics g, float x, RectangleF track, bool hot)
            {
                float cy = track.Y + track.Height / 2f;
                float r = ThumbDiameter / 2f;

                RectangleF thumb = new RectangleF(x - r, cy - r, ThumbDiameter, ThumbDiameter);

                using (SolidBrush sb = new SolidBrush(Color.FromArgb(hot ? 90 : 60, 0, 0, 0)))
                {
                    g.FillEllipse(sb, new RectangleF(thumb.X + 1.5f, thumb.Y + 1.5f, thumb.Width, thumb.Height));
                }

                using (SolidBrush b = new SolidBrush(ThumbColor))
                using (Pen p = new Pen(hot ? Color.White : ThumbBorderColor, hot ? 2f : 1f))
                {
                    g.FillEllipse(b, thumb);
                    g.DrawEllipse(p, thumb);
                }
            }

            private void DrawBubble(Graphics g, float x, RectangleF track, string text)
            {
                using (Font f = new Font(Font.FontFamily, Font.Size, FontStyle.Regular))
                {
                    SizeF sz = g.MeasureString(text, f);
                    float paddingX = 10f;
                    float paddingY = 6f;

                    float w = sz.Width + paddingX * 2;
                    float h = sz.Height + paddingY * 2;

                    float bx = x - w / 2f;
                    float by = track.Y - h - 10f;

                    RectangleF bubble = new RectangleF(bx, by, w, h);

                    using (GraphicsPath p = CreateRoundRect(bubble, 10))
                    using (SolidBrush bb = new SolidBrush(BubbleBackColor))
                    using (Pen border = new Pen(Color.FromArgb(90, 90, 90)))
                    using (SolidBrush tb = new SolidBrush(ForeColor))
                    {
                        g.FillPath(bb, p);
                        g.DrawPath(border, p);
                        g.DrawString(text, f, tb, bubble.X + paddingX, bubble.Y + paddingY);
                    }
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                Focus();

                RectangleF track = GetTrackRect();
                float leftPx = ValueToPixel(_valueLeft, track);
                float rightPx = ValueToPixel(_valueRight, track);

                bool onLeft = IsOnThumb(e.Location, leftPx, track);
                bool onRight = IsOnThumb(e.Location, rightPx, track);

                if (onLeft) _dragLeft = true;
                else if (onRight) _dragRight = true;
                else
                {
                    // Track click -> move nearest thumb
                    float dxL = Math.Abs(e.X - leftPx);
                    float dxR = Math.Abs(e.X - rightPx);

                    int v = PixelToValue(e.X, track);
                    if (dxL <= dxR) ValueLeft = v;
                    else ValueRight = v;
                }

                Invalidate();
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                _dragLeft = false;
                _dragRight = false;
                Invalidate();
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                RectangleF track = GetTrackRect();
                float leftPx = ValueToPixel(_valueLeft, track);
                float rightPx = ValueToPixel(_valueRight, track);

                if (_dragLeft)
                {
                    ValueLeft = PixelToValue(e.X, track);
                    return;
                }
                if (_dragRight)
                {
                    ValueRight = PixelToValue(e.X, track);
                    return;
                }

                bool hoverL = IsOnThumb(e.Location, leftPx, track);
                bool hoverR = IsOnThumb(e.Location, rightPx, track);

                if (hoverL != _hoverLeft || hoverR != _hoverRight)
                {
                    _hoverLeft = hoverL;
                    _hoverRight = hoverR;
                    Cursor = (hoverL || hoverR) ? Cursors.Hand : Cursors.Default;
                    Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _hoverLeft = false;
                _hoverRight = false;
                Cursor = Cursors.Default;
                Invalidate();
            }

            private RectangleF GetTrackRect()
            {
                float pad = 12f;
                float y = Height / 2f - TrackThickness / 2f + 6f;
                float x = pad;
                float w = Math.Max(10f, Width - pad * 2);
                return new RectangleF(x, y, w, TrackThickness);
            }

            private bool IsOnThumb(Point p, float thumbCenterX, RectangleF track)
            {
                float cy = track.Y + track.Height / 2f;
                float r = ThumbDiameter / 2f + 2f;
                RectangleF hit = new RectangleF(thumbCenterX - r, cy - r, r * 2, r * 2);
                return hit.Contains(p);
            }

            private int Clamp(int v)
            {
                if (v < _minimum) return _minimum;
                if (v > _maximum) return _maximum;
                return v;
            }

            private float ValueToPixel(int v, RectangleF track)
            {
                if (_maximum == _minimum) return track.Left;
                float ratio = (float)(v - _minimum) / (_maximum - _minimum);
                return track.Left + ratio * track.Width;
            }

            private int PixelToValue(float px, RectangleF track)
            {
                if (track.Width <= 1f) return _minimum;
                float ratio = (px - track.Left) / track.Width;
                int v = _minimum + (int)(ratio * (_maximum - _minimum));
                return Clamp(v);
            }

            private static GraphicsPath CreateRoundRect(RectangleF r, float radius)
            {
                float d = radius * 2f;
                GraphicsPath p = new GraphicsPath();
                p.AddArc(r.X, r.Y, d, d, 180, 90);
                p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }
        }
    }