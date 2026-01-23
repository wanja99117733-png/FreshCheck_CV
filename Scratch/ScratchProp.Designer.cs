namespace FreshCheck_CV.Scratch
{
    partial class ScratchProp
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
            this.btnEraseBg = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnEraseBg
            // 
            this.btnEraseBg.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnEraseBg.Font = new System.Drawing.Font("굴림", 8F);
            this.btnEraseBg.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnEraseBg.Location = new System.Drawing.Point(14, 13);
            this.btnEraseBg.Name = "btnEraseBg";
            this.btnEraseBg.Size = new System.Drawing.Size(172, 23);
            this.btnEraseBg.TabIndex = 5;
            this.btnEraseBg.Text = "배경 삭제";
            this.btnEraseBg.UseVisualStyleBackColor = false;
            this.btnEraseBg.Click += new System.EventHandler(this.btnEraseBg_Click);
            // 
            // ScratchProp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnEraseBg);
            this.Name = "ScratchProp";
            this.Size = new System.Drawing.Size(199, 272);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnEraseBg;
    }
}
