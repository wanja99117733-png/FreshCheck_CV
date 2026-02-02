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
using System.Drawing.Drawing2D;
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

        public void BeginPickColor()
        {
            _isPickMode = !_isPickMode; // 픽커를 뺄 수도 있게 변경함

            if (_isPickMode)
                StartFakeCursorPick(); // 🔥 가짜 커서 방식 시작
            else
                EndFakeCursorPick(); // 픽커를 빼면 디폴트 커서로 변경함
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
        public void LoadImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            if (File.Exists(imagePath) == false)
                return;

            Bitmap loaded = null;
            Bitmap resized640 = null;

            try
            {
                loaded = LoadBitmapNoLock(imagePath);

                // ✅ 마지막 입력을 640x640으로 고정 (Letterbox 추천)
                resized640 = FreshCheck_CV.Util.ImageResizeHelper.ToSquare(
                    loaded,
                    640,
                    FreshCheck_CV.Util.SquareResizeMode.Letterbox);

                // ✅ ImageViewCtrl은 LoadBitmap을 사용
                imageViewCtrl.LoadBitmap(resized640);

                // LoadBitmap이 resized640을 소유하게 되므로 여기서 Dispose 하면 안 됨
                resized640 = null;
            }
            finally
            {
                loaded?.Dispose();
                resized640?.Dispose(); // null이면 아무 일 없음
            }
        }
        


        private static Bitmap LoadBitmapNoLock(string path)
        {
            // 파일 락 방지: FileStream → MemoryStream → Bitmap 복제
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var ms = new MemoryStream())
            {
                fs.CopyTo(ms);
                ms.Position = 0;

                using (var temp = new Bitmap(ms))
                {
                    // 스트림과 분리된 진짜 Bitmap
                    return new Bitmap(temp);
                }
            }
        }

        private static Bitmap ResizeHalfFast(Bitmap src)
        {
            if (src == null)
                return null;

            int w = Math.Max(1, src.Width / 2);
            int h = Math.Max(1, src.Height / 2);

            var dst = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (var g = Graphics.FromImage(dst))
            {
                // 속도 우선 세팅
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.Low;
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                g.DrawImage(src, new Rectangle(0, 0, w, h));
            }

            return dst;
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

        private void SetCustomCursor()
        {
            Bitmap bitmap = (Bitmap)global::FreshCheck_CV.Properties.Resources.droppericon_70x70;
            // 48x48 등 원하는 사이즈로 리사이징 후 사용 권장
            IntPtr ptr = bitmap.GetHicon();
            this.Cursor = new Cursor(ptr);
        }

        private void InitFakeCursor()
        {
            _fakeCursor = new PictureBox();

            //string path = Path.Combine(
            //    Application.StartupPath,
            //    "droppericon_70x70.png"
            //);

            //using (var temp = Image.FromFile(path))
            //{
            //    _fakeCursor.Image = new Bitmap(temp);
            //}

            _fakeCursor.Image = global::FreshCheck_CV.Properties.Resources.droppericon_70x70;

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
            
            SetCustomCursor();

            //Cursor.Hide();
            //_fakeCursor.Visible = true;

            // 🔥 핵심: imageViewCtrl에도 이벤트 연결
            //this.MouseMove += CameraForm_MouseMove;
            //imageViewCtrl.MouseMove += CameraForm_MouseMove;

            //this.MouseDown += CameraForm_MouseDown;
            //imageViewCtrl.MouseDown += CameraForm_MouseDown;
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
            //if (imageViewCtrl.TryPickColor(e.Location, out Color pickedColor))
            //{
            //    ColorPicked?.Invoke(this, new ColorPickedEventArgs(pickedColor));
            //}

            //EndFakeCursorPick();
        }

        public void EndFakeCursorPick()
        {
            _isPickingColor = false;

            this.Cursor = Cursors.Default;

            //_fakeCursor.Visible = false;
            //Cursor.Show();

            //this.MouseMove -= CameraForm_MouseMove;
            //imageViewCtrl.MouseMove -= CameraForm_MouseMove;

            //this.MouseDown -= CameraForm_MouseDown;
            //imageViewCtrl.MouseDown -= CameraForm_MouseDown;
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



        //#17_WORKING_STATE#5 작업 상태 화면 표시 설정
        public void SetWorkingState(WorkingState workingState)
        {
            string state = "";
            switch (workingState)
            {
                case WorkingState.INSPECT:
                    state = "INSPECT";
                    break;

                case WorkingState.LIVE:
                    state = "LIVE";
                    break;

                case WorkingState.CYCLE:
                    state = "CYCLE";
                    break;

                case WorkingState.ALARM:
                    state = "ALARM";
                    break;
            }

            imageViewCtrl.WorkingState = state;
            imageViewCtrl.Invalidate();
        }

    }
}
