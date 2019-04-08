namespace RatchetEdit
{
    partial class Main
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.mapOpenBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.mapSaveBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.mapSaveAsBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewToolStipItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mobyCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.tieCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.shrubCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.collCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.terrainCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.splineCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.skyboxCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.cuboidCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.type0CCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.modelViewerToolBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.spriteViewerToolBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.UISpriteToolBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
            this.levelVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.glControl = new RatchetEdit.CustomGLControl();
            this.toolstrip1 = new System.Windows.Forms.ToolStrip();
            this.cloneBtn = new System.Windows.Forms.ToolStripButton();
            this.deleteBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.translateToolBtn = new System.Windows.Forms.ToolStripButton();
            this.rotateToolBtn = new System.Windows.Forms.ToolStripButton();
            this.scaleToolBtn = new System.Windows.Forms.ToolStripButton();
            this.splineToolBtn = new System.Windows.Forms.ToolStripButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.objectTree = new RatchetEdit.ObjectTreeView();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.properties = new System.Windows.Forms.PropertyGrid();
            this.camYLabel = new System.Windows.Forms.Label();
            this.yawLabel = new System.Windows.Forms.Label();
            this.camXLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.camZLabel = new System.Windows.Forms.Label();
            this.pitchLabel = new System.Windows.Forms.Label();
            this.mapSaveDialog = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolstrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.ViewToolStipItem,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1269, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem4,
            this.mapOpenBtn,
            this.mapSaveBtn,
            this.mapSaveAsBtn,
            this.toolStripSeparator1,
            this.exitToolBtn});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItem1.Text = "&File";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Enabled = false;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(155, 22);
            this.toolStripMenuItem4.Text = "New";
            // 
            // mapOpenBtn
            // 
            this.mapOpenBtn.Name = "mapOpenBtn";
            this.mapOpenBtn.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mapOpenBtn.Size = new System.Drawing.Size(155, 22);
            this.mapOpenBtn.Text = "Open...";
            this.mapOpenBtn.Click += new System.EventHandler(this.mapOpenBtn_Click);
            // 
            // mapSaveBtn
            // 
            this.mapSaveBtn.Name = "mapSaveBtn";
            this.mapSaveBtn.Size = new System.Drawing.Size(155, 22);
            this.mapSaveBtn.Text = "Save";
            // 
            // mapSaveAsBtn
            // 
            this.mapSaveAsBtn.Name = "mapSaveAsBtn";
            this.mapSaveAsBtn.Size = new System.Drawing.Size(155, 22);
            this.mapSaveAsBtn.Text = "Save as...";
            this.mapSaveAsBtn.Click += new System.EventHandler(this.mapSaveAsBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // exitToolBtn
            // 
            this.exitToolBtn.Name = "exitToolBtn";
            this.exitToolBtn.Size = new System.Drawing.Size(155, 22);
            this.exitToolBtn.Text = "Exit";
            this.exitToolBtn.Click += new System.EventHandler(this.exitToolBtn_Click);
            // 
            // ViewToolStipItem
            // 
            this.ViewToolStipItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mobyCheck,
            this.tieCheck,
            this.shrubCheck,
            this.collCheck,
            this.terrainCheck,
            this.splineCheck,
            this.skyboxCheck,
            this.cuboidCheck,
            this.type0CCheck});
            this.ViewToolStipItem.Name = "ViewToolStipItem";
            this.ViewToolStipItem.Size = new System.Drawing.Size(44, 20);
            this.ViewToolStipItem.Text = "View";
            // 
            // mobyCheck
            // 
            this.mobyCheck.Checked = true;
            this.mobyCheck.CheckOnClick = true;
            this.mobyCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mobyCheck.Enabled = false;
            this.mobyCheck.Name = "mobyCheck";
            this.mobyCheck.Size = new System.Drawing.Size(120, 22);
            this.mobyCheck.Text = "Mobys";
            this.mobyCheck.CheckedChanged += new System.EventHandler(this.mobyCheck_CheckedChanged);
            // 
            // tieCheck
            // 
            this.tieCheck.Checked = true;
            this.tieCheck.CheckOnClick = true;
            this.tieCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tieCheck.Enabled = false;
            this.tieCheck.Name = "tieCheck";
            this.tieCheck.Size = new System.Drawing.Size(120, 22);
            this.tieCheck.Text = "Ties";
            this.tieCheck.CheckedChanged += new System.EventHandler(this.tieCheck_CheckedChanged);
            // 
            // shrubCheck
            // 
            this.shrubCheck.Checked = true;
            this.shrubCheck.CheckOnClick = true;
            this.shrubCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.shrubCheck.Enabled = false;
            this.shrubCheck.Name = "shrubCheck";
            this.shrubCheck.Size = new System.Drawing.Size(120, 22);
            this.shrubCheck.Text = "Shrubs";
            this.shrubCheck.CheckedChanged += new System.EventHandler(this.shrubCheck_CheckedChanged);
            // 
            // collCheck
            // 
            this.collCheck.CheckOnClick = true;
            this.collCheck.Enabled = false;
            this.collCheck.Name = "collCheck";
            this.collCheck.Size = new System.Drawing.Size(120, 22);
            this.collCheck.Text = "Collision";
            this.collCheck.CheckedChanged += new System.EventHandler(this.collCheck_CheckedChanged);
            // 
            // terrainCheck
            // 
            this.terrainCheck.Checked = true;
            this.terrainCheck.CheckOnClick = true;
            this.terrainCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.terrainCheck.Enabled = false;
            this.terrainCheck.Name = "terrainCheck";
            this.terrainCheck.Size = new System.Drawing.Size(120, 22);
            this.terrainCheck.Text = "Terrain";
            this.terrainCheck.CheckedChanged += new System.EventHandler(this.terrainCheck_CheckedChanged);
            // 
            // splineCheck
            // 
            this.splineCheck.CheckOnClick = true;
            this.splineCheck.Enabled = false;
            this.splineCheck.Name = "splineCheck";
            this.splineCheck.Size = new System.Drawing.Size(120, 22);
            this.splineCheck.Text = "Splines";
            this.splineCheck.CheckedChanged += new System.EventHandler(this.splineCheck_CheckedChanged);
            // 
            // skyboxCheck
            // 
            this.skyboxCheck.CheckOnClick = true;
            this.skyboxCheck.Enabled = false;
            this.skyboxCheck.Name = "skyboxCheck";
            this.skyboxCheck.Size = new System.Drawing.Size(120, 22);
            this.skyboxCheck.Text = "Skybox";
            this.skyboxCheck.CheckedChanged += new System.EventHandler(this.skyboxCheck_CheckedChanged);
            // 
            // cuboidCheck
            // 
            this.cuboidCheck.CheckOnClick = true;
            this.cuboidCheck.Enabled = false;
            this.cuboidCheck.Name = "cuboidCheck";
            this.cuboidCheck.Size = new System.Drawing.Size(120, 22);
            this.cuboidCheck.Text = "Cuboids";
            this.cuboidCheck.CheckedChanged += new System.EventHandler(this.cuboidCheck_CheckedChanged);
            // 
            // type0CCheck
            // 
            this.type0CCheck.CheckOnClick = true;
            this.type0CCheck.Enabled = false;
            this.type0CCheck.Name = "type0CCheck";
            this.type0CCheck.Size = new System.Drawing.Size(120, 22);
            this.type0CCheck.Text = "Type0Cs";
            this.type0CCheck.CheckedChanged += new System.EventHandler(this.type0CCheck_CheckedChanged);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modelViewerToolBtn,
            this.toolStripMenuItem10,
            this.toolStripMenuItem11,
            this.spriteViewerToolBtn,
            this.UISpriteToolBtn,
            this.toolStripMenuItem14,
            this.levelVariablesToolStripMenuItem});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(63, 20);
            this.toolStripMenuItem2.Text = "Window";
            // 
            // modelViewerToolBtn
            // 
            this.modelViewerToolBtn.Name = "modelViewerToolBtn";
            this.modelViewerToolBtn.Size = new System.Drawing.Size(174, 22);
            this.modelViewerToolBtn.Text = "Model Viewer";
            this.modelViewerToolBtn.Click += new System.EventHandler(this.modelViewerToolBtn_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(174, 22);
            this.toolStripMenuItem10.Text = "Level object viewer";
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(174, 22);
            this.toolStripMenuItem11.Text = "Textures";
            this.toolStripMenuItem11.Click += new System.EventHandler(this.toolStripMenuItem11_Click);
            // 
            // spriteViewerToolBtn
            // 
            this.spriteViewerToolBtn.Name = "spriteViewerToolBtn";
            this.spriteViewerToolBtn.Size = new System.Drawing.Size(174, 22);
            this.spriteViewerToolBtn.Text = "Sprites";
            this.spriteViewerToolBtn.Click += new System.EventHandler(this.spriteViewerToolBtn_Click);
            // 
            // UISpriteToolBtn
            // 
            this.UISpriteToolBtn.Name = "UISpriteToolBtn";
            this.UISpriteToolBtn.Size = new System.Drawing.Size(174, 22);
            this.UISpriteToolBtn.Text = "UI Sprites";
            this.UISpriteToolBtn.Click += new System.EventHandler(this.UISpriteToolBtn_Click);
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(174, 22);
            this.toolStripMenuItem14.Text = "Console";
            // 
            // levelVariablesToolStripMenuItem
            // 
            this.levelVariablesToolStripMenuItem.Name = "levelVariablesToolStripMenuItem";
            this.levelVariablesToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.levelVariablesToolStripMenuItem.Text = "Level Variables";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(80, 20);
            this.toolStripMenuItem3.Text = "Preferences";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // mapOpenDialog
            // 
            this.mapOpenDialog.Filter = "Engine file|engine.ps3";
            // 
            // tickTimer
            // 
            this.tickTimer.Enabled = true;
            this.tickTimer.Interval = 1;
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.glControl);
            this.splitContainer1.Panel1.Controls.Add(this.toolstrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1269, 657);
            this.splitContainer1.SplitterDistance = 1000;
            this.splitContainer1.TabIndex = 17;
            // 
            // glControl
            // 
            this.glControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Location = new System.Drawing.Point(3, 28);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(994, 626);
            this.glControl.TabIndex = 16;
            this.glControl.VSync = false;
            this.glControl.ObjectClick += new System.EventHandler<RatchetEdit.RatchetEventArgs>(this.glControl_ObjectClick);
            this.glControl.ObjectDeleted += new System.EventHandler<RatchetEdit.RatchetEventArgs>(this.glControl_ObjectDeleted);
            this.glControl.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDoubleClick);
            // 
            // toolstrip1
            // 
            this.toolstrip1.ImageScalingSize = new System.Drawing.Size(18, 18);
            this.toolstrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cloneBtn,
            this.deleteBtn,
            this.toolStripSeparator2,
            this.translateToolBtn,
            this.rotateToolBtn,
            this.scaleToolBtn,
            this.splineToolBtn});
            this.toolstrip1.Location = new System.Drawing.Point(0, 0);
            this.toolstrip1.Name = "toolstrip1";
            this.toolstrip1.Size = new System.Drawing.Size(1000, 25);
            this.toolstrip1.TabIndex = 15;
            this.toolstrip1.Text = "toolStrip1";
            // 
            // cloneBtn
            // 
            this.cloneBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cloneBtn.Image = global::RatchetEdit.Properties.Resources.add_button;
            this.cloneBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cloneBtn.Name = "cloneBtn";
            this.cloneBtn.Size = new System.Drawing.Size(23, 22);
            this.cloneBtn.Text = "Clone Moby";
            this.cloneBtn.Click += new System.EventHandler(this.cloneBtn_Click);
            // 
            // deleteBtn
            // 
            this.deleteBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteBtn.Image = global::RatchetEdit.Properties.Resources.delete_button;
            this.deleteBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.Size = new System.Drawing.Size(23, 22);
            this.deleteBtn.Text = "Delete Moby (DEL)";
            this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // translateToolBtn
            // 
            this.translateToolBtn.CheckOnClick = true;
            this.translateToolBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.translateToolBtn.Image = global::RatchetEdit.Properties.Resources.translate_tool;
            this.translateToolBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.translateToolBtn.Name = "translateToolBtn";
            this.translateToolBtn.Size = new System.Drawing.Size(23, 22);
            this.translateToolBtn.Text = "Translate Tool (F1)";
            this.translateToolBtn.Click += new System.EventHandler(this.translateToolBtn_Click);
            // 
            // rotateToolBtn
            // 
            this.rotateToolBtn.CheckOnClick = true;
            this.rotateToolBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rotateToolBtn.Image = global::RatchetEdit.Properties.Resources.rotate_tool;
            this.rotateToolBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rotateToolBtn.Name = "rotateToolBtn";
            this.rotateToolBtn.Size = new System.Drawing.Size(23, 22);
            this.rotateToolBtn.Text = "Rotate Tool (F2)";
            this.rotateToolBtn.Click += new System.EventHandler(this.rotateToolBtn_Click);
            // 
            // scaleToolBtn
            // 
            this.scaleToolBtn.CheckOnClick = true;
            this.scaleToolBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.scaleToolBtn.Image = global::RatchetEdit.Properties.Resources.scale_tool;
            this.scaleToolBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.scaleToolBtn.Name = "scaleToolBtn";
            this.scaleToolBtn.Size = new System.Drawing.Size(23, 22);
            this.scaleToolBtn.Text = "Scale Tool (F3)";
            this.scaleToolBtn.Click += new System.EventHandler(this.scaleToolBtn_Click);
            // 
            // splineToolBtn
            // 
            this.splineToolBtn.CheckOnClick = true;
            this.splineToolBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.splineToolBtn.Image = global::RatchetEdit.Properties.Resources.spline_tool;
            this.splineToolBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.splineToolBtn.Name = "splineToolBtn";
            this.splineToolBtn.Size = new System.Drawing.Size(23, 22);
            this.splineToolBtn.Text = "Spline Tool (F4)";
            this.splineToolBtn.Click += new System.EventHandler(this.splineToolBtn_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.objectTree);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.button10);
            this.splitContainer2.Panel2.Controls.Add(this.button9);
            this.splitContainer2.Panel2.Controls.Add(this.button8);
            this.splitContainer2.Panel2.Controls.Add(this.button7);
            this.splitContainer2.Panel2.Controls.Add(this.button6);
            this.splitContainer2.Panel2.Controls.Add(this.button5);
            this.splitContainer2.Panel2.Controls.Add(this.button4);
            this.splitContainer2.Panel2.Controls.Add(this.button3);
            this.splitContainer2.Panel2.Controls.Add(this.button2);
            this.splitContainer2.Panel2.Controls.Add(this.button1);
            this.splitContainer2.Panel2.Controls.Add(this.properties);
            this.splitContainer2.Panel2.Controls.Add(this.camYLabel);
            this.splitContainer2.Panel2.Controls.Add(this.yawLabel);
            this.splitContainer2.Panel2.Controls.Add(this.camXLabel);
            this.splitContainer2.Panel2.Controls.Add(this.label10);
            this.splitContainer2.Panel2.Controls.Add(this.label18);
            this.splitContainer2.Panel2.Controls.Add(this.camZLabel);
            this.splitContainer2.Panel2.Controls.Add(this.pitchLabel);
            this.splitContainer2.Size = new System.Drawing.Size(265, 657);
            this.splitContainer2.SplitterDistance = 270;
            this.splitContainer2.TabIndex = 21;
            // 
            // objectTree
            // 
            this.objectTree.Location = new System.Drawing.Point(4, 3);
            this.objectTree.Name = "objectTree";
            this.objectTree.Size = new System.Drawing.Size(258, 264);
            this.objectTree.TabIndex = 0;
            this.objectTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.objectTreeView1_AfterSelect);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(7, 357);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(75, 23);
            this.button10.TabIndex = 18;
            this.button10.Text = "button10";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(23, 339);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(105, 23);
            this.button9.TabIndex = 28;
            this.button9.Text = "Remove skybox";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Visible = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(23, 318);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(105, 23);
            this.button8.TabIndex = 27;
            this.button8.Text = "Remove terrain";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Visible = false;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(23, 297);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(105, 23);
            this.button7.TabIndex = 26;
            this.button7.Text = "Remove collision";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Visible = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(134, 357);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(52, 23);
            this.button6.TabIndex = 25;
            this.button6.Text = "Imp tex";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Visible = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(192, 357);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(70, 23);
            this.button5.TabIndex = 24;
            this.button5.Text = "Export tex";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Visible = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(134, 336);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(52, 23);
            this.button4.TabIndex = 23;
            this.button4.Text = "Imp coll";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(134, 318);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(52, 23);
            this.button3.TabIndex = 22;
            this.button3.Text = "imp terr";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(192, 339);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(70, 23);
            this.button2.TabIndex = 21;
            this.button2.Text = "Export colli";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(192, 318);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(70, 23);
            this.button1.TabIndex = 20;
            this.button1.Text = "Export ter";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // properties
            // 
            this.properties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.properties.HelpVisible = false;
            this.properties.Location = new System.Drawing.Point(0, 3);
            this.properties.Name = "properties";
            this.properties.Size = new System.Drawing.Size(262, 317);
            this.properties.TabIndex = 19;
            this.properties.ToolbarVisible = false;
            this.properties.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // camYLabel
            // 
            this.camYLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.camYLabel.AutoSize = true;
            this.camYLabel.Location = new System.Drawing.Point(4, 349);
            this.camYLabel.Name = "camYLabel";
            this.camYLabel.Size = new System.Drawing.Size(13, 13);
            this.camYLabel.TabIndex = 12;
            this.camYLabel.Text = "0";
            // 
            // yawLabel
            // 
            this.yawLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.yawLabel.AutoSize = true;
            this.yawLabel.Location = new System.Drawing.Point(81, 336);
            this.yawLabel.Name = "yawLabel";
            this.yawLabel.Size = new System.Drawing.Size(13, 13);
            this.yawLabel.TabIndex = 11;
            this.yawLabel.Text = "0";
            // 
            // camXLabel
            // 
            this.camXLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.camXLabel.AutoSize = true;
            this.camXLabel.Location = new System.Drawing.Point(4, 336);
            this.camXLabel.Name = "camXLabel";
            this.camXLabel.Size = new System.Drawing.Size(13, 13);
            this.camXLabel.TabIndex = 11;
            this.camXLabel.Text = "0";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 323);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(43, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Camera";
            // 
            // label18
            // 
            this.label18.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(81, 323);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(47, 13);
            this.label18.TabIndex = 15;
            this.label18.Text = "Rotation";
            // 
            // camZLabel
            // 
            this.camZLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.camZLabel.AutoSize = true;
            this.camZLabel.Location = new System.Drawing.Point(4, 362);
            this.camZLabel.Name = "camZLabel";
            this.camZLabel.Size = new System.Drawing.Size(13, 13);
            this.camZLabel.TabIndex = 13;
            this.camZLabel.Text = "0";
            // 
            // pitchLabel
            // 
            this.pitchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pitchLabel.AutoSize = true;
            this.pitchLabel.Location = new System.Drawing.Point(81, 349);
            this.pitchLabel.Name = "pitchLabel";
            this.pitchLabel.Size = new System.Drawing.Size(13, 13);
            this.pitchLabel.TabIndex = 12;
            this.pitchLabel.Text = "0";
            // 
            // mapSaveDialog
            // 
            this.mapSaveDialog.FileName = "gameplay_ntsc";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1269, 681);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Replanetizer";
            this.Load += new System.EventHandler(this.Main_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolstrip1.ResumeLayout(false);
            this.toolstrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mapOpenBtn;
        private System.Windows.Forms.ToolStripMenuItem mapSaveBtn;
        private System.Windows.Forms.ToolStripMenuItem mapSaveAsBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolBtn;
        private System.Windows.Forms.ToolStripMenuItem modelViewerToolBtn;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem11;
        private System.Windows.Forms.ToolStripMenuItem spriteViewerToolBtn;
        private System.Windows.Forms.ToolStripMenuItem UISpriteToolBtn;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem14;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label camXLabel;
        private System.Windows.Forms.Label camYLabel;
        private System.Windows.Forms.Label camZLabel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog mapOpenDialog;
        private System.Windows.Forms.Timer tickTimer;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label yawLabel;
        private System.Windows.Forms.Label pitchLabel;
        private System.Windows.Forms.ToolStripMenuItem ViewToolStipItem;
        private System.Windows.Forms.ToolStripMenuItem mobyCheck;
        private System.Windows.Forms.ToolStripMenuItem tieCheck;
        private System.Windows.Forms.ToolStripMenuItem shrubCheck;
        private System.Windows.Forms.ToolStripMenuItem collCheck;
        private System.Windows.Forms.ToolStripMenuItem terrainCheck;
        private System.Windows.Forms.ToolStripMenuItem splineCheck;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PropertyGrid properties;
        private System.Windows.Forms.ToolStripMenuItem levelVariablesToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog mapSaveDialog;
        private System.Windows.Forms.ToolStripMenuItem skyboxCheck;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ToolStrip toolstrip1;
        private System.Windows.Forms.ToolStripButton cloneBtn;
        private System.Windows.Forms.ToolStripButton deleteBtn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton translateToolBtn;
        private System.Windows.Forms.ToolStripButton rotateToolBtn;
        private System.Windows.Forms.ToolStripButton scaleToolBtn;
        private System.Windows.Forms.ToolStripButton splineToolBtn;
        private System.Windows.Forms.ToolStripMenuItem cuboidCheck;
        private System.Windows.Forms.ToolStripMenuItem type0CCheck;
        private ObjectTreeView objectTree;
        private CustomGLControl glControl;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
    }
}

