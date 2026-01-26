namespace FreshCheck_CV
{
    partial class ResultForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabResults = new System.Windows.Forms.TabPage();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.cbResultFilter = new System.Windows.Forms.ComboBox();
            this.tabLogs = new System.Windows.Forms.TabPage();
            this.listBoxLogs = new System.Windows.Forms.ListBox();
            this.darkTabControl1 = new FreshCheck_CV.UIControl.DarkTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabMain.SuspendLayout();
            this.tabResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.panelTop.SuspendLayout();
            this.tabLogs.SuspendLayout();
            this.darkTabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabResults);
            this.tabMain.Controls.Add(this.tabLogs);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Right;
            this.tabMain.Location = new System.Drawing.Point(341, 0);
            this.tabMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(552, 675);
            this.tabMain.TabIndex = 1;
            // 
            // tabResults
            // 
            this.tabResults.Controls.Add(this.dgvResults);
            this.tabResults.Controls.Add(this.panelTop);
            this.tabResults.Location = new System.Drawing.Point(4, 25);
            this.tabResults.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabResults.Name = "tabResults";
            this.tabResults.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabResults.Size = new System.Drawing.Size(544, 646);
            this.tabResults.TabIndex = 0;
            this.tabResults.Text = "Results";
            this.tabResults.UseVisualStyleBackColor = true;
            // 
            // dgvResults
            // 
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.Location = new System.Drawing.Point(3, 33);
            this.dgvResults.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.RowHeadersWidth = 51;
            this.dgvResults.RowTemplate.Height = 23;
            this.dgvResults.Size = new System.Drawing.Size(538, 609);
            this.dgvResults.TabIndex = 1;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnExportCsv);
            this.panelTop.Controls.Add(this.btnClear);
            this.panelTop.Controls.Add(this.cbResultFilter);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(3, 4);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(538, 29);
            this.panelTop.TabIndex = 0;
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Location = new System.Drawing.Point(238, -1);
            this.btnExportCsv.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(125, 29);
            this.btnExportCsv.TabIndex = 2;
            this.btnExportCsv.Text = "Export CSV";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(145, -1);
            this.btnClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(86, 29);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            // 
            // cbResultFilter
            // 
            this.cbResultFilter.FormattingEnabled = true;
            this.cbResultFilter.Items.AddRange(new object[] {
            "ALL",
            "OK",
            "NG"});
            this.cbResultFilter.Location = new System.Drawing.Point(2, 0);
            this.cbResultFilter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbResultFilter.Name = "cbResultFilter";
            this.cbResultFilter.Size = new System.Drawing.Size(138, 23);
            this.cbResultFilter.TabIndex = 0;
            // 
            // tabLogs
            // 
            this.tabLogs.Controls.Add(this.listBoxLogs);
            this.tabLogs.Location = new System.Drawing.Point(4, 25);
            this.tabLogs.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabLogs.Name = "tabLogs";
            this.tabLogs.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabLogs.Size = new System.Drawing.Size(395, 808);
            this.tabLogs.TabIndex = 1;
            this.tabLogs.Text = "Logs";
            this.tabLogs.UseVisualStyleBackColor = true;
            // 
            // listBoxLogs
            // 
            this.listBoxLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLogs.FormattingEnabled = true;
            this.listBoxLogs.ItemHeight = 15;
            this.listBoxLogs.Location = new System.Drawing.Point(3, 4);
            this.listBoxLogs.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listBoxLogs.Name = "listBoxLogs";
            this.listBoxLogs.Size = new System.Drawing.Size(608, 1000);
            this.listBoxLogs.TabIndex = 0;
            // 
            // darkTabControl1
            // 
            this.darkTabControl1.Controls.Add(this.tabPage1);
            this.darkTabControl1.Controls.Add(this.tabPage2);
            this.darkTabControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.darkTabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.darkTabControl1.HeaderBackColor = System.Drawing.Color.Black;
            this.darkTabControl1.Location = new System.Drawing.Point(0, 0);
            this.darkTabControl1.Name = "darkTabControl1";
            this.darkTabControl1.SelectedIndex = 0;
            this.darkTabControl1.Size = new System.Drawing.Size(321, 675);
            this.darkTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.darkTabControl1.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(34)))), ((int)(((byte)(40)))));
            this.darkTabControl1.TabBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.darkTabControl1.TabForeColor = System.Drawing.Color.Gainsboro;
            this.darkTabControl1.TabIndex = 2;
            this.darkTabControl1.TabSelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.darkTabControl1.TabSelectedForeColor = System.Drawing.Color.White;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(34)))), ((int)(((byte)(40)))));
            this.tabPage1.ForeColor = System.Drawing.Color.LightGray;
            this.tabPage1.Location = new System.Drawing.Point(4, 30);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(250, 513);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(34)))), ((int)(((byte)(40)))));
            this.tabPage2.ForeColor = System.Drawing.Color.LightGray;
            this.tabPage2.Location = new System.Drawing.Point(4, 30);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(391, 502);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            // 
            // ResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(893, 675);
            this.Controls.Add(this.darkTabControl1);
            this.Controls.Add(this.tabMain);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ResultForm";
            this.Text = "LogForm";
            this.tabMain.ResumeLayout(false);
            this.tabResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.tabLogs.ResumeLayout(false);
            this.darkTabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.TabPage tabLogs;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ComboBox cbResultFilter;
        private System.Windows.Forms.ListBox listBoxLogs;
        private UIControl.DarkTabControl darkTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}