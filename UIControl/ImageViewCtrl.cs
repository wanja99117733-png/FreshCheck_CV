using FreshCheck_CV.Inspect;
using SaigeVision.Net.Core.V2;
using SaigeVision.Net.V2;
using SaigeVision.Net.V2.Segmentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Windows.Forms;



namespace FreshCheck_CV.UIControl
{

    public partial class ImageViewCtrl : UserControl
    {
        private readonly object _imgLock = new object();
        //#4_IMAGE_VIEWER#1 변수 선언
        private bool _isInitialized = false;

        // 현재 로드된 이미지
        private Bitmap _bitmapImage = null;
        private static Bitmap SafeClone(Bitmap src)
        {
            if (src == null) return null;

            try
            {
                // Clone이 가장 빠르고 픽셀포맷도 유지됨
                return (Bitmap)src.Clone();
            }
            catch
            {
                // src가 이미 Dispose된 상태면 여기로 들어올 수 있음
                return null;
            }
        }

        // 프리뷰 이미지 변수
        private Bitmap _previewImage = null;

        public Bitmap PreviewImage
        {
            get
            {
                lock (_imgLock)
                {
                    return SafeClone(_previewImage);
                }
            }
            set
            {
                Bitmap clone = SafeClone(value); // ✅ 들어오는 순간 복제

                lock (_imgLock)
                {
                    if (_previewImage != null)
                    {
                        _previewImage.Dispose();
                        _previewImage = null;
                    }

                    _previewImage = clone;
                }

                Invalidate();
            }
        }

        private Bitmap _resultImage = null;

        public Bitmap ResultImage
        {
            get
            {
                lock (_imgLock)
                {
                    return SafeClone(_resultImage);
                }
            }
            set
            {
                Bitmap clone = SafeClone(value); // ✅ 들어오는 순간 복제

                lock (_imgLock)
                {
                    if (_resultImage != null)
                    {
                        _resultImage.Dispose();
                        _resultImage = null;
                    }

                    _resultImage = clone;
                }

                Invalidate();
            }
        }


        // 더블 버퍼링을 위한 캔버스
        // 더블버퍼링 : 화면 깜빡임을 방지하고 부드러운 펜더링위해 사용
        private Bitmap Canvas = null;

        // 화면에 표시될 이미지의 크기 및 위치
        // 부동 소수점(float) 좌표를 사용하는 사각형 구조체
        private RectangleF ImageRect = new RectangleF(0, 0, 0, 0);

        // 현재 줌 배율
        private float _curZoom = 1.0f;
        // 줌 배율 변경 시, 확대/축소 단위
        private float _zoomFactor = 1.1f;

        // 최소 및 최대 줌 제한 값
        private float MinZoom = 1.0f;
        private const float MaxZoom = 100.0f;

        // 스크래치 세그멘테이션 결과 저장용
        private SegmentationResult _scratchResult = null;


        //#17_WORKING_STATE#3 작업 상태 변수
        public string WorkingState { get; set; } = "";


        public ImageViewCtrl()
        {
            InitializeComponent();
            InitializeCanvas();

            MouseWheel += new MouseEventHandler(ImageViewCCtrl_MouseWheel);
        }


        //#4_IMAGE_VIEWER#2 캔버스 초기화 및 설정
        private void InitializeCanvas()
        {
            // 캔버스를 UserControl 크기만큼 생성
            ResizeCanvas();

            // 화면 깜빡임을 방지하기 위한 더블 버퍼링 설정
            DoubleBuffered = true;
        }

        //줌에 따른 좌표 계산 기능 수정 
        private void ResizeCanvas()
        {
            if (Width <= 0 || Height <= 0 || _bitmapImage == null)
                return;

            // 캔버스를 UserControl 크기만큼 생성
            Canvas = new Bitmap(Width, Height);
            if (Canvas == null)
                return;

            // 이미지 원본 크기 기준으로 확대/축소 (ZoomFactor 유지)
            float virtualWidth = _bitmapImage.Width * _curZoom;
            float virtualHeight = _bitmapImage.Height * _curZoom;

            float offsetX = virtualWidth < Width ? (Width - virtualWidth) / 2f : 0f;
            float offsetY = virtualHeight < Height ? (Height - virtualHeight) / 2f : 0f;

            ImageRect = new RectangleF(offsetX, offsetY, virtualWidth, virtualHeight);
        }

        //#4_IMAGE_VIEWER#5 이미지 로딩 함수
        public void LoadBitmap(Bitmap bitmap)
        {
            Bitmap clone = SafeClone(bitmap);
            if (clone == null) return;

            lock (_imgLock)
            {
                if (_bitmapImage != null)
                {
                    _bitmapImage.Dispose();
                    _bitmapImage = null;
                }

                _bitmapImage = clone;
            }

            Invalidate();
        }

        private void FitImageToScreen()
        {
            RecalcZoomRatio();

            float NewWidth = _bitmapImage.Width * _curZoom;

            float NewHeight = _bitmapImage.Height * _curZoom;

            // 이미지가 UserControl 중앙에 배치되도록 정렬
            ImageRect = new RectangleF(
                (Width - NewWidth) / 2, // UserControl 너비에서 이미지 너비를 뺀 후, 절반을 왼쪽 여백으로 설정하여 중앙 정렬
                (Height - NewHeight) / 2,
                NewWidth,
                NewHeight
            );

            Invalidate();
        }
        // 스크래치 결과와 함께 Preview 설정
        public void SetPreviewWithScratch(Bitmap previewImage, SegmentationResult scratchResult)
        {
            _resultImage = previewImage?.Clone() as Bitmap;  // 강제 PreviewImage 설정!
            _scratchResult = scratchResult;

            // 🔥 Preview 전용 줌 리셋
            if (_resultImage != null)
            {
                _bitmapImage = _resultImage.Clone() as Bitmap;  // _bitmapImage도 Preview로!
                FitImageToScreen();  // 크기 맞춤
            }

            Invalidate();
        }

        // 스크래치 결과만 클리어
        public void ClearScratchResult()
        {
            _scratchResult = null;
            Invalidate();
        }


        //#GROUP ROI#7 현재 이미지를 기준으로 줌 비율 재계산
        private void RecalcZoomRatio()
        {
            if (_bitmapImage == null || Width <= 0 || Height <= 0)
                return;

            Size imageSize = new Size(_bitmapImage.Width, _bitmapImage.Height);

            float aspectRatio = (float)imageSize.Height / (float)imageSize.Width;
            float clientAspect = (float)Height / (float)Width;

            //UserControl과 이미지의 비율의 관계를 통해, 이미지가 UserControl안에 들어가도록 Zoom비율 설정
            float ratio;
            if (aspectRatio <= clientAspect)
                ratio = (float)Width / (float)imageSize.Width;
            else
                ratio = (float)Height / (float)imageSize.Height;

            //최소 줌 비율은 이미지가 UserControl에 꽉차게 들어가는 것으로 설정
            float minZoom = ratio;

            // MinZoom 및 줌 적용
            MinZoom = minZoom;

            _curZoom = Math.Max(MinZoom, Math.Min(MaxZoom, ratio));

            Invalidate();
        }

        //#4_IMAGE_VIEWER#3
        // Windows Forms에서 컨트롤이 다시 그려질 때 자동으로 호출되는 메서드
        // 화면새로고침(Invalidate()), 창 크기변경, 컨트롤이 숨겨졌다가 나타날때 실행
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);

                // ✅ 표시 우선순위: Result > Preview > Original
                Bitmap src = _resultImage ?? _previewImage ?? _bitmapImage;
                if (src == null)
                    return;

                // ✅ Canvas가 없거나 크기가 바뀌었으면 재생성 (Dispose/Resize 방어)
                int cw = Math.Max(1, this.Width);
                int ch = Math.Max(1, this.Height);

                if (Canvas == null || Canvas.Width != cw || Canvas.Height != ch)
                {
                    Canvas?.Dispose();
                    Canvas = new Bitmap(cw, ch, PixelFormat.Format32bppPArgb);
                }

                // ✅ 그리기 직전에 Clone해서 "그리는 중 Dispose" 레이스 끊기
                Bitmap drawBmp = null;
                try
                {
                    drawBmp = (Bitmap)src.Clone();
                }
                catch
                {
                    // src가 이미 Dispose되었거나 GDI+ 내부 상태가 깨진 경우
                    return;
                }

                try
                {
                    using (drawBmp)
                    using (Graphics g = Graphics.FromImage(Canvas))
                    {
                        g.Clear(Color.Transparent);
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.SmoothingMode = SmoothingMode.None;
                        g.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                        // ✅ 선택된 표시 비트맵(drawBmp) 기준으로 ImageRect 재계산
                        float virtualWidth = drawBmp.Width * _curZoom;
                        float virtualHeight = drawBmp.Height * _curZoom;

                        ImageRect = new RectangleF(
                            (Width - virtualWidth) / 2f,
                            (Height - virtualHeight) / 2f,
                            virtualWidth,
                            virtualHeight);

                        // ✅ 실제 그리기
                        g.DrawImage(drawBmp, ImageRect);

                        // ✅ 스크래치 박스
                        DrawScratchBoundingBoxes(g);

                        // ✅ 작업 상태 텍스트
                        if (string.IsNullOrEmpty(WorkingState) == false && (_resultImage != null || _previewImage != null))
                        {
                            float fontSize = 20.0f;
                            Color stateColor = Color.FromArgb(255, 128, 0);
                            PointF textPos = new PointF(10, 10);
                            DrawText(g, WorkingState, textPos, fontSize, stateColor);
                        }
                    }


                    // ✅ Canvas를 화면에 출력
                    e.Graphics.DrawImageUnscaled(Canvas, 0, 0);
                }
                catch (ArgumentException)
                {
                    // GDI+ "Parameter is not valid" 방어
                }
                catch (ObjectDisposedException)
                {
                    // Canvas나 Graphics가 Dispose된 타이밍 방어
                }
            }
            catch (ArgumentException)
            {
                // base.OnPaint 포함해서 혹시라도 올라오면 최종 방어
            }
            catch (ObjectDisposedException)
            {
            }
        }



        private void DrawText(Graphics g, string text, PointF position, float fontSize, Color color)
        {
            using (Font font = new Font("Segoe UI", fontSize, FontStyle.Bold))
            // 테두리용 검정색 브러시
            using (Brush outlineBrush = new SolidBrush(Color.Black))
            // 본문용 노란색 브러시
            using (Brush textBrush = new SolidBrush(color))
            {
                // 테두리 효과를 위해 주변 8방향으로 그리기
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue; // 가운데는 제외
                        PointF borderPos = new PointF(position.X + dx, position.Y + dy);
                        g.DrawString(text, font, outlineBrush, borderPos);
                    }
                }

                // 본문 텍스트
                g.DrawString(text, font, textBrush, position);
            }
        }


        // 사각형 그리기 (줌/스크롤 반영)
        private void DrawScratchBoundingBoxes(Graphics g)
        {
            if (_scratchResult?.SegmentedObjects == null || _scratchResult.SegmentedObjects.Length == 0)
                return;

            using (Pen pen = new Pen(Color.Red, 3))
            using (Font font = new Font("Segoe UI", 10, FontStyle.Bold))
            {
                foreach (var obj in _scratchResult.SegmentedObjects)
                {
                    if (obj == null)
                        continue;

                    // ★ BoundingBox 대신 Contour 사용 (안전)
                    var contour = obj.Contour?.Value;
                    if (contour == null || contour.Count < 3)
                        continue;

                    // ★ Contour에서 최소 바운딩 박스 계산
                    float minX = contour.Min(p => p.X), maxX = contour.Max(p => p.X);
                    float minY = contour.Min(p => p.Y), maxY = contour.Max(p => p.Y);

                    RectangleF boundingRect = new RectangleF(minX, minY, maxX - minX, maxY - minY);

                    // 화면 좌표 변환
                    RectangleF screenRect = VirtualToScreen(boundingRect);

                    if (screenRect.Width > 0 && screenRect.Height > 0)
                    {
                        // 사각형 그리기
                        g.DrawRectangle(pen, screenRect.X, screenRect.Y, screenRect.Width, screenRect.Height);

                    }
                }
            }
        }




        private RectangleF VirtualToScreen(RectangleF imgRect)
        {
            // 이미 구현된 Rectangle 버전의 로직과 동일하게 작동하도록 구현합니다.
            PointF offset = GetScreenOffset();

            return new RectangleF(
                (imgRect.X * _curZoom) + offset.X,
                (imgRect.Y * _curZoom) + offset.Y,
                imgRect.Width * _curZoom,
                imgRect.Height * _curZoom
            );
        }

        //#4_IMAGE_VIEWER#4 마우스휠을 이용한 확대/축소
        private void ImageViewCCtrl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                ZoomMove(_curZoom / _zoomFactor, e.Location);
            else
                ZoomMove(_curZoom * _zoomFactor, e.Location);

            // 새로운 이미지 위치 반영 (점진적으로 초기 상태로 회귀)
            if (_bitmapImage != null)
            {
                ImageRect.Width = _bitmapImage.Width * _curZoom;
                ImageRect.Height = _bitmapImage.Height * _curZoom;
            }

            // 다시 그리기 요청
            Invalidate();
        }

        //휠에 의해, Zoom 확대/축소 값 계산
        private void ZoomMove(float zoom, Point zoomOrigin)
        {
            PointF virtualOrigin = ScreenToVirtual(new PointF(zoomOrigin.X, zoomOrigin.Y));

            _curZoom = Math.Max(MinZoom, Math.Min(MaxZoom, zoom));
            if (_curZoom <= MinZoom)
                return;

            PointF zoomedOrigin = VirtualToScreen(virtualOrigin);

            float dx = zoomedOrigin.X - zoomOrigin.X;
            float dy = zoomedOrigin.Y - zoomOrigin.Y;

            ImageRect.X -= dx;
            ImageRect.Y -= dy;
        }

        public Bitmap GetCurBitmap()
        {
            return _bitmapImage;
        }

        // Virtual <-> Screen 좌표계 변환
        #region 좌표계 변환
        private PointF GetScreenOffset()
        {
            return new PointF(ImageRect.X, ImageRect.Y);
        }

        private Rectangle ScreenToVirtual(Rectangle screenRect)
        {
            PointF offset = GetScreenOffset();
            return new Rectangle(
                (int)((screenRect.X - offset.X) / _curZoom + 0.5f),
                (int)((screenRect.Y - offset.Y) / _curZoom + 0.5f),
                (int)(screenRect.Width / _curZoom + 0.5f),
                (int)(screenRect.Height / _curZoom + 0.5f));
        }

        private Rectangle VirtualToScreen(Rectangle virtualRect)
        {
            PointF offset = GetScreenOffset();
            return new Rectangle(
                (int)(virtualRect.X * _curZoom + offset.X + 0.5f),
                (int)(virtualRect.Y * _curZoom + offset.Y + 0.5f),
                (int)(virtualRect.Width * _curZoom + 0.5f),
                (int)(virtualRect.Height * _curZoom + 0.5f));
        }

        private PointF ScreenToVirtual(PointF screenPos)
        {
            PointF offset = GetScreenOffset();
            return new PointF(
                (screenPos.X - offset.X) / _curZoom,
                (screenPos.Y - offset.Y) / _curZoom);
        }

        private PointF VirtualToScreen(PointF virtualPos)
        {
            PointF offset = GetScreenOffset();
            return new PointF(
                virtualPos.X * _curZoom + offset.X,
                virtualPos.Y * _curZoom + offset.Y);
        }
        #endregion

        private void ImageViewCtrl_Resize(object sender, EventArgs e)
        {
            ResizeCanvas();
            Invalidate();
        }

        private void ImageViewCtrl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            FitImageToScreen();
        }

        public bool TryPickColor(Point screenPoint, out Color color)
        {
            color = Color.Empty;

            if (_bitmapImage == null)
            {
                return false;
            }

            // 줌/패닝을 반영해 화면좌표 -> 원본좌표 변환
            PointF v = ScreenToVirtual(new PointF(screenPoint.X, screenPoint.Y));

            int x = (int)Math.Round(v.X);
            int y = (int)Math.Round(v.Y);

            if (x < 0 || x >= _bitmapImage.Width || y < 0 || y >= _bitmapImage.Height)
            {
                return false;
            }

            color = _bitmapImage.GetPixel(x, y);
            return true;
        }


        // 이미지 클릭한 픽셀 위치 가져오기
        public bool TryGetImagePoint(System.Drawing.Point screenPoint, out System.Drawing.Point imagePoint)
        {
            imagePoint = System.Drawing.Point.Empty;

            if (_bitmapImage == null)
                return false;

            System.Drawing.PointF virtualPoint = ScreenToVirtual(new System.Drawing.PointF(screenPoint.X, screenPoint.Y));

            int x = (int)(virtualPoint.X + 0.5f);
            int y = (int)(virtualPoint.Y + 0.5f);

            if (x < 0 || y < 0 || x >= _bitmapImage.Width || y >= _bitmapImage.Height)
                return false;

            imagePoint = new System.Drawing.Point(x, y);
            return true;
        }

        public void ClearAllImages()
        {
            lock (_imgLock)
            {
                _bitmapImage?.Dispose(); _bitmapImage = null;
                _previewImage?.Dispose(); _previewImage = null;
                _resultImage?.Dispose(); _resultImage = null;
            }

            Invalidate();
        }

    }
}
