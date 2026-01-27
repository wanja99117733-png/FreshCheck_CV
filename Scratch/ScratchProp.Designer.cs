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
            this.btnScratchDet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnEraseBg
            // 
            this.btnEraseBg.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnEraseBg.Font = new System.Drawing.Font("굴림", 8F);
            this.btnEraseBg.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnEraseBg.Location = new System.Drawing.Point(16, 16);
            this.btnEraseBg.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnEraseBg.Name = "btnEraseBg";
            this.btnEraseBg.Size = new System.Drawing.Size(197, 29);
            this.btnEraseBg.TabIndex = 5;
            this.btnEraseBg.Text = "배경 삭제";
            this.btnEraseBg.UseVisualStyleBackColor = false;
            this.btnEraseBg.Click += new System.EventHandler(this.btnEraseBg_Click);
            // 
            // btnScratchDet
            // 
            this.btnScratchDet.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnScratchDet.Font = new System.Drawing.Font("굴림", 8F);
            this.btnScratchDet.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnScratchDet.Location = new System.Drawing.Point(16, 53);
            this.btnScratchDet.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnScratchDet.Name = "btnScratchDet";
            this.btnScratchDet.Size = new System.Drawing.Size(197, 29);
            this.btnScratchDet.TabIndex = 6;
            this.btnScratchDet.Text = "스크래치 검출";
            this.btnScratchDet.UseVisualStyleBackColor = false;
            this.btnScratchDet.Click += new System.EventHandler(this.btnScratchDet_Click);
            // 
            // ScratchProp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnScratchDet);
            this.Controls.Add(this.btnEraseBg);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ScratchProp";
            this.Size = new System.Drawing.Size(227, 340);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnEraseBg;
        private System.Windows.Forms.Button btnScratchDet;
    }
}
