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
            this.chkInvert = new System.Windows.Forms.CheckBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnPickColor = new System.Windows.Forms.Button();
            this.lblTargetColor = new System.Windows.Forms.Label();
            this.rangeTrackbar = new FreshCheck_CV.UIControl.RangeTrackbar();
            this.SuspendLayout();
            // 
            // cbMode
            // 
            this.cbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMode.FormattingEnabled = true;
            this.cbMode.Location = new System.Drawing.Point(19, 24);
            this.cbMode.Margin = new System.Windows.Forms.Padding(4);
            this.cbMode.Name = "cbMode";
            this.cbMode.Size = new System.Drawing.Size(313, 26);
            this.cbMode.TabIndex = 1;
            // 
            // chkInvert
            // 
            this.chkInvert.AutoSize = true;
            this.chkInvert.Location = new System.Drawing.Point(48, 214);
            this.chkInvert.Margin = new System.Windows.Forms.Padding(4);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(70, 22);
            this.chkInvert.TabIndex = 3;
            this.chkInvert.Text = "반전";
            this.chkInvert.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(233, 206);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(99, 36);
            this.btnApply.TabIndex = 4;
            this.btnApply.TabStop = false;
            this.btnApply.Text = "적용";
            this.btnApply.UseVisualStyleBackColor = true;
            // 
            // btnPickColor
            // 
            this.btnPickColor.Location = new System.Drawing.Point(233, 141);
            this.btnPickColor.Name = "btnPickColor";
            this.btnPickColor.Size = new System.Drawing.Size(99, 33);
            this.btnPickColor.TabIndex = 6;
            this.btnPickColor.Text = "스포이드";
            this.btnPickColor.UseVisualStyleBackColor = true;
            // 
            // lblTargetColor
            // 
            this.lblTargetColor.AutoSize = true;
            this.lblTargetColor.Location = new System.Drawing.Point(25, 148);
            this.lblTargetColor.Name = "lblTargetColor";
            this.lblTargetColor.Size = new System.Drawing.Size(102, 18);
            this.lblTargetColor.TabIndex = 7;
            this.lblTargetColor.Text = "TargetColor";
            // 
            // rangeTrackbar
            // 
            this.rangeTrackbar.Location = new System.Drawing.Point(19, 67);
            this.rangeTrackbar.Name = "rangeTrackbar";
            this.rangeTrackbar.Size = new System.Drawing.Size(313, 55);
            this.rangeTrackbar.TabIndex = 5;
            this.rangeTrackbar.ValueLeft = 80;
            this.rangeTrackbar.ValueRight = 200;
            // 
            // BinaryProp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblTargetColor);
            this.Controls.Add(this.btnPickColor);
            this.Controls.Add(this.rangeTrackbar);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.chkInvert);
            this.Controls.Add(this.cbMode);
            this.Name = "BinaryProp";
            this.Size = new System.Drawing.Size(354, 392);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbMode;
        private System.Windows.Forms.CheckBox chkInvert;
        private System.Windows.Forms.Button btnApply;
        private UIControl.RangeTrackbar rangeTrackbar;
        private System.Windows.Forms.Button btnPickColor;
        private System.Windows.Forms.Label lblTargetColor;
    }
}
