using DocumentFormat.OpenXml.Drawing.Charts;
using FreshCheck_CV.Core;
using FreshCheck_CV.Inspect;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SaigeVision.Net.V2.Segmentation;
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
        //private bool _isPickMode = false;
        private Bitmap _rawFrame;   // 오버레이/프리뷰 없는 원본
        private readonly object _rawLock = new object();
        private PictureBox _fakeCursor;
        private bool _isPickingColor = false;

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
            InitFakeCursor();
        }

        //public void BeginPickColor()
        //{
        //    _isPickMode = true;
        //}

        private void ImageViewCtrl_MouseClick(object sender, MouseEventArgs e)
        {
            //if (_isPickMode == false)
            //{
            //    return;
            //}

            //_isPickMode = false;

            //if (imageViewCtrl == null)
            //{
            //    return;
            //}

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
            //#4_IMAGE_VIEWER#6 이미지 뷰어 컨트롤을 사용하여 이미지를 로드
            Image bitmap = Image.FromFile(filePath);
            imageViewCtrl.LoadBitmap((Bitmap)bitmap);
        }

        private void CameraForm_Resize_1(object sender, EventArgs e)
        {
            int margin = 0;
            imageViewCtrl.Width = this.Width - margin * 2;
            imageViewCtrl.Height = this.Height - margin * 2;

            imageViewCtrl.Location = new System.Drawing.Point(margin, margin);
        }
        public void UpdatePreviewWithScratch(Bitmap bitmap, SegmentationResult scratchResult)
        {
            if (imageViewCtrl != null)
            {
                imageViewCtrl.SetPreviewWithScratch(bitmap, scratchResult);
            }
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

        public Color? TryPickColorFromDisplay(System.Drawing.Point clientPoint)
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

        private void InitFakeCursor()
        {
            _fakeCursor = new PictureBox();

            string path = Path.Combine(
                Application.StartupPath,
                "droppericon_70x70.png"
            );

            using (var temp = Image.FromFile(path))
            {
                _fakeCursor.Image = new Bitmap(temp);
            }

            _fakeCursor.SizeMode = PictureBoxSizeMode.Zoom;
            _fakeCursor.Size = new System.Drawing.Size(48, 48);
            _fakeCursor.BackColor = Color.Transparent;
            _fakeCursor.Visible = false;
            _fakeCursor.Enabled = false;

            // 🔥 핵심
            imageViewCtrl.Controls.Add(_fakeCursor);
            _fakeCursor.BringToFront();
        }
        public void StartFakeCursorPick()
        {
            if (_isPickingColor)
                return;

            _isPickingColor = true;

            Cursor.Hide();
            _fakeCursor.Visible = true;

            // 🔥 핵심: imageViewCtrl에도 이벤트 연결
            this.MouseMove += CameraForm_MouseMove;
            imageViewCtrl.MouseMove += CameraForm_MouseMove;

            this.MouseDown += CameraForm_MouseDown;
            imageViewCtrl.MouseDown += CameraForm_MouseDown;
        }
        private void CameraForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPickingColor)
                return;

            // 🔥 핫스팟 보정 (스포이드 끝 기준)
            int offsetX = 6;
            int offsetY = 42;

            _fakeCursor.Left = e.X - offsetX;
            _fakeCursor.Top = e.Y - offsetY;
        }
        private void CameraForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_isPickingColor || e.Button != MouseButtons.Left)
                return;

            if (imageViewCtrl == null)
                return;

            // 🔥 기존 imageViewCtrl 픽킹 로직 재사용
            if (imageViewCtrl.TryPickColor(e.Location, out Color pickedColor))
            {
                //ColorPicked?.Invoke(this, new ColorPickedEventArgs(pickedColor));
            }

            EndFakeCursorPick();
        }

        private void EndFakeCursorPick()
        {
            _isPickingColor = false;

            _fakeCursor.Visible = false;
            Cursor.Show();

            this.MouseMove -= CameraForm_MouseMove;
            imageViewCtrl.MouseMove -= CameraForm_MouseMove;

            this.MouseDown -= CameraForm_MouseDown;
            imageViewCtrl.MouseDown -= CameraForm_MouseDown;
        }



        public Bitmap GetPreviewImage()
        {
            return imageViewCtrl.PreviewImage;
        }

        public void UpdatePreview(Bitmap bitmap)
        {
            if (imageViewCtrl != null)
            {
                imageViewCtrl.PreviewImage = bitmap;
                imageViewCtrl.Invalidate(); // 다시 그려줌. OnPaint 함수 호출.
                imageViewCtrl.ClearScratchResult();
            }
        }

        public void ClearPreviewImage()
        {
            if (imageViewCtrl != null)
                imageViewCtrl.PreviewImage = null;
        }
    }
}
