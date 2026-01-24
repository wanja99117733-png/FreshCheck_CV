using FreshCheck_CV.Models;
using FreshCheck_CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FreshCheck_CV
{

    public partial class ResultForm : DockContent
    {
        private readonly List<ResultRecord> _allRecords = new List<ResultRecord>();
        private readonly BindingList<ResultRecord> _viewRecords = new BindingList<ResultRecord>();
        private int _seq = 0;

        public ResultForm()
        {
            InitializeComponent();

            ApplyDarkThemeToGrid(dgvResults);

            ApplyDarkThemeToTabs(tabMain);

            ApplyDarkButton(btnClear);

            ApplyDarkButton(btnExportCsv);

            ApplyDarkComboBox(cbResultFilter);


            BackColor = Color.FromArgb(30, 34, 40);

            // Logs 탭을 유지하는 경우 listBoxLogs 스타일
            if (listBoxLogs != null)
            {
                listBoxLogs.BackColor = Color.FromArgb(30, 34, 40);
                listBoxLogs.ForeColor = Color.Gainsboro;

                FormClosed += ResultForm_FormClosed;
                SLogger.LogUpdated += OnLogUpdated;
            }

            // 결과 테이블 바인딩
            if (dgvResults != null)
            {
                dgvResults.AutoGenerateColumns = true;
                dgvResults.DataSource = _viewRecords;

                dgvResults.DataBindingComplete += (s, e) => ApplyGridColumnsLayout(dgvResults);

                dgvResults.CellDoubleClick += DgvResults_CellDoubleClick;
            }

            if (cbResultFilter != null)
            {
                cbResultFilter.Items.Clear();
                cbResultFilter.Items.Add("ALL");
                cbResultFilter.Items.Add("OK");
                cbResultFilter.Items.Add("NG");
                cbResultFilter.SelectedIndex = 0;
                cbResultFilter.SelectedIndexChanged += (s, e) => ApplyFilter();
            }

            if (btnClear != null)
            {
                btnClear.Click += (s, e) => ClearResults();
            }
        }

        public void AddRecord(ResultRecord record)
        {
            if (record == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AddRecord(record)));
                return;
            }

            _seq++;
            record.No = _seq;

            _allRecords.Add(record);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string filter = cbResultFilter?.SelectedItem as string ?? "ALL";

            IEnumerable<ResultRecord> q = _allRecords;

            if (filter == "OK")
                q = q.Where(x => string.Equals(x.Result, "OK", StringComparison.OrdinalIgnoreCase));
            else if (filter == "NG")
                q = q.Where(x => string.Equals(x.Result, "NG", StringComparison.OrdinalIgnoreCase));

            _viewRecords.RaiseListChangedEvents = false;
            _viewRecords.Clear();

            foreach (ResultRecord r in q)
            {
                _viewRecords.Add(r);
            }

            _viewRecords.RaiseListChangedEvents = true;
            _viewRecords.ResetBindings();
        }

        private void ClearResults()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ClearResults));
                return;
            }

            _allRecords.Clear();
            _viewRecords.Clear();
            _seq = 0;
        }

        private void DgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            ResultRecord record = dgvResults.Rows[e.RowIndex].DataBoundItem as ResultRecord;
            if (record == null)
                return;

            if (!string.IsNullOrWhiteSpace(record.SavedPath) && System.IO.File.Exists(record.SavedPath))
            {
                System.Diagnostics.Process.Start(record.SavedPath);
            }
        }

        // 로그 탭 유지 시: 기존 로직 그대로
        private void OnLogUpdated(string logMessage)
        {
            if (listBoxLogs == null)
                return;

            if (listBoxLogs.InvokeRequired)
            {
                listBoxLogs.Invoke(new Action(() => AddLog(logMessage)));
            }
            else
            {
                AddLog(logMessage);
            }
        }

        private void AddLog(string logMessage)
        {
            if (listBoxLogs.Items.Count > 1000)
            {
                listBoxLogs.Items.RemoveAt(0);
            }

            listBoxLogs.Items.Add(logMessage);
            listBoxLogs.TopIndex = listBoxLogs.Items.Count - 1;
        }

        private void ResultForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SLogger.LogUpdated -= OnLogUpdated;
            FormClosed -= ResultForm_FormClosed;
        }

        private void ApplyDarkThemeToGrid(DataGridView grid)
        {
            if (grid == null)
                return;

            // 기본 옵션
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(55, 55, 55);

            grid.EnableHeadersVisualStyles = false; // 헤더 스타일을 우리가 지정한 대로 쓰게 함
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;

            // 배경
            grid.BackgroundColor = Color.FromArgb(30, 34, 40);

            // 셀 스타일
            var cellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 34, 40),
                ForeColor = Color.Gainsboro,
                SelectionBackColor = Color.FromArgb(60, 110, 180),
                SelectionForeColor = Color.White,
                WrapMode = DataGridViewTriState.False
            };
            grid.DefaultCellStyle = cellStyle;

            // 번갈아 행 색
            var altStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(26, 29, 34),
                ForeColor = Color.Gainsboro,
                SelectionBackColor = Color.FromArgb(60, 110, 180),
                SelectionForeColor = Color.White
            };
            grid.AlternatingRowsDefaultCellStyle = altStyle;

            // 헤더 스타일
            var headerStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(45, 45, 48),
                SelectionForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.False
            };
            grid.ColumnHeadersDefaultCellStyle = headerStyle;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            grid.RowHeadersVisible = false;

            grid.RowTemplate.Height = 28;
        }


        private void ApplyDarkThemeToTabs(TabControl tab)
        {
            if (tab == null)
                return;

            tab.DrawMode = TabDrawMode.OwnerDrawFixed;
            tab.SizeMode = TabSizeMode.Fixed;
            tab.ItemSize = new Size(90, 24); // 탭 크기(원하면 조정)
            tab.Appearance = TabAppearance.Normal;

            // 페이지 배경
            foreach (TabPage page in tab.TabPages)
            {
                page.BackColor = Color.FromArgb(30, 34, 40);
                page.ForeColor = Color.Gainsboro;
            }

            tab.DrawItem -= Tab_DrawItem_Dark; // 중복 구독 방지
            tab.DrawItem += Tab_DrawItem_Dark;
        }

        private void Tab_DrawItem_Dark(object sender, DrawItemEventArgs e)
        {
            var tab = (TabControl)sender;
            TabPage page = tab.TabPages[e.Index];
            Rectangle r = e.Bounds;

            bool selected = (e.Index == tab.SelectedIndex);

            Color back = selected ? Color.FromArgb(45, 45, 48) : Color.FromArgb(30, 34, 40);
            Color fore = selected ? Color.White : Color.Gainsboro;
            Color border = Color.FromArgb(70, 70, 70);

            using (var bg = new SolidBrush(back))
                e.Graphics.FillRectangle(bg, r);

            using (var pen = new Pen(border))
                e.Graphics.DrawRectangle(pen, r);

            TextRenderer.DrawText(
                e.Graphics,
                page.Text,
                tab.Font,
                r,
                fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void ApplyDarkButton(Button btn)
        {
            if (btn == null)
                return;

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(70, 70, 70);
            btn.BackColor = Color.FromArgb(45, 45, 48);
            btn.ForeColor = Color.White;

            btn.MouseEnter -= DarkButton_MouseEnter;
            btn.MouseLeave -= DarkButton_MouseLeave;

            btn.MouseEnter += DarkButton_MouseEnter;
            btn.MouseLeave += DarkButton_MouseLeave;
        }

        private void DarkButton_MouseEnter(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.BackColor = Color.FromArgb(60, 60, 64);
        }

        private void DarkButton_MouseLeave(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.BackColor = Color.FromArgb(45, 45, 48);
        }

        private void ApplyDarkComboBox(ComboBox cb)
        {
            if (cb == null)
                return;

            cb.DrawMode = DrawMode.OwnerDrawFixed;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.BackColor = Color.FromArgb(45, 45, 48);
            cb.ForeColor = Color.White;

            cb.DrawItem -= Combo_DrawItem_Dark;
            cb.DrawItem += Combo_DrawItem_Dark;
        }

        private void Combo_DrawItem_Dark(object sender, DrawItemEventArgs e)
        {
            var cb = (ComboBox)sender;
            e.DrawBackground();

            if (e.Index < 0)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color back = selected ? Color.FromArgb(60, 110, 180) : Color.FromArgb(45, 45, 48);
            Color fore = Color.White;

            using (var b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, e.Bounds);

            string text = cb.GetItemText(cb.Items[e.Index]);
            TextRenderer.DrawText(e.Graphics, text, cb.Font, e.Bounds, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            e.DrawFocusRectangle();
        }

        private void ApplyGridColumnsLayout(DataGridView grid)
        {
            if (grid == null || grid.Columns.Count == 0)
                return;

            // 1) 자동 리사이즈 끄고, 우리가 지정한 폭을 쓰게 함
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // 2) 컬럼 폭 (원하는 값으로 조절)
            SetColWidth(grid, "No", 60);
            SetColWidth(grid, "Time", 170);
            SetColWidth(grid, "Result", 70);
            SetColWidth(grid, "DefectType", 90);
            SetColWidth(grid, "Ratio", 90);
            SetColWidth(grid, "SavedPath", 260);

            // Message는 남는 폭을 모두 차지하게(추천)
            if (grid.Columns.Contains("Message"))
                grid.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // 3) 가운데 정렬(헤더 + 셀)
            CenterAlign(grid, "No");
            CenterAlign(grid, "Time");
            CenterAlign(grid, "Result");
            CenterAlign(grid, "DefectType");
            CenterAlign(grid, "Ratio");

            // SavedPath / Message는 보통 왼쪽 정렬이 읽기 좋음
            LeftAlign(grid, "SavedPath");
            LeftAlign(grid, "Message");

            // 4) 표시 포맷(선택)
            if (grid.Columns.Contains("Ratio"))
                grid.Columns["Ratio"].DefaultCellStyle.Format = "0.0000";
        }

        private void SetColWidth(DataGridView grid, string colName, int width)
        {
            if (!grid.Columns.Contains(colName))
                return;

            grid.Columns[colName].Width = width;
        }

        private void CenterAlign(DataGridView grid, string colName)
        {
            if (!grid.Columns.Contains(colName))
                return;

            grid.Columns[colName].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void LeftAlign(DataGridView grid, string colName)
        {
            if (!grid.Columns.Contains(colName))
                return;

            grid.Columns[colName].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.Columns[colName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        }
    }
}
