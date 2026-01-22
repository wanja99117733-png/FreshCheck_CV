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
            this.colorRuleList = new FreshCheck_CV.Scratch.ColorRuleList();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnEraseBg = new System.Windows.Forms.Button();
            this.btnTemp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // colorRuleList
            // 
            this.colorRuleList.BackColor = System.Drawing.Color.White;
            this.colorRuleList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.colorRuleList.Location = new System.Drawing.Point(14, 108);
            this.colorRuleList.Name = "colorRuleList";
            this.colorRuleList.Size = new System.Drawing.Size(172, 150);
            this.colorRuleList.TabIndex = 0;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(14, 72);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(44, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "+";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(68, 72);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(44, 23);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "-";
            this.btnRemove.UseVisualStyleBackColor = true;
            // 
            // btnEraseBg
            // 
            this.btnEraseBg.Font = new System.Drawing.Font("굴림", 8F);
            this.btnEraseBg.Location = new System.Drawing.Point(126, 72);
            this.btnEraseBg.Name = "btnEraseBg";
            this.btnEraseBg.Size = new System.Drawing.Size(60, 23);
            this.btnEraseBg.TabIndex = 3;
            this.btnEraseBg.Text = "배경삭제";
            this.btnEraseBg.UseVisualStyleBackColor = true;
            // 
            // btnTemp
            // 
            this.btnTemp.Font = new System.Drawing.Font("굴림", 8F);
            this.btnTemp.Location = new System.Drawing.Point(14, 14);
            this.btnTemp.Name = "btnTemp";
            this.btnTemp.Size = new System.Drawing.Size(172, 23);
            this.btnTemp.TabIndex = 4;
            this.btnTemp.Text = "임시 배경 삭제";
            this.btnTemp.UseVisualStyleBackColor = true;
            this.btnTemp.Click += new System.EventHandler(this.btnTemp_Click);
            // 
            // ScratchProp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnTemp);
            this.Controls.Add(this.btnEraseBg);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.colorRuleList);
            this.Name = "ScratchProp";
            this.Size = new System.Drawing.Size(199, 272);
            this.ResumeLayout(false);

        }

        #endregion

        private ColorRuleList colorRuleList;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnEraseBg;
        private System.Windows.Forms.Button btnTemp;
    }
}
