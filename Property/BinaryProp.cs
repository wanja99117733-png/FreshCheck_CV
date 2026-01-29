using FreshCheck_CV.Core;
using FreshCheck_CV.Inspect;
using FreshCheck_CV.Models;
using SaigeVision.Net.V2.Segmentation;
using System;
using System.Drawing;
using System.Windows.Forms;
using static FreshCheck_CV.CameraForm;
using FreshCheck_CV.Properties;


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

            // 🔥 가짜 커서 방식 시작
            cameraForm.StartFakeCursorPick();

        }
        private void CameraForm_ColorPicked(object sender, ColorPickedEventArgs e)
        {
            if (_prevCursor != null)
                Cursor.Current = _prevCursor;

            Color c = e.Color;

            _options.TargetB = c.B;
            _options.TargetG = c.G;
            _options.TargetR = c.R;

            UpdateTargetUi(_options.TargetB, _options.TargetG, _options.TargetR);
            ApplyFromUi();
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
            if (saigeAI == null) return;

            // 1. 검사 대상: 배경제거된 이미지 (배경이 검정색이어야 스크래치 집중도가 높음)
            Bitmap noBgImage = Global.Inst.InspStage.GetPreviewImage();

            // 2. 출력 대상: 원본 이미지 (사용자가 보기 편하도록)
            Bitmap originalImage = Global.Inst.InspStage.GetCurrentImage();

            if (noBgImage == null)
            {
                MessageBox.Show("먼저 [배경제거] 버튼을 눌러주세요!", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 🔥 수정: 스크래치 전용 엔진으로 검사 수행
            if (!saigeAI.InspAIModule(noBgImage, AIEngineType.ScratchSegmentation))
            {
                MessageBox.Show("Scratch 검출 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SegmentationResult scratchResult = saigeAI.GetScratchResult();

            // 3. 원본 이미지(originalImage) 위에 검출된 결과(scratchResult)의 사각형을 그림
            Global.Inst.InspStage.UpdatePreviewWithScratch(originalImage, scratchResult);
        }

    }
}
