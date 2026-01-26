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
            HookEvents();
            HookCameraPickEvent();
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

    }
}