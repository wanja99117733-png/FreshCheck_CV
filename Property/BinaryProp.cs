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
            ApplyLayoutFixes();
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

            chkInvert.Checked = false;
            chkAutoApply.Checked = false;

            UpdateToleranceLabel();
            UpdateTargetUi(0, 0, 0);
        }

        private void HookEvents()
        {
            btnApply.Click += (s, e) => ApplyFromUi();
            btnPickColor.Click += (s, e) => BeginPickColor();

            
            rangeTrackbar.RangeChanged += (s, e) =>
            {
                UpdateToleranceLabel();
                if (chkAutoApply.Checked) ApplyFromUi();
            };

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