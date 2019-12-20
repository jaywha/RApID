namespace RApID_Project_WPF.CustomControls
{
    partial class AssemblyLinkLabel
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblECO = new System.Windows.Forms.Label();
            this.lblREV = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblECO
            // 
            this.lblECO.AutoSize = true;
            this.lblECO.CausesValidation = false;
            this.lblECO.Location = new System.Drawing.Point(0, 0);
            this.lblECO.Name = "lblECO";
            this.lblECO.Size = new System.Drawing.Size(100, 23);
            this.lblECO.TabIndex = 0;
            this.lblECO.Visible = false;
            // 
            // lblREV
            // 
            this.lblREV.AutoSize = true;
            this.lblREV.Location = new System.Drawing.Point(0, 0);
            this.lblREV.Name = "lblREV";
            this.lblREV.Size = new System.Drawing.Size(100, 23);
            this.lblREV.TabIndex = 0;
            this.lblREV.Visible = false;
            // 
            // AssemblyLinkLabel
            // 
            this.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.LinkColor = System.Drawing.Color.Cyan;
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label lblECO;
        public System.Windows.Forms.Label lblREV;
    }
}
