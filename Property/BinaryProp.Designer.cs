namespace FreshCheck_CV.Property
{
    partial class BinaryProp
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tlpRoot = new System.Windows.Forms.TableLayoutPanel();
            this.pnlMode = new System.Windows.Forms.Panel();
            this.cbMode = new System.Windows.Forms.ComboBox();
            this.lblMode = new System.Windows.Forms.Label();
            this.grpTarget = new System.Windows.Forms.GroupBox();
            this.tlpTarget = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTargetSwatch = new System.Windows.Forms.Panel();
            this.lblTargetColor = new System.Windows.Forms.Label();
            this.btnPickColor = new System.Windows.Forms.Button();
            this.grpTolerance = new System.Windows.Forms.GroupBox();
            this.tlpTol = new System.Windows.Forms.TableLayoutPanel();
            this.lblTolerance = new System.Windows.Forms.Label();
            this.chkAutoApply = new System.Windows.Forms.CheckBox();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.tlpOptions = new System.Windows.Forms.TableLayoutPanel();
            this.chkInvert = new System.Windows.Forms.CheckBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.grpTest = new System.Windows.Forms.GroupBox();
            this.tlpTest = new System.Windows.Forms.TableLayoutPanel();
            this.btnRunMold = new System.Windows.Forms.Button();
            this.rangeTrackbar = new FreshCheck_CV.UIControl.RangeTrackbar();
            this.tlpRoot.SuspendLayout();
            this.pnlMode.SuspendLayout();
            this.grpTarget.SuspendLayout();
            this.tlpTarget.SuspendLayout();
            this.grpTolerance.SuspendLayout();
            this.tlpTol.SuspendLayout();
            this.grpOptions.SuspendLayout();
            this.tlpOptions.SuspendLayout();
            this.grpTest.SuspendLayout();
            this.tlpTest.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpRoot
            // 
            this.tlpRoot.ColumnCount = 1;
            this.tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpRoot.Controls.Add(this.pnlMode, 0, 0);
            this.tlpRoot.Controls.Add(this.grpTarget, 0, 1);
            this.tlpRoot.Controls.Add(this.grpTolerance, 0, 2);
            this.tlpRoot.Controls.Add(this.grpOptions, 0, 3);
            this.tlpRoot.Controls.Add(this.grpTest, 0, 4);
            this.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpRoot.Location = new System.Drawing.Point(0, 0);
            this.tlpRoot.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tlpRoot.Name = "tlpRoot";
            this.tlpRoot.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.tlpRoot.RowCount = 5;
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRoot.Size = new System.Drawing.Size(283, 354);
            this.tlpRoot.TabIndex = 0;
            // 
            // pnlMode
            // 
            this.pnlMode.Controls.Add(this.cbMode);
            this.pnlMode.Controls.Add(this.lblMode);
            this.pnlMode.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMode.Location = new System.Drawing.Point(8, 8);
            this.pnlMode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.pnlMode.Name = "pnlMode";
            this.pnlMode.Size = new System.Drawing.Size(267, 23);
            this.pnlMode.TabIndex = 0;
            // 
            // cbMode
            // 
            this.cbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMode.FormattingEnabled = true;
            this.cbMode.Location = new System.Drawing.Point(63, 2);
            this.cbMode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbMode.Name = "cbMode";
            this.cbMode.Size = new System.Drawing.Size(166, 20);
            this.cbMode.TabIndex = 1;
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblMode.Location = new System.Drawing.Point(0, 5);
            this.lblMode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(57, 12);
            this.lblMode.TabIndex = 0;
            this.lblMode.Text = "보기 모드";
            // 
            // grpTarget
            // 
            this.grpTarget.AutoSize = true;
            this.grpTarget.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpTarget.Controls.Add(this.tlpTarget);
            this.grpTarget.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTarget.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.grpTarget.Location = new System.Drawing.Point(8, 38);
            this.grpTarget.Margin = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.grpTarget.Name = "grpTarget";
            this.grpTarget.Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.grpTarget.Size = new System.Drawing.Size(267, 55);
            this.grpTarget.TabIndex = 2;
            this.grpTarget.TabStop = false;
            this.grpTarget.Text = "타깃 색상";
            // 
            // tlpTarget
            // 
            this.tlpTarget.ColumnCount = 3;
            this.tlpTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tlpTarget.Controls.Add(this.pnlTargetSwatch, 0, 0);
            this.tlpTarget.Controls.Add(this.lblTargetColor, 1, 0);
            this.tlpTarget.Controls.Add(this.btnPickColor, 2, 0);
            this.tlpTarget.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpTarget.Location = new System.Drawing.Point(7, 21);
            this.tlpTarget.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tlpTarget.Name = "tlpTarget";
            this.tlpTarget.RowCount = 1;
            this.tlpTarget.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTarget.Size = new System.Drawing.Size(253, 27);
            this.tlpTarget.TabIndex = 0;
            // 
            // pnlTargetSwatch
            // 
            this.pnlTargetSwatch.BackColor = System.Drawing.Color.Black;
            this.pnlTargetSwatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTargetSwatch.Location = new System.Drawing.Point(0, 3);
            this.pnlTargetSwatch.Margin = new System.Windows.Forms.Padding(0, 3, 6, 3);
            this.pnlTargetSwatch.Name = "pnlTargetSwatch";
            this.pnlTargetSwatch.Size = new System.Drawing.Size(14, 21);
            this.pnlTargetSwatch.TabIndex = 0;
            // 
            // lblTargetColor
            // 
            this.lblTargetColor.AutoSize = true;
            this.lblTargetColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTargetColor.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblTargetColor.Location = new System.Drawing.Point(20, 5);
            this.lblTargetColor.Margin = new System.Windows.Forms.Padding(0, 5, 6, 0);
            this.lblTargetColor.Name = "lblTargetColor";
            this.lblTargetColor.Size = new System.Drawing.Size(163, 22);
            this.lblTargetColor.TabIndex = 1;
            this.lblTargetColor.Text = "Target: (B=0, G=0, R=0)";
            // 
            // btnPickColor
            // 
            this.btnPickColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPickColor.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnPickColor.Location = new System.Drawing.Point(191, 2);
            this.btnPickColor.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnPickColor.Name = "btnPickColor";
            this.btnPickColor.Size = new System.Drawing.Size(80, 23);
            this.btnPickColor.TabIndex = 2;
            this.btnPickColor.Text = "스포이드";
            this.btnPickColor.UseVisualStyleBackColor = true;
            // 
            // grpTolerance
            // 
            this.grpTolerance.AutoSize = true;
            this.grpTolerance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpTolerance.Controls.Add(this.tlpTol);
            this.grpTolerance.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTolerance.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.grpTolerance.Location = new System.Drawing.Point(8, 100);
            this.grpTolerance.Margin = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.grpTolerance.Name = "grpTolerance";
            this.grpTolerance.Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.grpTolerance.Size = new System.Drawing.Size(267, 95);
            this.grpTolerance.TabIndex = 3;
            this.grpTolerance.TabStop = false;
            this.grpTolerance.Text = "허용오차(민감도)";
            // 
            // tlpTol
            // 
            this.tlpTol.ColumnCount = 1;
            this.tlpTol.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTol.Controls.Add(this.lblTolerance, 0, 0);
            this.tlpTol.Controls.Add(this.rangeTrackbar, 0, 1);
            this.tlpTol.Controls.Add(this.chkAutoApply, 0, 2);
            this.tlpTol.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpTol.Location = new System.Drawing.Point(7, 21);
            this.tlpTol.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tlpTol.Name = "tlpTol";
            this.tlpTol.RowCount = 3;
            this.tlpTol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTol.Size = new System.Drawing.Size(253, 67);
            this.tlpTol.TabIndex = 0;
            // 
            // lblTolerance
            // 
            this.lblTolerance.AutoSize = true;
            this.lblTolerance.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTolerance.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTolerance.Location = new System.Drawing.Point(0, 0);
            this.lblTolerance.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.lblTolerance.Name = "lblTolerance";
            this.lblTolerance.Size = new System.Drawing.Size(253, 12);
            this.lblTolerance.TabIndex = 0;
            this.lblTolerance.Text = "허용오차: −80 / +120";
            // 
            // chkAutoApply
            // 
            this.chkAutoApply.AutoSize = true;
            this.chkAutoApply.Dock = System.Windows.Forms.DockStyle.Top;
            this.chkAutoApply.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.chkAutoApply.Location = new System.Drawing.Point(0, 55);
            this.chkAutoApply.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.chkAutoApply.Name = "chkAutoApply";
            this.chkAutoApply.Size = new System.Drawing.Size(253, 16);
            this.chkAutoApply.TabIndex = 2;
            this.chkAutoApply.Text = "자동 적용";
            this.chkAutoApply.UseVisualStyleBackColor = true;
            // 
            // grpOptions
            // 
            this.grpOptions.AutoSize = true;
            this.grpOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpOptions.Controls.Add(this.tlpOptions);
            this.grpOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpOptions.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.grpOptions.Location = new System.Drawing.Point(8, 202);
            this.grpOptions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 7);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.grpOptions.Size = new System.Drawing.Size(267, 55);
            this.grpOptions.TabIndex = 4;
            this.grpOptions.TabStop = false;
            this.grpOptions.Text = "옵션";
            // 
            // tlpOptions
            // 
            this.tlpOptions.ColumnCount = 2;
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.tlpOptions.Controls.Add(this.chkInvert, 0, 0);
            this.tlpOptions.Controls.Add(this.btnApply, 1, 0);
            this.tlpOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpOptions.Location = new System.Drawing.Point(7, 21);
            this.tlpOptions.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tlpOptions.Name = "tlpOptions";
            this.tlpOptions.RowCount = 1;
            this.tlpOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOptions.Size = new System.Drawing.Size(253, 27);
            this.tlpOptions.TabIndex = 0;
            // 
            // chkInvert
            // 
            this.chkInvert.AutoSize = true;
            this.chkInvert.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkInvert.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.chkInvert.Location = new System.Drawing.Point(0, 4);
            this.chkInvert.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(189, 23);
            this.chkInvert.TabIndex = 0;
            this.chkInvert.Text = "반전";
            this.chkInvert.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            this.btnApply.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApply.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnApply.Location = new System.Drawing.Point(191, 2);
            this.btnApply.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(60, 23);
            this.btnApply.TabIndex = 1;
            this.btnApply.Text = "적용";
            this.btnApply.UseVisualStyleBackColor = true;
            // 
            // grpTest
            // 
            this.grpTest.AutoSize = true;
            this.grpTest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.grpTest.Controls.Add(this.tlpTest);
            this.grpTest.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTest.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.grpTest.Location = new System.Drawing.Point(8, 264);
            this.grpTest.Margin = new System.Windows.Forms.Padding(0);
            this.grpTest.Name = "grpTest";
            this.grpTest.Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.grpTest.Size = new System.Drawing.Size(267, 55);
            this.grpTest.TabIndex = 5;
            this.grpTest.TabStop = false;
            this.grpTest.Text = "테스트";
            // 
            // tlpTest
            // 
            this.tlpTest.ColumnCount = 1;
            this.tlpTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTest.Controls.Add(this.btnRunMold, 0, 0);
            this.tlpTest.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpTest.Location = new System.Drawing.Point(7, 21);
            this.tlpTest.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tlpTest.Name = "tlpTest";
            this.tlpTest.RowCount = 1;
            this.tlpTest.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpTest.Size = new System.Drawing.Size(253, 27);
            this.tlpTest.TabIndex = 0;
            // 
            // btnRunMold
            // 
            this.btnRunMold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRunMold.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnRunMold.Location = new System.Drawing.Point(2, 2);
            this.btnRunMold.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnRunMold.Name = "btnRunMold";
            this.btnRunMold.Size = new System.Drawing.Size(249, 23);
            this.btnRunMold.TabIndex = 0;
            this.btnRunMold.Text = "현재 프레임 테스트";
            this.btnRunMold.UseVisualStyleBackColor = true;
            this.btnRunMold.Click += new System.EventHandler(this.btnRunMold_Click);
            // 
            // rangeTrackbar
            // 
            this.rangeTrackbar.BackColor = System.Drawing.Color.Transparent;
            this.rangeTrackbar.BubbleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(38)))), ((int)(((byte)(44)))));
            this.rangeTrackbar.CornerRadius = 6;
            this.rangeTrackbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.rangeTrackbar.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.rangeTrackbar.Location = new System.Drawing.Point(0, 15);
            this.rangeTrackbar.Margin = new System.Windows.Forms.Padding(0);
            this.rangeTrackbar.Name = "rangeTrackbar";
            this.rangeTrackbar.RangeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(140)))), ((int)(((byte)(220)))));
            this.rangeTrackbar.Size = new System.Drawing.Size(253, 36);
            this.rangeTrackbar.TabIndex = 1;
            this.rangeTrackbar.ThumbBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.rangeTrackbar.ThumbColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.rangeTrackbar.ThumbDiameter = 16;
            this.rangeTrackbar.TrackBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(66)))), ((int)(((byte)(74)))));
            this.rangeTrackbar.TrackThickness = 6;
            this.rangeTrackbar.ValueLeft = 80;
            this.rangeTrackbar.ValueRight = 120;
            // 
            // BinaryProp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(32)))), ((int)(((byte)(38)))));
            this.Controls.Add(this.tlpRoot);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "BinaryProp";
            this.Size = new System.Drawing.Size(283, 354);
            this.tlpRoot.ResumeLayout(false);
            this.tlpRoot.PerformLayout();
            this.pnlMode.ResumeLayout(false);
            this.pnlMode.PerformLayout();
            this.grpTarget.ResumeLayout(false);
            this.tlpTarget.ResumeLayout(false);
            this.tlpTarget.PerformLayout();
            this.grpTolerance.ResumeLayout(false);
            this.tlpTol.ResumeLayout(false);
            this.tlpTol.PerformLayout();
            this.grpOptions.ResumeLayout(false);
            this.tlpOptions.ResumeLayout(false);
            this.tlpOptions.PerformLayout();
            this.grpTest.ResumeLayout(false);
            this.tlpTest.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.Panel pnlMode;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.ComboBox cbMode;

        private System.Windows.Forms.GroupBox grpTarget;
        private System.Windows.Forms.TableLayoutPanel tlpTarget;
        private System.Windows.Forms.Panel pnlTargetSwatch;
        private System.Windows.Forms.Label lblTargetColor;
        private System.Windows.Forms.Button btnPickColor;

        private System.Windows.Forms.GroupBox grpTolerance;
        private System.Windows.Forms.TableLayoutPanel tlpTol;
        private System.Windows.Forms.Label lblTolerance;
        private FreshCheck_CV.UIControl.RangeTrackbar rangeTrackbar;
        private System.Windows.Forms.CheckBox chkAutoApply;

        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.TableLayoutPanel tlpOptions;
        private System.Windows.Forms.CheckBox chkInvert;
        private System.Windows.Forms.Button btnApply;

        private System.Windows.Forms.GroupBox grpTest;
        private System.Windows.Forms.TableLayoutPanel tlpTest;
        private System.Windows.Forms.Button btnRunMold;
    }
}

