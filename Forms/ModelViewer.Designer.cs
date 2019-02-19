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
            this.saveObjBtn = new System.Windows.Forms.Button();
            this.LoadObjBtn = new System.Windows.Forms.Button();
            this.glControl1 = new OpenTK.GLControl();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.objSave = new System.Windows.Forms.SaveFileDialog();
            this.objOpen = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // modelView
            // 
            this.modelView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.modelView.Location = new System.Drawing.Point(12, 12);
            this.modelView.Name = "modelView";
            this.modelView.Size = new System.Drawing.Size(157, 554);
            this.modelView.TabIndex = 0;
            this.modelView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.modelView_AfterSelect);
            // 
            // saveObjBtn
            // 
            this.saveObjBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveObjBtn.Location = new System.Drawing.Point(874, 533);
            this.saveObjBtn.Name = "saveObjBtn";
            this.saveObjBtn.Size = new System.Drawing.Size(83, 33);
            this.saveObjBtn.TabIndex = 4;
            this.saveObjBtn.Text = "Save to .obj";
            this.saveObjBtn.UseVisualStyleBackColor = true;
            this.saveObjBtn.Click += new System.EventHandler(this.saveObjBtn_Click);
            // 
            // LoadObjBtn
            // 
            this.LoadObjBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadObjBtn.Location = new System.Drawing.Point(780, 533);
            this.LoadObjBtn.Name = "LoadObjBtn";
            this.LoadObjBtn.Size = new System.Drawing.Size(88, 33);
            this.LoadObjBtn.TabIndex = 5;
            this.LoadObjBtn.Text = "Load From .obj";
            this.LoadObjBtn.UseVisualStyleBackColor = true;
            this.LoadObjBtn.Click += new System.EventHandler(this.LoadObjBtn_Click);
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(175, 12);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(599, 554);
            this.glControl1.TabIndex = 6;
            this.glControl1.VSync = false;
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            this.glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseWheel);
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // tickTimer
            // 
            this.tickTimer.Enabled = true;
            this.tickTimer.Interval = 16;
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(780, 12);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(177, 515);
            this.propertyGrid1.TabIndex = 7;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // objSave
            // 
            this.objSave.Filter = "OBJ file|.obj";
            // 
            // objOpen
            // 
            this.objOpen.FileName = "openFileDialog1";
            this.objOpen.Filter = "obj files (*.obj)|*.obj";
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(969, 578);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.LoadObjBtn);
            this.Controls.Add(this.saveObjBtn);
            this.Controls.Add(this.modelView);
            this.Name = "ModelViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ModelViewer";
            this.Load += new System.EventHandler(this.ModelViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView modelView;
        private System.Windows.Forms.Button saveObjBtn;
        private System.Windows.Forms.Button LoadObjBtn;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Timer tickTimer;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.SaveFileDialog objSave;
        private System.Windows.Forms.OpenFileDialog objOpen;
    }
}