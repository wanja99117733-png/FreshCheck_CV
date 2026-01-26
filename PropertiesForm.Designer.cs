namespace FreshCheck_CV
{
    partial class PropertiesForm
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
            this.tabPropControl = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // tabPropControl
            // 
            this.tabPropControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabPropControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPropControl.ItemSize = new System.Drawing.Size(0, 1);
            this.tabPropControl.Location = new System.Drawing.Point(0, 0);
            this.tabPropControl.Name = "tabPropControl";
            this.tabPropControl.SelectedIndex = 0;
            this.tabPropControl.Size = new System.Drawing.Size(1278, 723);
            this.tabPropControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabPropControl.TabIndex = 0;
            // 
            // PropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(1278, 723);
            this.Controls.Add(this.tabPropControl);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Name = "PropertiesForm";
            this.Text = "PropertiesForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabPropControl;
    }
}