namespace RatchetEdit
{
    partial class ModelViewer
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
            this.modelView = new System.Windows.Forms.TreeView();
            this.vertCountLabel = new System.Windows.Forms.Label();
            this.vertCountBox = new System.Windows.Forms.TextBox();
            this.saveObjBtn = new System.Windows.Forms.Button();
            this.faceCountLabel = new System.Windows.Forms.Label();
            this.faceCountBox = new System.Windows.Forms.TextBox();
            this.IDLabel = new System.Windows.Forms.Label();
            this.IDBox = new System.Windows.Forms.TextBox();
            this.sizeLabel = new System.Windows.Forms.Label();
            this.sizeBox = new System.Windows.Forms.TextBox();
            this.texCountLabel = new System.Windows.Forms.Label();
            this.texCountBox = new System.Windows.Forms.TextBox();
            this.LoadObjBtn = new System.Windows.Forms.Button();
            this.glControl1 = new OpenTK.GLControl();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // modelView
            // 
            this.modelView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.modelView.Location = new System.Drawing.Point(12, 12);
            this.modelView.Name = "modelView";
            this.modelView.Size = new System.Drawing.Size(157, 599);
            this.modelView.TabIndex = 0;
            this.modelView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.modelView_AfterSelect);
            // 
            // vertCountLabel
            // 
            this.vertCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.vertCountLabel.AutoSize = true;
            this.vertCountLabel.Location = new System.Drawing.Point(257, 575);
            this.vertCountLabel.Name = "vertCountLabel";
            this.vertCountLabel.Size = new System.Drawing.Size(48, 13);
            this.vertCountLabel.TabIndex = 1;
            this.vertCountLabel.Text = "Vertices:";
            // 
            // vertCountBox
            // 
            this.vertCountBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.vertCountBox.Location = new System.Drawing.Point(260, 591);
            this.vertCountBox.Name = "vertCountBox";
            this.vertCountBox.Size = new System.Drawing.Size(80, 20);
            this.vertCountBox.TabIndex = 2;
            // 
            // saveObjBtn
            // 
            this.saveObjBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveObjBtn.Location = new System.Drawing.Point(698, 578);
            this.saveObjBtn.Name = "saveObjBtn";
            this.saveObjBtn.Size = new System.Drawing.Size(81, 33);
            this.saveObjBtn.TabIndex = 4;
            this.saveObjBtn.Text = "Save to .obj";
            this.saveObjBtn.UseVisualStyleBackColor = true;
            // 
            // faceCountLabel
            // 
            this.faceCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.faceCountLabel.AutoSize = true;
            this.faceCountLabel.Location = new System.Drawing.Point(343, 575);
            this.faceCountLabel.Name = "faceCountLabel";
            this.faceCountLabel.Size = new System.Drawing.Size(39, 13);
            this.faceCountLabel.TabIndex = 1;
            this.faceCountLabel.Text = "Faces:";
            // 
            // faceCountBox
            // 
            this.faceCountBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.faceCountBox.Location = new System.Drawing.Point(346, 591);
            this.faceCountBox.Name = "faceCountBox";
            this.faceCountBox.Size = new System.Drawing.Size(80, 20);
            this.faceCountBox.TabIndex = 2;
            // 
            // IDLabel
            // 
            this.IDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.IDLabel.AutoSize = true;
            this.IDLabel.Location = new System.Drawing.Point(174, 575);
            this.IDLabel.Name = "IDLabel";
            this.IDLabel.Size = new System.Drawing.Size(53, 13);
            this.IDLabel.TabIndex = 1;
            this.IDLabel.Text = "Model ID:";
            // 
            // IDBox
            // 
            this.IDBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.IDBox.Location = new System.Drawing.Point(174, 591);
            this.IDBox.Name = "IDBox";
            this.IDBox.Size = new System.Drawing.Size(80, 20);
            this.IDBox.TabIndex = 2;
            // 
            // sizeLabel
            // 
            this.sizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sizeLabel.AutoSize = true;
            this.sizeLabel.Location = new System.Drawing.Point(429, 575);
            this.sizeLabel.Name = "sizeLabel";
            this.sizeLabel.Size = new System.Drawing.Size(76, 13);
            this.sizeLabel.TabIndex = 1;
            this.sizeLabel.Text = "Standard Size:";
            // 
            // sizeBox
            // 
            this.sizeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sizeBox.Location = new System.Drawing.Point(432, 591);
            this.sizeBox.Name = "sizeBox";
            this.sizeBox.Size = new System.Drawing.Size(80, 20);
            this.sizeBox.TabIndex = 2;
            // 
            // texCountLabel
            // 
            this.texCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.texCountLabel.AutoSize = true;
            this.texCountLabel.Location = new System.Drawing.Point(515, 575);
            this.texCountLabel.Name = "texCountLabel";
            this.texCountLabel.Size = new System.Drawing.Size(77, 13);
            this.texCountLabel.TabIndex = 1;
            this.texCountLabel.Text = "Texture Count:";
            // 
            // texCountBox
            // 
            this.texCountBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.texCountBox.Location = new System.Drawing.Point(518, 591);
            this.texCountBox.Name = "texCountBox";
            this.texCountBox.Size = new System.Drawing.Size(80, 20);
            this.texCountBox.TabIndex = 2;
            // 
            // LoadObjBtn
            // 
            this.LoadObjBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadObjBtn.Location = new System.Drawing.Point(604, 578);
            this.LoadObjBtn.Name = "LoadObjBtn";
            this.LoadObjBtn.Size = new System.Drawing.Size(88, 33);
            this.LoadObjBtn.TabIndex = 5;
            this.LoadObjBtn.Text = "Load From .obj";
            this.LoadObjBtn.UseVisualStyleBackColor = true;
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(175, 12);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(604, 560);
            this.glControl1.TabIndex = 6;
            this.glControl1.VSync = false;
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            this.glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseWheel);
            // 
            // tickTimer
            // 
            this.tickTimer.Enabled = true;
            this.tickTimer.Interval = 16;
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 623);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.LoadObjBtn);
            this.Controls.Add(this.saveObjBtn);
            this.Controls.Add(this.IDBox);
            this.Controls.Add(this.texCountBox);
            this.Controls.Add(this.sizeBox);
            this.Controls.Add(this.texCountLabel);
            this.Controls.Add(this.faceCountBox);
            this.Controls.Add(this.IDLabel);
            this.Controls.Add(this.sizeLabel);
            this.Controls.Add(this.vertCountBox);
            this.Controls.Add(this.faceCountLabel);
            this.Controls.Add(this.vertCountLabel);
            this.Controls.Add(this.modelView);
            this.Name = "ModelViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ModelViewer";
            this.Load += new System.EventHandler(this.ModelViewer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView modelView;
        private System.Windows.Forms.Label vertCountLabel;
        private System.Windows.Forms.TextBox vertCountBox;
        private System.Windows.Forms.Button saveObjBtn;
        private System.Windows.Forms.Label faceCountLabel;
        private System.Windows.Forms.TextBox faceCountBox;
        private System.Windows.Forms.Label IDLabel;
        private System.Windows.Forms.TextBox IDBox;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.TextBox sizeBox;
        private System.Windows.Forms.Label texCountLabel;
        private System.Windows.Forms.TextBox texCountBox;
        private System.Windows.Forms.Button LoadObjBtn;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Timer tickTimer;
    }
}