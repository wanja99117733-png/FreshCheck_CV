namespace FreshCheck_CV
{
    partial class DefectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel flowDefectImages;

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
            this.flowDefectImages = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flowDefectImages
            // 
            this.flowDefectImages.AutoScroll = true;
            this.flowDefectImages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowDefectImages.BackColor = System.Drawing.Color.FromArgb(28, 32, 38);
            this.flowDefectImages.Location = new System.Drawing.Point(0, 0);
            this.flowDefectImages.Name = "flowDefectImages";
            this.flowDefectImages.Padding = new System.Windows.Forms.Padding(6);
            this.flowDefectImages.Size = new System.Drawing.Size(400, 200);
            this.flowDefectImages.TabIndex = 0;
            // 
            // DefectForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(400, 200);
            this.Controls.Add(this.flowDefectImages);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "DefectForm";
            this.Text = "Defect Images";
            this.ResumeLayout(false);
        }

        #endregion
    }
}