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
            this.glControl = new OpenTK.GLControl();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.modelProperties = new System.Windows.Forms.PropertyGrid();
            this.modelSave = new System.Windows.Forms.SaveFileDialog();
            this.objOpen = new System.Windows.Forms.OpenFileDialog();
            this.textureView = new System.Windows.Forms.ListView();
            this.textureList = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromobjToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // modelView
            // 
            this.modelView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.modelView.Location = new System.Drawing.Point(12, 27);
            this.modelView.Name = "modelView";
            this.modelView.Size = new System.Drawing.Size(157, 539);
            this.modelView.TabIndex = 0;
            this.modelView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.modelView_AfterSelect);
            // 
            // glControl
            // 
            this.glControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Location = new System.Drawing.Point(175, 27);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(774, 539);
            this.glControl.TabIndex = 6;
            this.glControl.VSync = false;
            this.glControl.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            this.glControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseWheel);
            this.glControl.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // tickTimer
            // 
            this.tickTimer.Enabled = true;
            this.tickTimer.Interval = 16;
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
            // 
            // modelProperties
            // 
            this.modelProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelProperties.HelpVisible = false;
            this.modelProperties.Location = new System.Drawing.Point(955, 254);
            this.modelProperties.Name = "modelProperties";
            this.modelProperties.Size = new System.Drawing.Size(192, 312);
            this.modelProperties.TabIndex = 7;
            this.modelProperties.ToolbarVisible = false;
            // 
            // modelSave
            // 
            this.modelSave.Filter = "Inter-Quake Model (*.iqe)|*.iqe|OBJ file|.obj";
            // 
            // objOpen
            // 
            this.objOpen.FileName = "openFileDialog1";
            this.objOpen.Filter = "obj files (*.obj)|*.obj";
            // 
            // textureView
            // 
            this.textureView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textureView.LargeImageList = this.textureList;
            this.textureView.Location = new System.Drawing.Point(955, 27);
            this.textureView.Name = "textureView";
            this.textureView.Size = new System.Drawing.Size(192, 221);
            this.textureView.SmallImageList = this.textureList;
            this.textureView.TabIndex = 10;
            this.textureView.UseCompatibleStateImageBehavior = false;
            this.textureView.DoubleClick += new System.EventHandler(this.textureView_DoubleClick);
            // 
            // textureList
            // 
            this.textureList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.textureList.ImageSize = new System.Drawing.Size(32, 32);
            this.textureList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1159, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.importFromobjToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // importFromobjToolStripMenuItem
            // 
            this.importFromobjToolStripMenuItem.Name = "importFromobjToolStripMenuItem";
            this.importFromobjToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.importFromobjToolStripMenuItem.Text = "Import model";
            this.importFromobjToolStripMenuItem.Click += new System.EventHandler(this.importFromobjToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Export model";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1159, 578);
            this.Controls.Add(this.textureView);
            this.Controls.Add(this.modelProperties);
            this.Controls.Add(this.glControl);
            this.Controls.Add(this.modelView);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ModelViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ModelViewer";
            this.Load += new System.EventHandler(this.ModelViewer_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView modelView;
        private OpenTK.GLControl glControl;
        private System.Windows.Forms.Timer tickTimer;
        private System.Windows.Forms.PropertyGrid modelProperties;
        private System.Windows.Forms.SaveFileDialog modelSave;
        private System.Windows.Forms.OpenFileDialog objOpen;
        private System.Windows.Forms.ListView textureView;
        private System.Windows.Forms.ImageList textureList;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFromobjToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}