using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FreshCheck_CV
{
    public partial class CameraForm : DockContent
    {
        private Bitmap _rawFrame;   // 오버레이/프리뷰 없는 원본
        private readonly object _rawLock = new object();

        public event EventHandler<ColorPickedEventArgs> ColorPicked;

        public CameraForm()
        {
            InitializeComponent();

            if (imageViewCtrl != null)
            {
                imageViewCtrl.MouseClick += ImageViewCtrl_MouseClick;
            }
        }

        // ===============================
        // Raw Frame 관리
        // ===============================
        public void SetRawFrame(Bitmap frame)
        {
            if (frame == null) return;

            lock (_rawLock)
            {
                _rawFrame?.Dispose();
                _rawFrame = new Bitmap(frame);
            }
        }

        public Bitmap GetRawFrameCopy()
        {
            lock (_rawLock)
            {
                return _rawFrame != null ? new Bitmap(_rawFrame) : null;
            }
        }

        // ===============================
        // 색상 픽킹 (클릭 기반, 단순/안정)
        // ===============================
        private void ImageViewCtrl_MouseClick(object sender, MouseEventArgs e)
        {
            if (imageViewCtrl == null)
                return;

            if (imageViewCtrl.TryPickColor(e.Location, out Color pickedColor) == false)
                return;

            ColorPicked?.Invoke(this, new ColorPickedEventArgs(pickedColor));

            // UI 톤 유지
            this.BackColor = Color.FromArgb(25, 25, 25);
            imageViewCtrl.BackColor = Color.Black;
        }

        // ===============================
        // 이미지 로딩
        // ===============================
        public void LoadImage(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            using (var temp = (Bitmap)Image.FromFile(filePath))
            {
                imageViewCtrl.LoadBitmap(new Bitmap(temp));
            }
        }

        // ===============================
        // Resize 대응
        // ===============================
        private void CameraForm_Resize_1(object sender, EventArgs e)
        {
            if (imageViewCtrl == null)
                return;

            int margin = 0;
            imageViewCtrl.Width = this.Width - margin * 2;
            imageViewCtrl.Height = this.Height - margin * 2;
            imageViewCtrl.Location = new Point(margin, margin);
        }

        // ===============================
        // 디스플레이 갱신
        // ===============================
        public void UpdateDisplay(Bitmap bitmap = null)
        {
            if (imageViewCtrl == null || bitmap == null)
                return;

            imageViewCtrl.LoadBitmap(bitmap);
        }

        public Bitmap GetDisplayImage()
        {
            if (imageViewCtrl == null)
                return null;

            Bitmap cur = imageViewCtrl.GetCurBitmap();
            if (cur == null)
                return null;

            return new Bitmap(cur);
        }

        // ===============================
        // 좌표 기반 색 픽킹 (보조 API)
        // ===============================
        public Color? TryPickColorFromDisplay(Point clientPoint)
        {
            Bitmap bmp = imageViewCtrl?.GetCurBitmap();
            if (bmp == null)
                return null;

            int x = (int)(clientPoint.X * (bmp.Width / (float)imageViewCtrl.Width));
            int y = (int)(clientPoint.Y * (bmp.Height / (float)imageViewCtrl.Height));

            if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height)
                return null;

            return bmp.GetPixel(x, y);
        }

        // ===============================
        // 이벤트 인자
        // ===============================
        public sealed class ColorPickedEventArgs : EventArgs
        {
            public ColorPickedEventArgs(Color color)
            {
                Color = color;
            }

            public Color Color { get; }
        }
    }
}
