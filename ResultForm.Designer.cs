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
            this.tabLogs = new System.Windows.Forms.TabPage();
            this.listBoxLogs = new System.Windows.Forms.ListBox();
            this.tabResults = new System.Windows.Forms.TabPage();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.cbResultFilter = new System.Windows.Forms.ComboBox();
            this.tabMain = new FreshCheck_CV.UIControl.DarkTabControl();
            this.tabLogs.SuspendLayout();
            this.tabResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.panelTop.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabLogs
            // 
            this.tabLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(34)))), ((int)(((byte)(40)))));
            this.tabLogs.Controls.Add(this.listBoxLogs);
            this.tabLogs.ForeColor = System.Drawing.Color.LightGray;
            this.tabLogs.Location = new System.Drawing.Point(4, 34);
            this.tabLogs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabLogs.Name = "tabLogs";
            this.tabLogs.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabLogs.Size = new System.Drawing.Size(706, 502);
            this.tabLogs.TabIndex = 1;
            this.tabLogs.Text = "Logs";
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
            this.tabResults.ForeColor = System.Drawing.Color.LightGray;
            this.tabResults.Location = new System.Drawing.Point(4, 34);
            this.tabResults.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabResults.Name = "tabResults";
            this.tabResults.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabResults.Size = new System.Drawing.Size(773, 502);
            this.tabResults.TabIndex = 0;
            this.tabResults.Text = "Results";
            // 
            // dgvResults
            // 
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResults.Location = new System.Drawing.Point(3, 25);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.RowHeadersWidth = 51;
            this.dgvResults.RowTemplate.Height = 23;
            this.dgvResults.Size = new System.Drawing.Size(767, 475);
            this.dgvResults.TabIndex = 6;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnExportCsv);
            this.panelTop.Controls.Add(this.btnClear);
            this.panelTop.Controls.Add(this.cbResultFilter);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(3, 2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(767, 23);
            this.panelTop.TabIndex = 1;
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Location = new System.Drawing.Point(208, -1);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(109, 23);
            this.btnExportCsv.TabIndex = 2;
            this.btnExportCsv.Text = "Export CSV";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(127, -1);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
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
            this.cbResultFilter.Name = "cbResultFilter";
            this.cbResultFilter.Size = new System.Drawing.Size(121, 20);
            this.cbResultFilter.TabIndex = 0;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabResults);
            this.tabMain.Controls.Add(this.tabLogs);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabMain.ItemSize = new System.Drawing.Size(100, 30);
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(781, 540);
            this.tabMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabMain.TabIndex = 2;
            // 
            // ResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 540);
            this.Controls.Add(this.tabMain);
            this.Name = "ResultForm";
            this.Text = "LogForm";
            this.tabLogs.ResumeLayout(false);
            this.tabResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabPage tabLogs;
        private System.Windows.Forms.ListBox listBoxLogs;
        private System.Windows.Forms.TabPage tabResults;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ComboBox cbResultFilter;
        private UIControl.DarkTabControl tabMain;
    }
}