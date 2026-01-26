using FreshCheck_CV.Models;
using FreshCheck_CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using static System.Net.Mime.MediaTypeNames;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DrawingColor = System.Drawing.Color;
using OxText = DocumentFormat.OpenXml.Spreadsheet.Text;

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


            BackColor = DrawingColor.FromArgb(30, 34, 40);

            if (btnExportCsv != null)
            {
                btnExportCsv.Click += (s, e) => ExportXlsx();
            }

            if (listBoxLogs != null)
            {
                listBoxLogs.BackColor = DrawingColor.FromArgb(30, 34, 40);
                listBoxLogs.ForeColor = DrawingColor.Gainsboro;

                FormClosed += ResultForm_FormClosed;
                SLogger.LogUpdated += OnLogUpdated;
            }

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
                cbResultFilter.Items.Add("Mold");
                cbResultFilter.Items.Add("Scratch");

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


            if (string.Equals(filter, "ALL", StringComparison.OrdinalIgnoreCase))
            {
            }

            else if (string.Equals(filter, "OK", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(x => string.Equals(x.Result, "OK", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(filter, "Mold", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(x =>
                    x.DefectType != null &&
                    string.Equals(x.DefectType.ToString(), "Mold", StringComparison.OrdinalIgnoreCase));
            }
            else if (string.Equals(filter, "Scratch", StringComparison.OrdinalIgnoreCase))
            {
                q = q.Where(x =>
                    x.DefectType != null &&
                    string.Equals(x.DefectType.ToString(), "Scratch", StringComparison.OrdinalIgnoreCase));
            }

            _viewRecords.RaiseListChangedEvents = false;
            _viewRecords.Clear();

            foreach (ResultRecord r in q)
                _viewRecords.Add(r);

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

            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = DrawingColor.FromArgb(55, 55, 55);

            grid.EnableHeadersVisualStyles = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;

            grid.BackgroundColor = DrawingColor.FromArgb(30, 34, 40);

            var cellStyle = new DataGridViewCellStyle
            {
                BackColor = DrawingColor.FromArgb(30, 34, 40),
                ForeColor = DrawingColor.Gainsboro,
                SelectionBackColor = DrawingColor.FromArgb(60, 110, 180),
                SelectionForeColor = DrawingColor.White,
                WrapMode = DataGridViewTriState.False
            };
            grid.DefaultCellStyle = cellStyle;

            var altStyle = new DataGridViewCellStyle
            {
                BackColor = DrawingColor.FromArgb(26, 29, 34),
                ForeColor = DrawingColor.Gainsboro,
                SelectionBackColor = DrawingColor.FromArgb(60, 110, 180),
                SelectionForeColor = DrawingColor.White
            };
            grid.AlternatingRowsDefaultCellStyle = altStyle;

            var headerStyle = new DataGridViewCellStyle
            {
                BackColor = DrawingColor.FromArgb(45, 45, 48),
                ForeColor = DrawingColor.White,
                SelectionBackColor = DrawingColor.FromArgb(45, 45, 48),
                SelectionForeColor = DrawingColor.White,
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
            tab.ItemSize = new Size(90, 24);
            tab.Appearance = TabAppearance.Normal;

            // 페이지 배경
            foreach (TabPage page in tab.TabPages)
            {
                page.BackColor = DrawingColor.FromArgb(30, 34, 40);
                page.ForeColor = DrawingColor.Gainsboro;
            }

            tab.DrawItem -= Tab_DrawItem_Dark;
            tab.DrawItem += Tab_DrawItem_Dark;
        }

        private void Tab_DrawItem_Dark(object sender, DrawItemEventArgs e)
        {
            var tab = (TabControl)sender;
            TabPage page = tab.TabPages[e.Index];
            Rectangle r = e.Bounds;

            bool selected = (e.Index == tab.SelectedIndex);

            DrawingColor back = selected ? DrawingColor.FromArgb(45, 45, 48) : DrawingColor.FromArgb(30, 34, 40);
            DrawingColor fore = selected ? DrawingColor.White : DrawingColor.Gainsboro;
            DrawingColor border = DrawingColor.FromArgb(70, 70, 70);

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
            btn.FlatAppearance.BorderColor = DrawingColor.FromArgb(70, 70, 70);
            btn.BackColor = DrawingColor.FromArgb(45, 45, 48);
            btn.ForeColor = DrawingColor.White;

            btn.MouseEnter -= DarkButton_MouseEnter;
            btn.MouseLeave -= DarkButton_MouseLeave;

            btn.MouseEnter += DarkButton_MouseEnter;
            btn.MouseLeave += DarkButton_MouseLeave;
        }

        private void DarkButton_MouseEnter(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.BackColor = DrawingColor.FromArgb(60, 60, 64);
        }

        private void DarkButton_MouseLeave(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.BackColor = DrawingColor.FromArgb(45, 45, 48);
        }

        private void ApplyDarkComboBox(ComboBox cb)
        {
            if (cb == null)
                return;

            cb.DrawMode = DrawMode.OwnerDrawFixed;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.BackColor = DrawingColor.FromArgb(45, 45, 48);
            cb.ForeColor = DrawingColor.White;

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

            DrawingColor back = selected ? DrawingColor.FromArgb(60, 110, 180) : DrawingColor.FromArgb(45, 45, 48);
            DrawingColor fore = DrawingColor.White;

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

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            SetColWidth(grid, "No", 60);
            SetColWidth(grid, "Time", 170);
            SetColWidth(grid, "Result", 70);
            SetColWidth(grid, "DefectType", 90);
            SetColWidth(grid, "Ratio", 90);
            SetColWidth(grid, "SavedPath", 260);

            if (grid.Columns.Contains("Message"))
                grid.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            CenterAlign(grid, "No");
            CenterAlign(grid, "Time");
            CenterAlign(grid, "Result");
            CenterAlign(grid, "DefectType");
            CenterAlign(grid, "Ratio");

            LeftAlign(grid, "SavedPath");
            LeftAlign(grid, "Message");

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

        private void ExportXlsx()
        {
            if (_viewRecords == null || _viewRecords.Count == 0)
            {
                MessageBox.Show("내보낼 데이터가 없습니다.", "Export Excel",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel 파일 (*.xlsx)|*.xlsx";
                sfd.Title = "결과 Excel 내보내기";
                sfd.FileName = $"results_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                CreateXlsxWithFilter(sfd.FileName);

                MessageBox.Show("Excel(.xlsx) 내보내기가 완료되었습니다.", "Export Excel",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CreateXlsxWithFilter(string filePath)
        {
            string[] headers = { "No", "Time", "Result", "DefectType", "Ratio", "SavedPath", "Message" };

            uint rowCount = (uint)_viewRecords.Count + 1; // header 포함
            using (SpreadsheetDocument doc = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();

                // ===== SheetData 생성 =====
                var sheetData = new SheetData();

                // Header row
                var headerRow = new Row { RowIndex = 1 };
                for (int i = 0; i < headers.Length; i++)
                    headerRow.Append(CreateTextCell(i + 1, 1, headers[i]));
                sheetData.Append(headerRow);

                // Data rows
                for (int r = 0; r < _viewRecords.Count; r++)
                {
                    ResultRecord rec = _viewRecords[r];
                    uint rowIndex = (uint)(r + 2);

                    var row = new Row { RowIndex = rowIndex };
                    row.Append(CreateTextCell(1, rowIndex, rec.No.ToString()));
                    row.Append(CreateTextCell(2, rowIndex, rec.Time.ToString("yyyy-MM-dd HH:mm:ss")));
                    row.Append(CreateTextCell(3, rowIndex, rec.Result ?? ""));
                    row.Append(CreateTextCell(4, rowIndex, rec.DefectType?.ToString() ?? ""));
                    row.Append(CreateTextCell(5, rowIndex, rec.Ratio.ToString("0.0000")));
                    row.Append(CreateTextCell(6, rowIndex, rec.SavedPath ?? ""));
                    row.Append(CreateTextCell(7, rowIndex, rec.Message ?? ""));
                    sheetData.Append(row);
                }

                // ===== AutoFilter (Result, DefectType, Message만 버튼 표시) =====
                string lastCol = GetExcelColumnName(headers.Length); // 7 -> G
                string filterRange = $"A1:{lastCol}{rowCount}";

                var autoFilter = new AutoFilter { Reference = filterRange };

                int[] enableCols = { 2, 3, 6 }; // 0-based: C(Result), D(DefectType), G(Message)
                for (int colId = 0; colId < headers.Length; colId++)
                {
                    if (!enableCols.Contains(colId))
                    {
                        autoFilter.Append(new FilterColumn
                        {
                            ColumnId = (uint)colId,
                            HiddenButton = true
                        });
                    }
                }

                // ===== Worksheet 구성 (순서 중요: SheetData -> AutoFilter) =====
                var worksheet = new Worksheet();
                worksheet.Append(sheetData);
                worksheet.Append(autoFilter);

                wsPart.Worksheet = worksheet;
                wsPart.Worksheet.Save();

                // ===== (중요) Workbook에 Sheet 등록 =====
                Sheets sheets = wbPart.Workbook.AppendChild(new Sheets());

                Sheet sheet = new Sheet
                {
                    Id = wbPart.GetIdOfPart(wsPart),
                    SheetId = 1,
                    Name = "Results"
                };
                sheets.Append(sheet);

                wbPart.Workbook.Save();
            }
        }
        private static Cell CreateTextCell(int columnIndex, uint rowIndex, string text)
        {
            var cell = new Cell
            {
                CellReference = GetExcelColumnName(columnIndex) + rowIndex,
                DataType = CellValues.InlineString
            };

            cell.InlineString = new InlineString(new OxText(text ?? ""));
            return cell;
        }

        // 1->A, 2->B ... 27->AA
        private static string GetExcelColumnName(int columnNumber)
        {
            string col = "";
            while (columnNumber > 0)
            {
                int mod = (columnNumber - 1) % 26;
                col = (char)('A' + mod) + col;
                columnNumber = (columnNumber - mod) / 26;
                columnNumber--;
            }
            return col;
        }
    }
}
