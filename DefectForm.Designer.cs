using System.Drawing;
using System.Windows.Forms;

namespace FreshCheck_CV
{
    partial class DefectForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        

        private FlowLayoutPanel panelDefects;


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelDefects = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // panelDefects
            // 
            this.panelDefects.AutoScroll = true;
            this.panelDefects.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(32)))), ((int)(((byte)(38)))));
            this.panelDefects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDefects.Location = new System.Drawing.Point(0, 0);
            this.panelDefects.Name = "panelDefects";
            this.panelDefects.Padding = new System.Windows.Forms.Padding(10);
            this.panelDefects.Size = new System.Drawing.Size(493, 200);
            this.panelDefects.TabIndex = 0;
            // 
            // DefectForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(32)))), ((int)(((byte)(38)))));
            this.ClientSize = new System.Drawing.Size(493, 200);
            this.Controls.Add(this.panelDefects);
            this.Name = "DefectForm";
            this.Text = "Defect Images";
            this.ResumeLayout(false);

        }

        #endregion
    }
}