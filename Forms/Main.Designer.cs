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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Mobys");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Ties");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Shrubs");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Splines");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Game Cameras");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Spawn points");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Type0Cs");
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
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.modelViewerToolBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
            this.levelVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label10 = new System.Windows.Forms.Label();
            this.camXLabel = new System.Windows.Forms.Label();
            this.camYLabel = new System.Windows.Forms.Label();
            this.camZLabel = new System.Windows.Forms.Label();
            this.mapOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.label18 = new System.Windows.Forms.Label();
            this.yawLabel = new System.Windows.Forms.Label();
            this.pitchLabel = new System.Windows.Forms.Label();
            this.objectTree = new System.Windows.Forms.TreeView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolstrip1 = new System.Windows.Forms.ToolStrip();
            this.cloneBtn = new System.Windows.Forms.ToolStripButton();
            this.deleteBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.translateToolBtn = new System.Windows.Forms.ToolStripButton();
            this.rotateToolBtn = new System.Windows.Forms.ToolStripButton();
            this.scaleToolBtn = new System.Windows.Forms.ToolStripButton();
            this.splineToolBtn = new System.Windows.Forms.ToolStripButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.properties = new System.Windows.Forms.PropertyGrid();
            this.mapSaveDialog = new System.Windows.Forms.SaveFileDialog();
            this.glControl1 = new RatchetEdit.CustomGLControl();
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
            this.skyboxCheck});
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
            this.mobyCheck.Click += new System.EventHandler(this.EnableCheck);
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
            this.tieCheck.Click += new System.EventHandler(this.EnableCheck);
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
            this.shrubCheck.Click += new System.EventHandler(this.EnableCheck);
            // 
            // collCheck
            // 
            this.collCheck.Checked = true;
            this.collCheck.CheckOnClick = true;
            this.collCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.collCheck.Enabled = false;
            this.collCheck.Name = "collCheck";
            this.collCheck.Size = new System.Drawing.Size(120, 22);
            this.collCheck.Text = "Collision";
            this.collCheck.Click += new System.EventHandler(this.EnableCheck);
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
            this.terrainCheck.Click += new System.EventHandler(this.EnableCheck);
            // 
            // splineCheck
            // 
            this.splineCheck.CheckOnClick = true;
            this.splineCheck.Enabled = false;
            this.splineCheck.Name = "splineCheck";
            this.splineCheck.Size = new System.Drawing.Size(120, 22);
            this.splineCheck.Text = "Splines";
            this.splineCheck.Click += new System.EventHandler(this.EnableCheck);
            // 
            // skyboxCheck
            // 
            this.skyboxCheck.CheckOnClick = true;
            this.skyboxCheck.Enabled = false;
            this.skyboxCheck.Name = "skyboxCheck";
            this.skyboxCheck.Size = new System.Drawing.Size(120, 22);
            this.skyboxCheck.Text = "Skybox";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modelViewerToolBtn,
            this.toolStripMenuItem10,
            this.toolStripMenuItem11,
            this.toolStripMenuItem12,
            this.toolStripMenuItem13,
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
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new System.Drawing.Size(174, 22);
            this.toolStripMenuItem12.Text = "Sprites";
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(174, 22);
            this.toolStripMenuItem13.Text = "Strings";
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
            // mapOpenDialog
            // 
            this.mapOpenDialog.Filter = "Engine file|engine.ps3";
            // 
            // tickTimer
            // 
            this.tickTimer.Enabled = true;
            this.tickTimer.Interval = 16;
            this.tickTimer.Tick += new System.EventHandler(this.tickTimer_Tick);
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
            // objectTree
            // 
            this.objectTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectTree.HideSelection = false;
            this.objectTree.Location = new System.Drawing.Point(0, 3);
            this.objectTree.Name = "objectTree";
            treeNode1.Name = "mobyNode";
            treeNode1.Text = "Mobys";
            treeNode2.Name = "tieNode";
            treeNode2.Text = "Ties";
            treeNode3.Name = "shrubNode";
            treeNode3.Text = "Shrubs";
            treeNode4.Name = "splineNode";
            treeNode4.Text = "Splines";
            treeNode5.Name = "gameCameraNode";
            treeNode5.Text = "Game Cameras";
            treeNode6.Name = "spawnPointNode";
            treeNode6.Text = "Spawn points";
            treeNode7.Name = "type0CNode";
            treeNode7.Text = "Type0Cs";
            this.objectTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7});
            this.objectTree.Size = new System.Drawing.Size(262, 264);
            this.objectTree.TabIndex = 16;
            this.objectTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.objectTree_AfterSelect);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolstrip1);
            this.splitContainer1.Panel1.Controls.Add(this.glControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1269, 657);
            this.splitContainer1.SplitterDistance = 1000;
            this.splitContainer1.TabIndex = 17;
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
            // mapSaveDialog
            // 
            this.mapSaveDialog.FileName = "gameplay_ntsc";
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(3, 3);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(999, 651);
            this.glControl1.TabIndex = 14;
            this.glControl1.VSync = false;
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            this.glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseWheel);
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
            this.Text = "Ratchet Level Editor";
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem12;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem14;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label camXLabel;
        private System.Windows.Forms.Label camYLabel;
        private System.Windows.Forms.Label camZLabel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog mapOpenDialog;
        private CustomGLControl glControl1;
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
        private System.Windows.Forms.TreeView objectTree;
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
    }
}

