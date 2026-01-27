using FreshCheck_CV.Core;
using FreshCheck_CV.Models;
using FreshCheck_CV.UIControl;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FreshCheck_CV.Property
{
    public sealed class InspectionMonitorProp : UserControl
    {

        private OverallRateBarControl _overallBars;

        private readonly Timer _timer;

        private Label _lblRun;
        private Label _lblItemsPerMin;

        private Label _lblTotal;
        private Label _lblOk;
        private Label _lblMold;
        private Label _lblScratch;

        private Label _lblRecentOk;
        private Label _lblRecentMold;
        private Label _lblRecentScratch;

        private Panel _pnlAlert;
        private Label _lblAlert;

        private TrendChartControl _trendChart;

        public InspectionMonitorProp()
        {
            BuildUi();

            _timer = new Timer();
            _timer.Interval = 250;
            _timer.Tick += (s, e) => RefreshSnapshot();
            _timer.Start();
        }

        private void BuildUi()
        {
            BackColor = Color.FromArgb(30, 34, 40);
            ForeColor = Color.White;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ForeColor = ForeColor,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(10),
                AutoScroll = true
            };

            root.RowStyles.Clear();
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));    // 헤더
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));   // 누적(여유 있게)
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));    // 최근 3줄(OK/Mold/Scratch)
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));    // 차트
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // Alert

            // 0) 헤더
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor
            };

            _lblRun = MakeBadge("STOP");
            _lblRun.Location = new Point(0, 7);

            _lblItemsPerMin = new Label
            {
                AutoSize = false,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 12f, FontStyle.Regular),
                Text = "Items/min: 0",
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Right,
                Width = 150
            };

            header.Controls.Add(_lblItemsPerMin);
            header.Controls.Add(_lblRun);

            // 1) 누적(큰 글씨)
            var totalPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Padding = new Padding(8),
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblTotal = MakeBig("Total: 0", Color.White, 22f);
            _lblOk = MakeBig("OK: 0", Color.FromArgb(120, 200, 80), 20f);
            _lblMold = MakeBig("Mold: 0", Color.FromArgb(220, 60, 60), 20f);
            _lblScratch = MakeBig("Scratch: 0", Color.FromArgb(240, 160, 60), 20f);

            _lblTotal.Dock = DockStyle.Top;
            _lblOk.Dock = DockStyle.Top;
            _lblMold.Dock = DockStyle.Top;
            _lblScratch.Dock = DockStyle.Top;

            totalPanel.Controls.Clear();
            totalPanel.Controls.Add(_lblScratch);
            totalPanel.Controls.Add(_lblMold);
            totalPanel.Controls.Add(_lblOk);
            totalPanel.Controls.Add(_lblTotal);

            // 최근 수치 (최근 50개 대비: OK/Mold/Scratch %)
            var recentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                Padding = new Padding(0, 2, 0, 2)
            };

            _lblRecentOk = MakeMid("최근 50개 OK: 0%", Color.FromArgb(120, 200, 80));
            _lblRecentMold = MakeMid("최근 50개 Mold: 0%", Color.FromArgb(220, 60, 60));
            _lblRecentScratch = MakeMid("최근 50개 Scratch: 0%", Color.FromArgb(240, 160, 60));

            ApplyEllipsis(_lblRecentOk);
            ApplyEllipsis(_lblRecentMold);
            ApplyEllipsis(_lblRecentScratch);

            _lblRecentScratch.Dock = DockStyle.Top;
            _lblRecentMold.Dock = DockStyle.Top;
            _lblRecentOk.Dock = DockStyle.Top;

            // Top 쌓는 경우 역순 Add
            recentPanel.Controls.Add(_lblRecentScratch);
            recentPanel.Controls.Add(_lblRecentMold);
            recentPanel.Controls.Add(_lblRecentOk);

            var chartWrapper = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 6, 0, 6)
            };

            // 위쪽 최근
            // 아래 전체
            chartWrapper.RowStyles.Clear();
            chartWrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            chartWrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            _trendChart = new TrendChartControl
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                MinimumSize = new Size(0, 110)
            };

            _overallBars = new OverallRateBarControl
            {
                Dock = DockStyle.Fill,
                BackColor = BackColor,
                MinimumSize = new Size(0, 80)
            };

            chartWrapper.Controls.Add(_trendChart, 0, 0);
            chartWrapper.Controls.Add(_overallBars, 0, 1);

            chartWrapper.Controls.Add(_trendChart);

            // 4) Alert
            _pnlAlert = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 44, 52),
                Visible = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0, 6, 0, 0)
            };

            _lblAlert = new Label
            {
                AutoSize = false,
                Height = 42,
                Dock = DockStyle.Top,
                ForeColor = Color.FromArgb(240, 200, 80),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                Text = ""
            };
            ApplyEllipsis(_lblAlert);

            _pnlAlert.Controls.Add(_lblAlert);

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(totalPanel, 0, 1);
            root.Controls.Add(recentPanel, 0, 2);
            root.Controls.Add(chartWrapper, 0, 3);
            root.Controls.Add(_pnlAlert, 0, 4);

            Controls.Add(root);
        }

        private void RefreshSnapshot()
        {
            var hub = Core.Global.Inst?.InspStage?.Hub;
            if (hub == null) return;

            InspectionSnapshot s = hub.GetSnapshot();

            _lblRun.Text = s.IsRunning ? "RUN" : "STOP";
            _lblRun.BackColor = s.IsRunning
                ? Color.FromArgb(120, 200, 80)
                : Color.FromArgb(140, 140, 140);

            _lblItemsPerMin.Text = $"Items/min: {(int)Math.Round(s.ItemsPerMin)}";

            _lblTotal.Text = $"Total: {s.Total}";
            _lblOk.Text = $"OK: {s.Ok}";
            _lblMold.Text = $"Mold: {s.Mold}";
            _lblScratch.Text = $"Scratch: {s.Scratch}";

            // ★ 최근 50개 대비 퍼센트
            _lblRecentOk.Text = $"■ 최근 50개 OK: {(int)Math.Round(s.RecentOkRate * 100)}%";
            _lblRecentMold.Text = $"■ 최근 50개 Mold: {(int)Math.Round(s.RecentMoldRate * 100)}%";
            _lblRecentScratch.Text = $"■ 최근 50개 Scratch: {(int)Math.Round(s.RecentScratchRate * 100)}%";

            _trendChart.SetPoints(s.TrendPoints);
            _overallBars?.SetRates(s.OverallOkRate, s.OverallMoldRate, s.OverallScratchRate);

            bool hasAlert = !string.IsNullOrWhiteSpace(s.AlertText);
            _pnlAlert.Visible = hasAlert;
            _lblAlert.Text = hasAlert ? s.AlertText : "";
        }

        private static void ApplyEllipsis(Label label)
        {
            if (label == null) return;
            label.AutoEllipsis = true;
        }

        private static Label MakeBadge(string text)
        {
            return new Label
            {
                AutoSize = false,
                Size = new Size(90, 30),
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(140, 140, 140),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold)
            };
        }
        private static Label MakeBig(string text, Color color, float size)
        {
            return new Label
            {
                AutoSize = false,
                Height = (int)Math.Ceiling(size * 2.2) + 2, // 하단 잘림 방지
                Text = text,
                ForeColor = color,
                Font = new Font("Segoe UI", size, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 2, 0, 2)
            };
        }

        private static Label MakeMid(string text, Color color)
        {
            return new Label
            {
                AutoSize = false,
                Height = 26,
                Text = text,
                ForeColor = color,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 1, 0, 1)
            };
        }
    }
}
