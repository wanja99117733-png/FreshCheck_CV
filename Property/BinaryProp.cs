using FreshCheck_CV.Core;
using FreshCheck_CV.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FreshCheck_CV.CameraForm;

namespace FreshCheck_CV.Property
{
    public partial class BinaryProp : UserControl
    {
        private readonly BinaryOptions _options = new BinaryOptions();
        private bool _isCameraSubscribed = false;

        public BinaryProp()
        {
            InitializeComponent();
            InitUi();

            ApplyDarkButton(btnPickColor);
            ApplyDarkButton(btnApply);
            ApplyDarkButton(btnRunMold);

            HookEvents();
            HookCameraPickEvent();
            ApplyLayoutFixes();
        }

        private void ApplyLayoutFixes()
        {
            // ---------------------------
            // 1) 타깃 영역: 라벨 안 잘리게
            // ---------------------------
            // 컬럼 구성: [Swatch(고정)] [Label(남는폭)] [Button(내용만큼)]
            if (tlpTarget.ColumnStyles.Count >= 3)
            {
                tlpTarget.ColumnStyles[0].SizeType = SizeType.Absolute;
                tlpTarget.ColumnStyles[0].Width = 29f;

                tlpTarget.ColumnStyles[1].SizeType = SizeType.Percent;
                tlpTarget.ColumnStyles[1].Width = 100f;

                // ★ 핵심: 버튼 컬럼을 Absolute 고정값(160) -> AutoSize로
                tlpTarget.ColumnStyles[2].SizeType = SizeType.AutoSize;
            }

            // Row 높이도 고정 36이면 내부 컨트롤이 클리핑될 수 있으니 넉넉히
            tlpTarget.RowStyles.Clear();
            tlpTarget.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
            tlpTarget.Height = 44;

            // 버튼은 “내용만큼 + 최소 폭”으로, 셀에 맞게 채우기
            btnPickColor.AutoSize = true;
            btnPickColor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPickColor.Dock = DockStyle.Fill;
            btnPickColor.MinimumSize = new System.Drawing.Size(110, 34); // "스포이드" 안 잘리는 최소 폭
            btnPickColor.Margin = new Padding(6, 4, 0, 4);
            btnPickColor.Padding = new Padding(10, 0, 10, 0);

            // 라벨은 남는 폭을 전부 먹고, 폭이 부족하면 … 대신 줄바꿈/정렬로 처리
            lblTargetColor.AutoSize = false;
            lblTargetColor.Dock = DockStyle.Fill;
            lblTargetColor.Margin = new Padding(0, 8, 6, 0);
            lblTargetColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblTargetColor.AutoEllipsis = true; // 폭이 극단적으로 좁을 때도 깔끔

            // ---------------------------
            // 2) 허용오차 라벨: 절대 안 잘리게 (2줄로)
            // ---------------------------
            // 라벨을 2줄로 쓰도록 Row 높이를 확보
            if (tlpTol.RowStyles.Count >= 3)
            {
                tlpTol.RowStyles.Clear();
                tlpTol.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f)); // lblTolerance (2줄)
                tlpTol.RowStyles.Add(new RowStyle(SizeType.Absolute, 80f)); // rangeTrackbar
                tlpTol.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f)); // chkAutoApply
            }

            lblTolerance.AutoSize = false;
            lblTolerance.Dock = DockStyle.Fill;
            lblTolerance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblTolerance.AutoEllipsis = false; // 2줄이라 필요 없음
            lblTolerance.Margin = new Padding(0, 0, 0, 4);

            // RangeTrackbar / 체크박스는 고정 Row에 맞춰 Fill
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

            // 기본값
            rangeTrackbar.ValueLeft = 80;   // - 오차
            rangeTrackbar.ValueRight = 120; // + 오차

            chkInvert.Checked = false;
            chkAutoApply.Checked = false;

            // 초기 UI
            UpdateToleranceLabel();
            UpdateTargetUi(0, 0, 0);
        }

        private void HookEvents()
        {
            btnApply.Click += (s, e) => ApplyFromUi();
            btnPickColor.Click += (s, e) => BeginPickColor();

            // 트랙바 값 변경 시 라벨 갱신 + (옵션) 자동 적용
            rangeTrackbar.RangeChanged += (s, e) =>
            {
                UpdateToleranceLabel();
                if (chkAutoApply.Checked) ApplyFromUi();
            };

            // 보기 모드 / 반전 변경 시 (옵션) 자동 적용
            cbMode.SelectedIndexChanged += (s, e) =>
            {
                if (chkAutoApply.Checked) ApplyFromUi();
            };

            chkInvert.CheckedChanged += (s, e) =>
            {
                if (chkAutoApply.Checked) ApplyFromUi();
            };

            chkAutoApply.CheckedChanged += (s, e) =>
            {
                // 켜는 순간 한 번 적용해주는 게 UX 좋음
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

        private void BeginPickColor()
        {
            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm == null)
            {
                return;
            }

            cameraForm.BeginPickColor();
        }

        private void CameraForm_ColorPicked(object sender, ColorPickedEventArgs e)
        {
            Color c = e.Color;

            _options.TargetB = c.B;
            _options.TargetG = c.G;
            _options.TargetR = c.R;

            UpdateTargetUi(_options.TargetB, _options.TargetG, _options.TargetR);

            // 픽킹 후 반영
            ApplyFromUi();
        }

        private void UpdateTargetUi(int b, int g, int r)
        {
            lblTargetColor.Text = $"Target: (B={b}, G={g}, R={r})";
            pnlTargetSwatch.BackColor = Color.FromArgb(r, g, b); // 표시용 Color는 RGB
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

            _options.Invert = chkInvert.Checked;

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
        private void ApplyDarkButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 90);

            btn.BackColor = Color.FromArgb(70, 70, 70);
            btn.ForeColor = Color.White;

            btn.UseVisualStyleBackColor = false;
        }

    }
}