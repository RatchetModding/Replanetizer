namespace RatchetEdit
{
    partial class TextureViewer
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
            this.texImages = new System.Windows.Forms.ImageList(this.components);
            this.ImportBtn = new System.Windows.Forms.Button();
            this.addTextureDialog = new System.Windows.Forms.OpenFileDialog();
            this.texAmountLabel = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.textureImage = new System.Windows.Forms.PictureBox();
            this.texListView = new System.Windows.Forms.ListView();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.textureImage)).BeginInit();
            this.SuspendLayout();
            // 
            // texImages
            // 
            this.texImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.texImages.ImageSize = new System.Drawing.Size(64, 64);
            this.texImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ImportBtn
            // 
            this.ImportBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ImportBtn.Location = new System.Drawing.Point(12, 597);
            this.ImportBtn.Name = "ImportBtn";
            this.ImportBtn.Size = new System.Drawing.Size(117, 32);
            this.ImportBtn.TabIndex = 2;
            this.ImportBtn.Text = "Import";
            this.ImportBtn.UseVisualStyleBackColor = true;
            this.ImportBtn.Click += new System.EventHandler(this.ImportBtn_Click);
            // 
            // addTextureDialog
            // 
            this.addTextureDialog.FileName = "openFileDialog1";
            // 
            // texAmountLabel
            // 
            this.texAmountLabel.AutoSize = true;
            this.texAmountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.texAmountLabel.Location = new System.Drawing.Point(12, 9);
            this.texAmountLabel.Name = "texAmountLabel";
            this.texAmountLabel.Size = new System.Drawing.Size(89, 13);
            this.texAmountLabel.TabIndex = 4;
            this.texAmountLabel.Text = "Texture Count: -1";
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // textureImage
            // 
            this.textureImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureImage.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.textureImage.Location = new System.Drawing.Point(12, 25);
            this.textureImage.Name = "textureImage";
            this.textureImage.Size = new System.Drawing.Size(560, 256);
            this.textureImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.textureImage.TabIndex = 12;
            this.textureImage.TabStop = false;
            // 
            // texListView
            // 
            this.texListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.texListView.LargeImageList = this.texImages;
            this.texListView.Location = new System.Drawing.Point(12, 287);
            this.texListView.Name = "texListView";
            this.texListView.Size = new System.Drawing.Size(560, 304);
            this.texListView.TabIndex = 13;
            this.texListView.UseCompatibleStateImageBehavior = false;
            this.texListView.VirtualMode = true;
            this.texListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.texListView_RetrieveVirtualItem);
            this.texListView.SelectedIndexChanged += new System.EventHandler(this.texListView_SelectedIndexChanged);
            this.texListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.texListView_MouseDoubleClick);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(492, 597);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 32);
            this.button1.TabIndex = 14;
            this.button1.Text = "Ok";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(406, 597);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(80, 32);
            this.button2.TabIndex = 15;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // TextureViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 641);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.texListView);
            this.Controls.Add(this.textureImage);
            this.Controls.Add(this.texAmountLabel);
            this.Controls.Add(this.ImportBtn);
            this.Name = "TextureViewer";
            this.Text = "TextureViewer";
            this.Load += new System.EventHandler(this.TextureViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.textureImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList texImages;
        private System.Windows.Forms.Button ImportBtn;
        private System.Windows.Forms.OpenFileDialog addTextureDialog;
        private System.Windows.Forms.Label texAmountLabel;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox textureImage;
        private System.Windows.Forms.ListView texListView;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}