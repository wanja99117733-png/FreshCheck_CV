namespace FreshCheck_CV
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImageFolderOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartCycleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopCycleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1261, 33);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.imageOpenToolStripMenuItem,
            this.imageSaveToolStripMenuItem,
            this.ImageFolderOpenToolStripMenuItem,
            this.StartCycleToolStripMenuItem,
            this.StopCycleToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(55, 29);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // imageOpenToolStripMenuItem
            // 
            this.imageOpenToolStripMenuItem.Name = "imageOpenToolStripMenuItem";
            this.imageOpenToolStripMenuItem.Size = new System.Drawing.Size(273, 34);
            this.imageOpenToolStripMenuItem.Text = "Image Open";
            this.imageOpenToolStripMenuItem.Click += new System.EventHandler(this.imageOpenToolStripMenuItem_Click);
            // 
            // imageSaveToolStripMenuItem
            // 
            this.imageSaveToolStripMenuItem.Name = "imageSaveToolStripMenuItem";
            this.imageSaveToolStripMenuItem.Size = new System.Drawing.Size(273, 34);
            this.imageSaveToolStripMenuItem.Text = "Image Save";
            // 
            // ImageFolderOpenToolStripMenuItem
            // 
            this.ImageFolderOpenToolStripMenuItem.Name = "ImageFolderOpenToolStripMenuItem";
            this.ImageFolderOpenToolStripMenuItem.Size = new System.Drawing.Size(273, 34);
            this.ImageFolderOpenToolStripMenuItem.Text = "Image Folder Open";
            this.ImageFolderOpenToolStripMenuItem.Click += new System.EventHandler(this.ImageFolderOpenToolStripMenuItem_Click);
            // 
            // StartCycleToolStripMenuItem
            // 
            this.StartCycleToolStripMenuItem.Name = "StartCycleToolStripMenuItem";
            this.StartCycleToolStripMenuItem.Size = new System.Drawing.Size(273, 34);
            this.StartCycleToolStripMenuItem.Text = "Start Cycle";
            this.StartCycleToolStripMenuItem.Click += new System.EventHandler(this.StartCycleToolStripMenuItem_Click);
            // 
            // StopCycleToolStripMenuItem
            // 
            this.StopCycleToolStripMenuItem.Name = "StopCycleToolStripMenuItem";
            this.StopCycleToolStripMenuItem.Size = new System.Drawing.Size(273, 34);
            this.StopCycleToolStripMenuItem.Text = "Stop Cycle";
            this.StopCycleToolStripMenuItem.Click += new System.EventHandler(this.StopCycleToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(32)))), ((int)(((byte)(38)))));
            this.ClientSize = new System.Drawing.Size(1261, 757);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageSaveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ImageFolderOpenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StartCycleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StopCycleToolStripMenuItem;
    }
}

