using FreshCheck_CV.Core;
using FreshCheck_CV.Inspect;
using FreshCheck_CV.Models;
using FreshCheck_CV.Properties;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SaigeVision.Net.V2;
using SaigeVision.Net.V2.Segmentation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using static FreshCheck_CV.CameraForm;

namespace FreshCheck_CV.Property
{
    public partial class BinaryProp : UserControl
    {
        private readonly BinaryOptions _options = new BinaryOptions();
        private bool _isCameraSubscribed = false;

        private Cursor _prevCursor;

        public BinaryProp()
        {
            InitializeComponent();
            InitUi();

            HookEvents();
            HookCameraPickEvent();
            ApplyLayoutFixes();
            ////스포이드 버튼 아이콘 + 중앙 정렬
            //btnPickColor.Image = Properties.Resources.icon;

            //// 아이콘 + 텍스트를 하나의 덩어리로 취급
            //btnPickColor.TextImageRelation = TextImageRelation.ImageBeforeText;

            //// 전체 묶음을 버튼 중앙으로
            //btnPickColor.TextAlign = ContentAlignment.MiddleCenter;
            //btnPickColor.ImageAlign = ContentAlignment.MiddleCenter;

            //// 좌우 여백으로 균형 잡기
            //btnPickColor.Padding = new Padding(10, 0, 10, 0);
            //btnPickColor.Image = new Bitmap(Properties.Resources.icon,new Size(24, 24));
        }

        private void ApplyLayoutFixes()
        {
            if (tlpTarget.ColumnStyles.Count >= 3)
            {
                tlpTarget.ColumnStyles[0].SizeType = SizeType.Absolute;
                tlpTarget.ColumnStyles[0].Width = 29f;

                tlpTarget.ColumnStyles[1].SizeType = SizeType.Percent;
                tlpTarget.ColumnStyles[1].Width = 100f;

                tlpTarget.ColumnStyles[2].SizeType = SizeType.AutoSize;
            }

            tlpTarget.RowStyles.Clear();
            tlpTarget.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            tlpTarget.Height = 44;

            btnPickColor.AutoSize = true;
            btnPickColor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPickColor.Dock = DockStyle.Fill;
            btnPickColor.MinimumSize = new System.Drawing.Size(110, 34);
            btnPickColor.Margin = new Padding(6, 4, 0, 4);
            btnPickColor.Padding = new Padding(10, 0, 10, 0);

            lblTargetColor.AutoSize = false;
            lblTargetColor.Dock = DockStyle.Fill;
            lblTargetColor.Margin = new Padding(0, 8, 6, 0);
            lblTargetColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblTargetColor.AutoEllipsis = true;

            if (tlpTol.RowStyles.Count >= 3)
            {
                tlpTol.RowStyles.Clear();
                tlpTol.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
                tlpTol.RowStyles.Add(new RowStyle(SizeType.Absolute, 80f));
                tlpTol.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));
            }

            lblTolerance.AutoSize = false;
            lblTolerance.Dock = DockStyle.Fill;
            lblTolerance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblTolerance.AutoEllipsis = false;
            lblTolerance.Margin = new Padding(0, 0, 0, 4);

            rangeTrackbar.Dock = DockStyle.Fill;
            rangeTrackbar.Margin = new Padding(0, 2, 0, 2);

            chkAutoApply.Dock = DockStyle.Fill;
            chkAutoApply.Margin = new Padding(0, 4, 0, 0);
        }

        private void InitUi()
        {
            cbMode.Items.Clear();
            cbMode.Items.Add(ShowBinaryMode.HighlightRed);
            cbMode.Items.Add(ShowBinaryMode.HighlightGreen);
            cbMode.Items.Add(ShowBinaryMode.HighlightBlue);
            cbMode.Items.Add(ShowBinaryMode.BinaryOnly);
            cbMode.Items.Add(ShowBinaryMode.None);
            cbMode.SelectedItem = ShowBinaryMode.HighlightRed;

            rangeTrackbar.Minimum = 0;
            rangeTrackbar.Maximum = 255;

            rangeTrackbar.ValueLeft = 80;
            rangeTrackbar.ValueRight = 120;


            UpdateToleranceLabel();
            UpdateTargetUi(0, 0, 0);
        }
        private SegmentationResult FilterStemScratches(Bitmap noBgImage, SegmentationResult scratchResult)
        {
            // 1. SDK 명칭 및 리스트 초기화 확인
            if (scratchResult == null || scratchResult.SegmentedObjects == null) return scratchResult;

            using (Mat src = OpenCvSharp.Extensions.BitmapConverter.ToMat(noBgImage))
            using (Mat gray = src.CvtColor(ColorConversionCodes.BGR2GRAY))
            using (Mat binary = gray.Threshold(1, 255, ThresholdTypes.Binary))
            {
                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchy;

                // 2. 외곽선 검출
                Cv2.FindContours(binary, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                if (contours.Length == 0) return scratchResult;

                var mainContour = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
                RotatedRect rect = Cv2.MinAreaRect(mainContour);

                var filteredList = new List<SegmentedObject>();

                foreach (var obj in scratchResult.SegmentedObjects)
                {
                    // [오류 해결] BoundingBox.Center를 사용하여 위치 파악
                    if (obj.BoundingBox != null)
                    {
                        var center = obj.BoundingBox.Center;
                        OpenCvSharp.Point2f scratchCenter = new OpenCvSharp.Point2f((float)center.X, (float)center.Y);

                        // 필터 조건 1: 전체 사각형 기준 양 끝단 영역인가?
                        bool isStemByRect = IsPointInStemArea(scratchCenter, rect, 0.05f);

                        // 필터 조건 2: 곡률 분석 (각도를 140도로 높여 뭉툭한 꼬다리까지 감지)
                        bool isStemByCurvature = IsPointNearSharpCurvature(scratchCenter, mainContour, 60f, 140.0);

                        // 필터 조건 3: 하단 경계 저격 (마지막 하나 남은 꼬다리 대응)
                        // 뭉치 하단 10~15% 영역에 있는 스크래치를 지웁니다.
                        bool isBottomEdge = scratchCenter.Y > (rect.Center.Y + (rect.Size.Height * 0.35f));

                        // 세 조건 중 하나라도 해당하면 꼬다리 오검출로 간주
                        if (!isStemByRect && !isStemByCurvature && !isBottomEdge)
                        {
                            filteredList.Add(obj);
                        }
                    }
                    else
                    {
                        filteredList.Add(obj);
                    }
                }

                // 3. 결과 반환 (InferenceTimeInfo 에러는 null로 해결)
                return new SegmentationResult(
                    filteredList.ToArray(),
                    scratchResult.Masks,
                    scratchResult.Scoremaps,
                    null
                );
            }
        }

        // --- 보조 메서드 1: 곡률 분석 (각도 조절 기능 포함) ---
        private bool IsPointNearSharpCurvature(OpenCvSharp.Point2f scratchPt, OpenCvSharp.Point[] contour, float distanceThreshold, double maxAngle)
        {
            if (contour.Length < 21) return false;

            for (int i = 10; i < contour.Length - 10; i += 2)
            {
                OpenCvSharp.Point p1 = contour[i - 10];
                OpenCvSharp.Point p2 = contour[i];
                OpenCvSharp.Point p3 = contour[i + 10];

                double ux = p1.X - p2.X; double uy = p1.Y - p2.Y;
                double vx = p3.X - p2.X; double vy = p3.Y - p2.Y;
                double dot = ux * vx + uy * vy;
                double magU = Math.Sqrt(ux * ux + uy * uy);
                double magV = Math.Sqrt(vx * vx + vy * vy);

                if (magU < 1 || magV < 1) continue;
                double angle = Math.Acos(dot / (magU * magV)) * (180.0 / Math.PI);

                // 지정된 maxAngle(예: 140도)보다 뾰족하면 끝단으로 인식
                if (angle < maxAngle)
                {
                    double dist = Math.Sqrt(Math.Pow(p2.X - scratchPt.X, 2) + Math.Pow(p2.Y - scratchPt.Y, 2));
                    if (dist < distanceThreshold) return true;
                }
            }
            return false;
        }

        // --- 보조 메서드 2: 사각형 끝단 판별 ---
        private bool IsPointInStemArea(OpenCvSharp.Point2f pt, RotatedRect rect, float ratio)
        {
            OpenCvSharp.Point2f[] vertices = rect.Points();
            float d1 = (float)Math.Sqrt(Math.Pow(vertices[0].X - vertices[1].X, 2) + Math.Pow(vertices[0].Y - vertices[1].Y, 2));
            float d2 = (float)Math.Sqrt(Math.Pow(vertices[1].X - vertices[2].X, 2) + Math.Pow(vertices[1].Y - vertices[2].Y, 2));

            float longSide = Math.Max(d1, d2);
            float threshold = longSide * ratio;

            double distToCenter = Math.Sqrt(Math.Pow(pt.X - rect.Center.X, 2) + Math.Pow(pt.Y - rect.Center.Y, 2));
            return distToCenter > (longSide / 2 - threshold);
        }
        private void HookEvents()
        {
            btnPickColor.Click += (s, e) =>
            {
                BeginPickColor();
            };

            rangeTrackbar.RangeChanged += (s, e) =>
            {
                UpdateToleranceLabel();
                if (chkAutoApply.Checked)
                {
                    ApplyFromUi();
                }
            };

            cbMode.SelectedIndexChanged += (s, e) =>
            {
                if (chkAutoApply.Checked)
                {
                    ApplyFromUi();
                }
            };

            chkAutoApply.CheckedChanged += (s, e) =>
            {
                if (chkAutoApply.Checked) ApplyFromUi();
            };
        }

        private void HookCameraPickEvent()
        {
            if (_isCameraSubscribed)
            {
                return;
            }

            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm == null)
            {
                return;
            }

            cameraForm.ColorPicked += CameraForm_ColorPicked;
            _isCameraSubscribed = true;
        }


        private void UpdateTargetUi(int b, int g, int r)
        {
            lblTargetColor.Text = $"Target: (B={b}, G={g}, R={r})";
            pnlTargetSwatch.BackColor = Color.FromArgb(r, g, b);
        }

        private void UpdateToleranceLabel()
        {
            int low = Math.Min(rangeTrackbar.ValueLeft, rangeTrackbar.ValueRight);
            int high = Math.Max(rangeTrackbar.ValueLeft, rangeTrackbar.ValueRight);
            lblTolerance.Text = $"허용오차: −{low} / +{high}";
        }

        private void ApplyFromUi()
        {
            int left = rangeTrackbar.ValueLeft;
            int right = rangeTrackbar.ValueRight;

            _options.TolLow = Math.Min(left, right);
            _options.TolHigh = Math.Max(left, right);


            if (cbMode.SelectedItem is ShowBinaryMode showMode)
            {
                _options.ShowMode = showMode;
            }

            Global.Inst.InspStage.ApplyBinary(_options);
        }

        private void btnRunMold_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.RunMoldInspectionTemp();

            using (var dlg = new DarkMessageForm("검사 완료"))
            {
                dlg.ShowDialog(this);
            }
        }
        private void BeginPickColor()
        {
            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm == null)
                return;
            cameraForm.BeginPickColor();

        }
        private void CameraForm_ColorPicked(object sender, ColorPickedEventArgs e)
        {
            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm == null)
                return;

            if (_prevCursor != null)
                Cursor.Current = _prevCursor;

            Color c = e.Color;

            _options.TargetB = c.B;
            _options.TargetG = c.G;
            _options.TargetR = c.R;

            UpdateTargetUi(_options.TargetB, _options.TargetG, _options.TargetR);
            ApplyFromUi();

            cameraForm.EndFakeCursorPick();
        }

        private void btnEraseBg_Click(object sender, EventArgs e)
        {
            SaigeAI saigeAI = Global.Inst.InspStage.AIModule;

            if (saigeAI == null)
            {
                MessageBox.Show("AI 모듈이 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap bitmap = Global.Inst.InspStage.GetCurrentImage();
            if (bitmap is null)
            {
                MessageBox.Show("현재 이미지가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 🔥 수정: 어떤 엔진을 사용할지 타입을 명시합니다. (상시 로딩 방식)
            if (saigeAI.InspAIModule(bitmap, AIEngineType.Segmentation))
            {
                Bitmap resultImage = saigeAI.GetResultImage(); // 배경 삭제된 이미지 생성
                Global.Inst.InspStage.UpdatePreview(resultImage);
            }
        }

        private void btnScratchDet_Click(object sender, EventArgs e)
        {
            SaigeAI saigeAI = Global.Inst.InspStage.AIModule;
            if (saigeAI == null)
            {
                MessageBox.Show("AI 모듈 없음", "오류");
                return;
            }

            Bitmap noBgImage = Global.Inst.InspStage.GetPreviewImage();
            Bitmap originalImage = Global.Inst.InspStage.GetCurrentImage();

            if (noBgImage == null)
            {
                MessageBox.Show("먼저 [배경제거] 버튼을 눌러주세요!", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!saigeAI.InspAIModule(noBgImage, AIEngineType.ScratchSegmentation))
            {
                MessageBox.Show("Scratch 검출 실패", "오류");
                return;
            }

            SegmentationResult scratchResult = saigeAI.GetScratchResult();
            SegmentationResult filteredResult = FilterStemScratches(noBgImage, scratchResult);

            Global.Inst.InspStage.UpdatePreviewWithScratch(originalImage, filteredResult);

        }

        // BinaryProp.cs 내부

        public void SetParameters(BinaryOptions options)
        {
            if (options == null) return;

            // 1. 내부 옵션 객체 복사 (참조 연결)
            // _options가 클래스 내부에서 필드로 선언되어 있다고 가정합니다.
            _options.ShowMode = options.ShowMode;
            _options.TargetR = options.TargetR;
            _options.TargetG = options.TargetG;
            _options.TargetB = options.TargetB;
            _options.TolLow = options.TolLow;
            _options.TolHigh = options.TolHigh;

            // 2. UI 컨트롤에 값 적용 (컨트롤 이름은 실제 디자인에 맞게 수정하세요)
            this.Invoke(new Action(() => {
                // 오차 범위 트랙바 (RangeTrackbar 사용 시)
                // rangeTrackbar의 Left/Right 값이 TolLow/TolHigh와 매핑된다고 가정
                rangeTrackbar.ValueLeft = _options.TolLow;
                rangeTrackbar.ValueRight = _options.TolHigh;

                // 선택된 색상 표시용 패널이나 라벨이 있다면 갱신
                pnlTargetSwatch.BackColor = Color.FromArgb(_options.TargetR, _options.TargetG, _options.TargetB);
                lblTargetColor.Text = $"Target: (B={_options.TargetB}, G={_options.TargetG}, R={_options.TargetR})";

                // 하이라이트 모드 라디오 버튼 등
                if (_options.ShowMode != null)
                {
                    ShowBinaryMode binaryMode = (ShowBinaryMode) _options.ShowMode;
                    cbMode.SelectedIndex = (int) binaryMode;
                }

                // UI 갱신 후 즉시 결과 반영을 위해 인스펙션 호출 (선택 사항)
                // btnApply.PerformClick(); 
            }));
        }


    }
}
