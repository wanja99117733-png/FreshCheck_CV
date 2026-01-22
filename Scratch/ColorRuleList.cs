using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FreshCheck_CV.Scratch
{
    public partial class ColorRuleList : UserControl
    {
        private readonly List<CucumberColorRule> _rules = new List<CucumberColorRule>();
        private int _selectedIndex = -1;

        private FlowLayoutPanel _panel;

        // 선택 컬러 프리뷰 UI
        private Panel _pnlPreview;
        private Label _lblInfo;

        public ColorRuleList()
        {
            InitializeComponent();
            BuildUi();
        }

        public event EventHandler RulesChanged;          // 룰 변경(추가/삭제/토글 등)
        public event EventHandler SelectedRuleChanged;   // 선택 변경

        public List<CucumberColorRule> Rules => _rules;

        public int SelectedIndex => _selectedIndex;

        public CucumberColorRule SelectedRule
        {
            get
            {
                if (_selectedIndex < 0 || _selectedIndex >= _rules.Count)
                    return null;

                return _rules[_selectedIndex];
            }
        }

        public void AddRule(CucumberColorRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _rules.Add(rule);
            RebuildUi();

            SelectIndex(_rules.Count - 1, raiseSelectedEvent: true);
            RaiseRulesChanged();
        }

        public void RemoveSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _rules.Count)
                return;

            _rules.RemoveAt(_selectedIndex);
            _selectedIndex = -1;

            RebuildUi();
            UpdatePreview();

            RaiseSelectedChanged();
            RaiseRulesChanged(); // 삭제 후 재계산/하이라이트 트리거
        }

        private void BuildUi()
        {
            // 상단 프리뷰 영역
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 34;

            _pnlPreview = new Panel();
            _pnlPreview.Width = 44;
            _pnlPreview.Height = 22;
            _pnlPreview.Left = 8;
            _pnlPreview.Top = 6;
            _pnlPreview.BorderStyle = BorderStyle.FixedSingle;
            _pnlPreview.BackColor = Color.Transparent;

            _lblInfo = new Label();
            _lblInfo.AutoSize = false;
            _lblInfo.Left = _pnlPreview.Right + 8;
            _lblInfo.Top = 8;
            _lblInfo.Width = 260;
            _lblInfo.Height = 18;
            _lblInfo.Text = "선택된 컬러 없음";

            header.Controls.Add(_pnlPreview);
            header.Controls.Add(_lblInfo);

            _panel = new FlowLayoutPanel();
            _panel.Dock = DockStyle.Fill;
            _panel.WrapContents = true;
            _panel.AutoScroll = true;

            Controls.Add(_panel);
            Controls.Add(header);
        }

        private void RebuildUi()
        {
            _panel.Controls.Clear();

            for (int i = 0; i < _rules.Count; i++)
            {
                CucumberColorRule rule = _rules[i];

                Panel item = new Panel();
                item.Width = 46;
                item.Height = 28;
                item.Margin = new Padding(6);
                item.Tag = i;
                item.Cursor = Cursors.Hand;
                item.BackColor = rule.SeedColor;
                item.BorderStyle = BorderStyle.FixedSingle;

                ToolTip tip = new ToolTip();
                tip.SetToolTip(item, rule.ToString());

                item.Click += Item_Click;
                item.MouseUp += Item_MouseUp;

                _panel.Controls.Add(item);
            }

            UpdateSelectionUi();
            UpdatePreview();
        }

        private void Item_Click(object sender, EventArgs e)
        {
            Panel p = sender as Panel;
            if (p == null)
                return;

            int idx = (int)p.Tag;
            SelectIndex(idx, raiseSelectedEvent: true);
        }

        private void Item_MouseUp(object sender, MouseEventArgs e)
        {
            // 우클릭: 포함/제외 토글은 유지하되, UI에 + 표시 안함
            if (e.Button != MouseButtons.Right)
                return;

            Panel p = sender as Panel;
            if (p == null)
                return;

            int idx = (int)p.Tag;
            if (idx < 0 || idx >= _rules.Count)
                return;

            _rules[idx].IsExclude = !_rules[idx].IsExclude;

            // 룰 상태가 바뀌었으니 재계산/하이라이트 필요
            RebuildUi();
            RaiseRulesChanged();
        }

        private void SelectIndex(int idx, bool raiseSelectedEvent)
        {
            _selectedIndex = idx;
            UpdateSelectionUi();
            UpdatePreview();

            if (raiseSelectedEvent)
                RaiseSelectedChanged();

            // 선택만 바뀐 경우는 굳이 RulesChanged는 안 쏩니다.
            // (선택 변경만으로는 마스크가 바뀌지 않으니까)
        }

        private void UpdateSelectionUi()
        {
            for (int i = 0; i < _panel.Controls.Count; i++)
            {
                Panel p = _panel.Controls[i] as Panel;
                if (p == null)
                    continue;

                int idx = (int)p.Tag;

                // 선택 강조: Padding으로 두껍게 보이게
                p.Padding = idx == _selectedIndex ? new Padding(2) : new Padding(0);
            }
        }

        private void UpdatePreview()
        {
            CucumberColorRule rule = SelectedRule;
            if (rule == null)
            {
                _pnlPreview.BackColor = Color.Transparent;
                _lblInfo.Text = "선택된 컬러 없음";
                return;
            }

            Color c = rule.SeedColor;
            _pnlPreview.BackColor = c;

            // 필요하면 HSV도 표시 가능(지금은 RGB만)
            _lblInfo.Text = $"RGB({c.R}, {c.G}, {c.B})  {(rule.IsExclude ? "[제외]" : "[포함]")}";
        }

        private void RaiseRulesChanged()
        {
            RulesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseSelectedChanged()
        {
            SelectedRuleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
