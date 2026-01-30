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
using FreshCheck_CV.Property;

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

            ApplyDarkButton(btnExportcsv);

            ApplyDarkComboBox(cbResultFilter);


            BackColor = DrawingColor.FromArgb(30, 34, 40);

            if (btnExportcsv != null)
            {
                btnExportcsv.Click += (s, e) => ExportXlsx();
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

                _viewRecords.ListChanged -= ViewRecords_ListChanged;
                _viewRecords.ListChanged += ViewRecords_ListChanged;

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

            _viewRecords.ListChanged -= ViewRecords_ListChanged;
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
                using (var dlg = new FreshCheck_CV.Dialogs.CustomMessageBoxForm("내보낼 데이터가 없습니다.", "데이터 형식 오류"))
                {
                    dlg.ShowDialog(this.FindForm());
                }
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

                using (var dlg = new FreshCheck_CV.Dialogs.CustomMessageBoxForm("Excel(.xlsx) 내보내기가 완료되었습니다.", "파일 내보내기 성공"))
                {
                    dlg.ShowDialog(this.FindForm());
                }
            }
        }

        private void ViewRecords_ListChanged(object sender, ListChangedEventArgs e)
        {
            // 새 아이템이 추가되거나 Reset(필터 변경 등)일 때만 아래로
            if (e.ListChangedType != ListChangedType.ItemAdded &&
                e.ListChangedType != ListChangedType.Reset)
                return;

            ScrollResultsToBottom();
        }


        private void ScrollResultsToBottom()
        {
            if (dgvResults == null)
                return;

            if (dgvResults.IsDisposed || !dgvResults.IsHandleCreated)
                return;

            // UI 스레드에서, 바인딩 반영 끝난 다음 실행
            BeginInvoke(new Action(() =>
            {
                if (dgvResults.RowCount <= 0)
                    return;

                int lastRowIndex = dgvResults.RowCount - 1;

                // 컬럼이 없으면 CurrentCell 설정 시 예외
                if (dgvResults.ColumnCount > 0)
                {
                    dgvResults.ClearSelection();
                    dgvResults.CurrentCell = dgvResults.Rows[lastRowIndex].Cells[0];
                    dgvResults.Rows[lastRowIndex].Selected = true;
                }

                dgvResults.FirstDisplayedScrollingRowIndex = lastRowIndex;
            }));
        }


        private void CreateXlsxWithFilter(string filePath)
        {
            string[] headers = { "No", "Time", "Result", "DefectType", "Ratio(%)", "SavedPath", "Message" };

            uint rowCount = (uint)_viewRecords.Count + 1; // header 포함
            uint colCount = (uint)headers.Length;

            using (SpreadsheetDocument doc = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();

                // ===== Styles 추가 (가독성/숫자/날짜 포맷용) =====
                WorkbookStylesPart stylesPart = wbPart.AddNewPart<WorkbookStylesPart>();
                StyleIndex style = BuildStylesheet(stylesPart);

                // ===== SheetData =====
                var sheetData = new SheetData();

                // ===== Header row =====
                var headerRow = new Row { RowIndex = 1 };
                for (int i = 0; i < headers.Length; i++)
                {
                    headerRow.Append(CreateTextCell(i + 1, 1, headers[i], style.Header));
                }
                sheetData.Append(headerRow);

                // ===== Data rows =====
                for (int r = 0; r < _viewRecords.Count; r++)
                {
                    ResultRecord rec = _viewRecords[r];
                    uint rowIndex = (uint)(r + 2);

                    var row = new Row { RowIndex = rowIndex };

                    // No : 정수(숫자)
                    row.Append(CreateNumberCell(1, rowIndex, rec.No, style.IntCenter));

                    // Time : 날짜/시간(숫자) + 표시 포맷 yyyy-mm-dd hh:mm:ss
                    row.Append(CreateDateTimeCell(2, rowIndex, rec.Time, style.DateTimeCenter));

                    // Result : 텍스트 + 가운데
                    row.Append(CreateTextCell(3, rowIndex, rec.Result ?? "", style.TextCenter));

                    // DefectType : 텍스트 + 가운데
                    row.Append(CreateTextCell(4, rowIndex, rec.DefectType?.ToString() ?? "", style.TextCenter));

                    // Ratio : 퍼센트 숫자 (0~1 가정) + 0.00% 포맷
                    // 만약 rec.Ratio가 0~100(퍼센트 값)이라면 -> (rec.Ratio / 100.0)로 바꿔서 넣어야 함
                    double ratio01 = rec.Ratio;
                    row.Append(CreateNumberCell(5, rowIndex, ratio01, style.PercentRight));

                    // SavedPath : 하이퍼링크(클릭) + 보기용 텍스트
                    row.Append(CreateHyperlinkFormulaCell(6, rowIndex, rec.SavedPath, style.LinkLeft));

                    // Message : 줄바꿈 + 왼쪽
                    row.Append(CreateTextCell(7, rowIndex, rec.Message ?? "", style.WrapLeft));

                    sheetData.Append(row);
                }

                // ===== AutoFilter (원래 로직 유지) =====
                string lastCol = GetExcelColumnName(headers.Length); // G
                string filterRange = $"A1:{lastCol}{rowCount}";

                var autoFilter = new AutoFilter { Reference = filterRange };


                var worksheet = new Worksheet();

                worksheet.Append(new SheetDimension
                {
                    Reference = $"A1:{GetExcelColumnName(headers.Length)}{rowCount}"
                });

                worksheet.Append(BuildFreezeTopRow());
                worksheet.Append(BuildColumns());
                worksheet.Append(sheetData);
                worksheet.Append(autoFilter);

                wsPart.Worksheet = worksheet;
                wsPart.Worksheet.Save();

                // ===== Workbook에 Sheet 등록 =====
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
        private static Cell CreateTextCell(int columnIndex, uint rowIndex, string text, uint styleIndex = 0)
        {
            var cell = new Cell
            {
                CellReference = GetExcelColumnName(columnIndex) + rowIndex,
                DataType = CellValues.InlineString,
                StyleIndex = styleIndex
            };

            cell.InlineString = new InlineString(new OxText(text ?? ""));
            return cell;
        }

        private static Cell CreateNumberCell(int columnIndex, uint rowIndex, int value, uint styleIndex = 0)
        {
            return CreateNumberCell(columnIndex, rowIndex, (double)value, styleIndex);
        }

        private static Cell CreateNumberCell(int columnIndex, uint rowIndex, double value, uint styleIndex = 0)
        {
            // Number 셀은 DataType 생략(또는 Number)해도 되지만 명확히 유지
            var cell = new Cell
            {
                CellReference = GetExcelColumnName(columnIndex) + rowIndex,
                DataType = CellValues.Number,
                StyleIndex = styleIndex,
                CellValue = new CellValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture))
            };
            return cell;
        }

        private static Cell CreateDateTimeCell(int columnIndex, uint rowIndex, DateTime dt, uint styleIndex)
        {
            // Excel 날짜/시간 = OADate(double)
            double oa = dt.ToOADate();
            return CreateNumberCell(columnIndex, rowIndex, oa, styleIndex);
        }

        private static Cell CreateHyperlinkFormulaCell(int columnIndex, uint rowIndex, string path, uint styleIndex)
        {
            string display;
            try
            {
                display = string.IsNullOrWhiteSpace(path) ? "" : System.IO.Path.GetFileName(path);
            }
            catch
            {
                display = "Open";
            }

            var cell = new Cell
            {
                CellReference = GetExcelColumnName(columnIndex) + rowIndex,
                StyleIndex = styleIndex
            };

            if (string.IsNullOrWhiteSpace(path))
            {
                cell.DataType = CellValues.InlineString;
                cell.InlineString = new InlineString(new OxText(""));
                return cell;
            }

            string safePath = path.Replace("\"", "\"\"");
            string safeDisplay = (display ?? "").Replace("\"", "\"\"");

            // ✅ 수식
            cell.CellFormula = new CellFormula($"HYPERLINK(\"{safePath}\",\"{safeDisplay}\")");

            // ✅ 캐시 값 + 타입 (Excel 복구 트리거 감소)
            cell.DataType = CellValues.String;
            cell.CellValue = new CellValue(display);

            return cell;
        }
        private sealed class StyleIndex
        {
            public uint Header;
            public uint IntCenter;
            public uint DateTimeCenter;
            public uint TextCenter;
            public uint PercentRight;
            public uint LinkLeft;
            public uint WrapLeft;
        }

        private static StyleIndex BuildStylesheet(WorkbookStylesPart stylesPart)
        {
            // ===== NumberingFormats =====
            var nfs = new NumberingFormats();

            uint fmtDateTimeId = 164;
            nfs.Append(new NumberingFormat
            {
                NumberFormatId = fmtDateTimeId,
                FormatCode = StringValue.FromString("yyyy-mm-dd hh:mm:ss")
            });

            uint fmtPercentId = 165;
            nfs.Append(new NumberingFormat
            {
                NumberFormatId = fmtPercentId,
                FormatCode = StringValue.FromString("0.00%")
            });

            nfs.Count = (uint)nfs.ChildElements.Count;

            // ===== Fonts =====
            var fonts = new Fonts(
                new DocumentFormat.OpenXml.Spreadsheet.Font(new FontSize { Val = 11 }),                 // 0
                new DocumentFormat.OpenXml.Spreadsheet.Font(new Bold(), new FontSize { Val = 11 }),     // 1 header
                new DocumentFormat.OpenXml.Spreadsheet.Font(                                            // 2 link
                    new Underline(),
                    new DocumentFormat.OpenXml.Spreadsheet.Color { Rgb = HexBinaryValue.FromString("0563C1") },
                    new FontSize { Val = 11 }
                )
            );
            fonts.Count = (uint)fonts.ChildElements.Count;

            // ===== Fills =====
            var fills = new Fills(
                new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = HexBinaryValue.FromString("2D2D30") })
                { PatternType = PatternValues.Solid })
            );
            fills.Count = (uint)fills.ChildElements.Count;

            // ===== Borders =====
            var borders = new Borders(
                new Border(
                    new LeftBorder { Style = BorderStyleValues.Thin },
                    new RightBorder { Style = BorderStyleValues.Thin },
                    new TopBorder { Style = BorderStyleValues.Thin },
                    new BottomBorder { Style = BorderStyleValues.Thin },
                    new DiagonalBorder()
                )
            );
            borders.Count = (uint)borders.ChildElements.Count;

            // ✅ 필수: CellStyleFormats (기본 1개)
            var csfs = new CellStyleFormats(
                new CellFormat
                {
                    NumberFormatId = 0,
                    FontId = 0,
                    FillId = 0,
                    BorderId = 0
                }
            );
            csfs.Count = 1;

            // ===== CellFormats =====
            var cfs = new CellFormats();

            // 0: 기본
            cfs.Append(new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                ApplyFont = true,
                ApplyBorder = true
            });

            // 1: 헤더
            cfs.Append(new CellFormat
            {
                FontId = 1,
                FillId = 2,
                BorderId = 0,
                ApplyFont = true,
                ApplyFill = true,
                ApplyBorder = true,
                Alignment = new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Center,
                    Vertical = VerticalAlignmentValues.Center
                },
                ApplyAlignment = true
            });

            // 2: 정수 가운데
            cfs.Append(new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                ApplyBorder = true,
                Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center },
                ApplyAlignment = true
            });

            // 3: 날짜/시간
            cfs.Append(new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                NumberFormatId = fmtDateTimeId,
                ApplyNumberFormat = true,
                ApplyBorder = true,
                Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center },
                ApplyAlignment = true
            });

            // 4: 텍스트 가운데
            cfs.Append(new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                ApplyBorder = true,
                Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center },
                ApplyAlignment = true
            });

            // 5: 퍼센트 오른쪽
            cfs.Append(new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                NumberFormatId = fmtPercentId,
                ApplyNumberFormat = true,
                ApplyBorder = true,
                Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Right, Vertical = VerticalAlignmentValues.Center },
                ApplyAlignment = true
            });

            // 6: 링크 왼쪽
            cfs.Append(new CellFormat
            {
                FontId = 2,
                FillId = 0,
                BorderId = 0,
                ApplyFont = true,
                ApplyBorder = true,
                Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Left, Vertical = VerticalAlignmentValues.Center },
                ApplyAlignment = true
            });

            // 7: 메시지 줄바꿈
            cfs.Append(new CellFormat
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0,
                ApplyBorder = true,
                Alignment = new Alignment
                {
                    Horizontal = HorizontalAlignmentValues.Left,
                    Vertical = VerticalAlignmentValues.Top,
                    WrapText = true
                },
                ApplyAlignment = true
            });

            cfs.Count = (uint)cfs.ChildElements.Count;

            // ✅ 필수: CellStyles (Normal)
            var cellStyles = new CellStyles(
                new CellStyle
                {
                    Name = "Normal",
                    FormatId = 0,
                    BuiltinId = 0
                }
            );
            cellStyles.Count = 1;

            // ✅ 필수: DifferentialFormats / TableStyles
            var dxfs = new DifferentialFormats { Count = 0 };
            var tableStyles = new TableStyles
            {
                Count = 0,
                DefaultTableStyle = "TableStyleMedium9",
                DefaultPivotStyle = "PivotStyleLight16"
            };

            var ss = new Stylesheet();

            // NumberingFormats는 존재할 때만 추가하는 게 더 안전(지금은 2개 있으니 OK)
            ss.Append(nfs);
            ss.Append(fonts);
            ss.Append(fills);
            ss.Append(borders);
            ss.Append(csfs);
            ss.Append(cfs);
            ss.Append(cellStyles);
            ss.Append(dxfs);
            ss.Append(tableStyles);

            stylesPart.Stylesheet = ss;
            stylesPart.Stylesheet.Save();

            return new StyleIndex
            {
                Header = 1,
                IntCenter = 2,
                DateTimeCenter = 3,
                TextCenter = 4,
                PercentRight = 5,
                LinkLeft = 6,
                WrapLeft = 7
            };
        }
        private static Columns BuildColumns()
        {
            // 엑셀 열 너비는 대략 “표시 글자 폭” 단위
            var cols = new Columns();

            cols.Append(new Column { Min = 1, Max = 1, Width = 6, CustomWidth = true });   // No
            cols.Append(new Column { Min = 2, Max = 2, Width = 20, CustomWidth = true });  // Time
            cols.Append(new Column { Min = 3, Max = 3, Width = 10, CustomWidth = true });  // Result
            cols.Append(new Column { Min = 4, Max = 4, Width = 12, CustomWidth = true });  // DefectType
            cols.Append(new Column { Min = 5, Max = 5, Width = 10, CustomWidth = true });  // Ratio(%)
            cols.Append(new Column { Min = 6, Max = 6, Width = 35, CustomWidth = true });  // SavedPath(link text)
            cols.Append(new Column { Min = 7, Max = 7, Width = 50, CustomWidth = true });  // Message

            return cols;
        }

        private static SheetViews BuildFreezeTopRow()
        {
            // 1행 고정
            var sheetViews = new SheetViews();
            var sheetView = new SheetView { WorkbookViewId = 0U };

            var pane = new Pane
            {
                HorizontalSplit = 1D,
                TopLeftCell = "A2",
                ActivePane = PaneValues.BottomLeft,
                State = PaneStateValues.Frozen
            };

            var selection = new Selection
            {
                Pane = PaneValues.BottomLeft,
                ActiveCell = "A2",
                SequenceOfReferences = new ListValue<StringValue> { InnerText = "A2" }
            };

            sheetView.Append(pane);
            sheetView.Append(selection);

            sheetViews.Append(sheetView);
            return sheetViews;
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
