namespace FreshCheck_CV.Property
{
    partial class BinaryProp
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

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbMode = new System.Windows.Forms.ComboBox();
            this.tbThreshold = new System.Windows.Forms.TrackBar();
            this.chkInvert = new System.Windows.Forms.CheckBox();
            this.btnApply = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // cbMode
            // 
            this.cbMode.FormattingEnabled = true;
            this.cbMode.Location = new System.Drawing.Point(13, 16);
            this.cbMode.Name = "cbMode";
            this.cbMode.Size = new System.Drawing.Size(159, 20);
            this.cbMode.TabIndex = 1;
            // 
            // tbThreshold
            // 
            this.tbThreshold.Location = new System.Drawing.Point(13, 43);
            this.tbThreshold.Maximum = 255;
            this.tbThreshold.Name = "tbThreshold";
            this.tbThreshold.Size = new System.Drawing.Size(159, 45);
            this.tbThreshold.TabIndex = 2;
            this.tbThreshold.Value = 128;
            // 
            // chkInvert
            // 
            this.chkInvert.AutoSize = true;
            this.chkInvert.Location = new System.Drawing.Point(13, 94);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(48, 16);
            this.chkInvert.TabIndex = 3;
            this.chkInvert.Text = "반전";
            this.chkInvert.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(111, 86);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(61, 24);
            this.btnApply.TabIndex = 4;
            this.btnApply.TabStop = false;
            this.btnApply.Text = "적용";
            this.btnApply.UseVisualStyleBackColor = true;
            // 
            // BinaryProp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.chkInvert);
            this.Controls.Add(this.tbThreshold);
            this.Controls.Add(this.cbMode);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "BinaryProp";
            this.Size = new System.Drawing.Size(187, 128);
            this.Load += new System.EventHandler(this.BinaryProp_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tbThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbMode;
        private System.Windows.Forms.TrackBar tbThreshold;
        private System.Windows.Forms.CheckBox chkInvert;
        private System.Windows.Forms.Button btnApply;
    }
}
