using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FreshCheck_CV
{
    public partial class CameraForm : DockContent
    {
        private bool _isPickMode = false;
        private Bitmap _rawFrame;   // 오버레이/프리뷰 없는 원본
        private readonly object _rawLock = new object();

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

        public event EventHandler<ColorPickedEventArgs> ColorPicked;
        public CameraForm()
        {
            InitializeComponent();

            if (imageViewCtrl != null)
            {
                imageViewCtrl.MouseClick += ImageViewCtrl_MouseClick;
            }
        }

        public void BeginPickColor()
        {
            _isPickMode = true;
        }

        private void ImageViewCtrl_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isPickMode == false)
            {
                return;
            }

            _isPickMode = false;

            if (imageViewCtrl == null)
            {
                return;
            }

            if (imageViewCtrl.TryPickColor(e.Location, out Color pickedColor) == false)
            {
                return;
            }

            ColorPicked?.Invoke(this, new ColorPickedEventArgs(pickedColor));
            // 폼 배경
            this.BackColor = Color.FromArgb(25, 25, 25);

            // 이미지 컨트롤 외 여백 색
            imageViewCtrl.BackColor = Color.Black;
        }




        //#3_CAMERAVIEW_PROPERTY#1 이미지 경로를 받아 PictureBox에 이미지를 로드하는 메서드
        public void LoadImage(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                return;
            }

            using (var temp = (Bitmap)Image.FromFile(filePath))
            {
                imageViewCtrl.LoadBitmap(new Bitmap(temp)); // 복사본 전달
            }
        }

        private void CameraForm_Resize_1(object sender, EventArgs e)
        {
            int margin = 0;
            imageViewCtrl.Width = this.Width - margin * 2;
            imageViewCtrl.Height = this.Height - margin * 2;

            imageViewCtrl.Location = new System.Drawing.Point(margin, margin);
        }

        public void UpdateDisplay(Bitmap bitmap = null)
        {
            if (imageViewCtrl == null || bitmap == null)
            {
                return;
            }

            imageViewCtrl.LoadBitmap(bitmap);
        }

        public Bitmap GetDisplayImage()
        {
            if (imageViewCtrl == null)
            {
                return null;
            }

            Bitmap cur = imageViewCtrl.GetCurBitmap();
            if (cur == null)
            {
                return null;
            }

            return new Bitmap(cur);
        }

        public Color? TryPickColorFromDisplay(Point clientPoint)
        {
            Bitmap bmp = imageViewCtrl?.GetCurBitmap();
            if (bmp == null)
            {
                return null;
            }

            // 가장 단순한 1차 버전: 컨트롤 좌표를 비트맵 좌표로 비례 매핑(정확도 100%는 아니지만 시작용)
            int x = (int)(clientPoint.X * (bmp.Width / (float)imageViewCtrl.Width));
            int y = (int)(clientPoint.Y * (bmp.Height / (float)imageViewCtrl.Height));

            if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height)
            {
                return null;
            }

            return bmp.GetPixel(x, y);
        }


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
