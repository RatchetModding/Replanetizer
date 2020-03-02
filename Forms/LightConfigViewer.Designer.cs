namespace RatchetEdit.Forms
{
    partial class LightConfigViewer
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
            this.properties = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // properties
            // 
            this.properties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.properties.HelpVisible = false;
            this.properties.Location = new System.Drawing.Point(0, 0);
            this.properties.Name = "properties";
            this.properties.Size = new System.Drawing.Size(327, 404);
            this.properties.TabIndex = 20;
            this.properties.ToolbarVisible = false;
            // 
            // LightConfigViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 404);
            this.Controls.Add(this.properties);
            this.Name = "LightConfigViewer";
            this.Text = "LightConfigViewer";
            this.Load += new System.EventHandler(this.LightConfigViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid properties;
    }
}