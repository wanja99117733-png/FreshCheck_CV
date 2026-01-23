namespace FreshCheck_CV
{
    partial class CameraForm
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
            this.components = new System.ComponentModel.Container();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.imageViewCtrl = new FreshCheck_CV.UIControl.ImageViewCtrl();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageViewCtrl
            // 
            this.imageViewCtrl.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.imageViewCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewCtrl.Location = new System.Drawing.Point(0, 0);
            this.imageViewCtrl.Name = "imageViewCtrl";
            this.imageViewCtrl.Size = new System.Drawing.Size(550, 460);
            this.imageViewCtrl.TabIndex = 0;
            // 
            // CameraForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 460);
            this.Controls.Add(this.imageViewCtrl);
            this.Name = "CameraForm";
            this.Text = "CameraForm";
            this.Resize += new System.EventHandler(this.CameraForm_Resize_1);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private UIControl.ImageViewCtrl imageViewCtrl;
    }
}