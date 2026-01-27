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
            this.btnExportcsv = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.cbResultFilter = new System.Windows.Forms.ComboBox();
            this.tabLogs = new System.Windows.Forms.TabPage();
            this.listBoxLogs = new System.Windows.Forms.ListBox();
            this.tabMain.SuspendLayout();
            this.tabResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.panelTop.SuspendLayout();
            this.tabLogs.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLogs
            // 
            this.tabMain.Controls.Add(this.tabResults);
            this.tabMain.Controls.Add(this.tabLogs);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Margin = new System.Windows.Forms.Padding(4);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(1116, 810);
            this.tabMain.TabIndex = 1;
            // 
            // listBoxLogs
            // 
            this.listBoxLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLogs.FormattingEnabled = true;
            this.listBoxLogs.ItemHeight = 12;
            this.listBoxLogs.Location = new System.Drawing.Point(3, 2);
            this.listBoxLogs.Name = "listBoxLogs";
            this.listBoxLogs.Size = new System.Drawing.Size(700, 498);
            this.listBoxLogs.TabIndex = 1;
            // 
            // tabResults
            // 
            this.tabResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(34)))), ((int)(((byte)(40)))));
            this.tabResults.Controls.Add(this.dgvResults);
            this.tabResults.Controls.Add(this.panelTop);
            this.tabResults.Location = new System.Drawing.Point(4, 28);
            this.tabResults.Margin = new System.Windows.Forms.Padding(4);
            this.tabResults.Name = "tabResults";
            this.tabResults.Padding = new System.Windows.Forms.Padding(4);
            this.tabResults.Size = new System.Drawing.Size(1108, 778);
            this.tabResults.TabIndex = 0;
            this.tabResults.Text = "Results";
            // 
            // dgvResults
            // 
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.Location = new System.Drawing.Point(4, 38);
            this.dgvResults.Margin = new System.Windows.Forms.Padding(4);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.RowHeadersWidth = 62;
            this.dgvResults.RowTemplate.Height = 23;
            this.dgvResults.Size = new System.Drawing.Size(1100, 736);
            this.dgvResults.TabIndex = 1;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnExportcsv);
            this.panelTop.Controls.Add(this.btnClear);
            this.panelTop.Controls.Add(this.cbResultFilter);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(4, 4);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1100, 34);
            this.panelTop.TabIndex = 0;
            // 
            // btnExportcsv
            // 
            this.btnExportcsv.Location = new System.Drawing.Point(297, -2);
            this.btnExportcsv.Margin = new System.Windows.Forms.Padding(4);
            this.btnExportcsv.Name = "btnExportcsv";
            this.btnExportcsv.Size = new System.Drawing.Size(156, 34);
            this.btnExportcsv.TabIndex = 2;
            this.btnExportcsv.Text = "Export Xlsx";
            this.btnExportcsv.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(181, -2);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(107, 34);
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
            this.cbResultFilter.Location = new System.Drawing.Point(3, 0);
            this.cbResultFilter.Margin = new System.Windows.Forms.Padding(4);
            this.cbResultFilter.Name = "cbResultFilter";
            this.cbResultFilter.Size = new System.Drawing.Size(171, 26);
            this.cbResultFilter.TabIndex = 0;
            // 
            // tabLogs
            // 
            this.tabLogs.Controls.Add(this.listBoxLogs);
            this.tabLogs.Location = new System.Drawing.Point(4, 28);
            this.tabLogs.Margin = new System.Windows.Forms.Padding(4);
            this.tabLogs.Name = "tabLogs";
            this.tabLogs.Padding = new System.Windows.Forms.Padding(4);
            this.tabLogs.Size = new System.Drawing.Size(1108, 778);
            this.tabLogs.TabIndex = 1;
            this.tabLogs.Text = "Logs";
            this.tabLogs.UseVisualStyleBackColor = true;
            // 
            // listBoxLogs
            // 
            this.listBoxLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLogs.FormattingEnabled = true;
            this.listBoxLogs.ItemHeight = 18;
            this.listBoxLogs.Location = new System.Drawing.Point(4, 4);
            this.listBoxLogs.Margin = new System.Windows.Forms.Padding(4);
            this.listBoxLogs.Name = "listBoxLogs";
            this.listBoxLogs.Size = new System.Drawing.Size(1100, 770);
            this.listBoxLogs.TabIndex = 0;
            // 
            // ResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1116, 810);
            this.Controls.Add(this.tabMain);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ResultForm";
            this.Text = "LogForm";
            this.tabLogs.ResumeLayout(false);
            this.tabResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.tabLogs.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabPage tabLogs;
        private System.Windows.Forms.ListBox listBoxLogs;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnExportcsv;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ComboBox cbResultFilter;
        private UIControl.DarkTabControl tabMain;
    }
}