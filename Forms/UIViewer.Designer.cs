namespace RatchetEdit
{
    partial class UIViewer
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
            this.components = new System.ComponentModel.Container();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textureView = new System.Windows.Forms.ListView();
            this.textureImages = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.IntegralHeight = false;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(133, 512);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // textureView
            // 
            this.textureView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureView.LargeImageList = this.textureImages;
            this.textureView.Location = new System.Drawing.Point(151, 12);
            this.textureView.Name = "textureView";
            this.textureView.Size = new System.Drawing.Size(508, 512);
            this.textureView.SmallImageList = this.textureImages;
            this.textureView.TabIndex = 1;
            this.textureView.UseCompatibleStateImageBehavior = false;
            this.textureView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textureView_MouseDoubleClick);
            // 
            // textureImages
            // 
            this.textureImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.textureImages.ImageSize = new System.Drawing.Size(64, 64);
            this.textureImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // UIViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 532);
            this.Controls.Add(this.textureView);
            this.Controls.Add(this.listBox1);
            this.Name = "UIViewer";
            this.Text = "UIViewer";
            this.Load += new System.EventHandler(this.UIViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListView textureView;
        private System.Windows.Forms.ImageList textureImages;
    }
}