namespace FreshCheck_CV
{
    partial class DefectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel panleDefects;

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
            this.panleDefects = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // panleDefects
            // 
            this.panleDefects.AutoScroll = true;
            this.panleDefects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(32)))), ((int)(((byte)(38)))));
            this.panleDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panleDefects.Location = new System.Drawing.Point(0, 0);
            this.panleDefects.Name = "panleDefects";
            this.panleDefects.Padding = new System.Windows.Forms.Padding(6);
            this.panleDefects.Size = new System.Drawing.Size(497, 200);
            this.panleDefects.TabIndex = 0;
            // 
            // DefectForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(497, 200);
            this.Controls.Add(this.panleDefects);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "DefectForm";
            this.Text = "Defect Images";
            this.ResumeLayout(false);

        }

        #endregion
    }
}