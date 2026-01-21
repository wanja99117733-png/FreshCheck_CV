using FreshCheck_CV.Core;
using FreshCheck_CV.Models;
using FreshCheck_CV.Models.FreshCheck_CV.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            // cbMode를 "표시 모드" 선택으로 사용 (이름은 나중에 바꿔도 됨)
            cbMode.Items.Clear();
            cbMode.Items.Add(ShowBinaryMode.HighlightRed);
            cbMode.Items.Add(ShowBinaryMode.HighlightGreen);
            cbMode.Items.Add(ShowBinaryMode.HighlightBlue);
            cbMode.Items.Add(ShowBinaryMode.BinaryOnly);
            cbMode.Items.Add(ShowBinaryMode.None);
            cbMode.SelectedItem = ShowBinaryMode.HighlightRed;

            // RangeTrackbar는 "허용오차"로 사용
            rangeTrackbar.Minimum = 0;
            rangeTrackbar.Maximum = 255;

            // 요청하신 기본값
            rangeTrackbar.ValueLeft = 80;   // 아래 오차
            rangeTrackbar.ValueRight = 120; // 위 오차

            chkInvert.Checked = false;

            // 초기 표시용 라벨
            lblTargetColor.Text = "Target: (B=0, G=0, R=0)";
        }

        private void HookEvents()
        {
            btnApply.Click += (s, e) => ApplyFromUi();

            // 즉시 적용 원하면 아래 주석 해제 (Scroll 말고 RangeChanged)
            // rangeTrackbar.RangeChanged += (s, e) => ApplyFromUi();
            // chkInvert.CheckedChanged += (s, e) => ApplyFromUi();
            // cbMode.SelectedIndexChanged += (s, e) => ApplyFromUi();

            btnPickColor.Click += (s, e) => BeginPickColor();
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
            // Windows Color는 ARGB지만, OpenCV는 BGR로 쓸 것
            Color c = e.Color;

            _options.TargetB = c.B;
            _options.TargetG = c.G;
            _options.TargetR = c.R;

            lblTargetColor.Text = $"Target: (B={_options.TargetB}, G={_options.TargetG}, R={_options.TargetR})";

            // 픽킹 후 즉시 반영
            ApplyFromUi();
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
    }
}