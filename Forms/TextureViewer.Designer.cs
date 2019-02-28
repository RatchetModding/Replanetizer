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
            this.importBtn = new System.Windows.Forms.Button();
            this.openTextureDialog = new System.Windows.Forms.OpenFileDialog();
            this.texAmountLabel = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.textureImage = new System.Windows.Forms.PictureBox();
            this.textureView = new System.Windows.Forms.ListView();
            this.okBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.textureImage)).BeginInit();
            this.SuspendLayout();
            // 
            // texImages
            // 
            this.texImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.texImages.ImageSize = new System.Drawing.Size(64, 64);
            this.texImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // importBtn
            // 
            this.importBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.importBtn.Location = new System.Drawing.Point(12, 597);
            this.importBtn.Name = "importBtn";
            this.importBtn.Size = new System.Drawing.Size(117, 32);
            this.importBtn.TabIndex = 2;
            this.importBtn.Text = "Import";
            this.importBtn.UseVisualStyleBackColor = true;
            this.importBtn.Click += new System.EventHandler(this.ImportBtn_Click);
            // 
            // openTextureDialog
            // 
            this.openTextureDialog.FileName = "openFileDialog1";
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
            // textureView
            // 
            this.textureView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureView.LargeImageList = this.texImages;
            this.textureView.Location = new System.Drawing.Point(12, 287);
            this.textureView.Name = "textureView";
            this.textureView.Size = new System.Drawing.Size(560, 304);
            this.textureView.TabIndex = 13;
            this.textureView.UseCompatibleStateImageBehavior = false;
            this.textureView.VirtualMode = true;
            this.textureView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.TexListView_RetrieveVirtualItem);
            this.textureView.SelectedIndexChanged += new System.EventHandler(this.TexListView_SelectedIndexChanged);
            this.textureView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CloseButtonClick);
            // 
            // okBtn
            // 
            this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okBtn.Location = new System.Drawing.Point(492, 597);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(80, 32);
            this.okBtn.TabIndex = 14;
            this.okBtn.Text = "Ok";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.CloseButtonClick);
            // 
            // cancelBtn
            // 
            this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelBtn.Location = new System.Drawing.Point(406, 597);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(80, 32);
            this.cancelBtn.TabIndex = 15;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // TextureViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 641);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.textureView);
            this.Controls.Add(this.textureImage);
            this.Controls.Add(this.texAmountLabel);
            this.Controls.Add(this.importBtn);
            this.Name = "TextureViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TextureViewer";
            this.Load += new System.EventHandler(this.TextureViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.textureImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList texImages;
        private System.Windows.Forms.Button importBtn;
        private System.Windows.Forms.OpenFileDialog openTextureDialog;
        private System.Windows.Forms.Label texAmountLabel;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox textureImage;
        private System.Windows.Forms.ListView textureView;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button cancelBtn;
    }
}