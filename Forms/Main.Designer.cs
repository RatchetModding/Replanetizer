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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.mapOpenBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.mapSaveBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.mapSaveAsBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolBtn = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mobyCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.tieCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.shrubCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.collCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.terrainCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.splineCheck = new System.Windows.Forms.ToolStripMenuItem();
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
            this.openModelViewerBtn = new System.Windows.Forms.Button();
            this.gotoPositionBtn = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.camXLabel = new System.Windows.Forms.Label();
            this.camYLabel = new System.Windows.Forms.Label();
            this.camZLabel = new System.Windows.Forms.Label();
            this.mapOpenDialog = new System.Windows.Forms.OpenFileDialog();
            this.glControl1 = new OpenTK.GLControl();
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.label18 = new System.Windows.Forms.Label();
            this.yawLabel = new System.Windows.Forms.Label();
            this.pitchLabel = new System.Windows.Forms.Label();
            this.objectTree = new System.Windows.Forms.TreeView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.properies = new System.Windows.Forms.PropertyGrid();
            this.label8 = new System.Windows.Forms.Label();
            this.splineVertex = new System.Windows.Forms.NumericUpDown();
            this.deleteButton = new System.Windows.Forms.Button();
            this.cloneButton = new System.Windows.Forms.Button();
            this.mapSaveDialog = new System.Windows.Forms.SaveFileDialog();
            this.toolLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splineVertex)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.viewToolStripMenuItem,
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
            this.toolStripMenuItem4.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem4.Text = "New";
            // 
            // mapOpenBtn
            // 
            this.mapOpenBtn.Name = "mapOpenBtn";
            this.mapOpenBtn.Size = new System.Drawing.Size(180, 22);
            this.mapOpenBtn.Text = "Open";
            this.mapOpenBtn.Click += new System.EventHandler(this.mapOpenBtn_Click);
            // 
            // mapSaveBtn
            // 
            this.mapSaveBtn.Name = "mapSaveBtn";
            this.mapSaveBtn.Size = new System.Drawing.Size(180, 22);
            this.mapSaveBtn.Text = "Save";
            // 
            // mapSaveAsBtn
            // 
            this.mapSaveAsBtn.Name = "mapSaveAsBtn";
            this.mapSaveAsBtn.Size = new System.Drawing.Size(180, 22);
            this.mapSaveAsBtn.Text = "Save as...";
            this.mapSaveAsBtn.Click += new System.EventHandler(this.mapSaveAsBtn_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolBtn
            // 
            this.exitToolBtn.Name = "exitToolBtn";
            this.exitToolBtn.Size = new System.Drawing.Size(180, 22);
            this.exitToolBtn.Text = "Exit";
            this.exitToolBtn.Click += new System.EventHandler(this.exitToolBtn_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mobyCheck,
            this.tieCheck,
            this.shrubCheck,
            this.collCheck,
            this.terrainCheck,
            this.splineCheck});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
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
            this.mobyCheck.Click += new System.EventHandler(this.enableCheck);
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
            this.tieCheck.Click += new System.EventHandler(this.enableCheck);
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
            this.shrubCheck.Click += new System.EventHandler(this.enableCheck);
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
            this.collCheck.Click += new System.EventHandler(this.enableCheck);
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
            this.terrainCheck.Click += new System.EventHandler(this.enableCheck);
            // 
            // splineCheck
            // 
            this.splineCheck.CheckOnClick = true;
            this.splineCheck.Enabled = false;
            this.splineCheck.Name = "splineCheck";
            this.splineCheck.Size = new System.Drawing.Size(120, 22);
            this.splineCheck.Text = "Splines";
            this.splineCheck.Click += new System.EventHandler(this.enableCheck);
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
            // openModelViewerBtn
            // 
            this.openModelViewerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.openModelViewerBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.openModelViewerBtn.Location = new System.Drawing.Point(136, 532);
            this.openModelViewerBtn.Name = "openModelViewerBtn";
            this.openModelViewerBtn.Size = new System.Drawing.Size(173, 23);
            this.openModelViewerBtn.TabIndex = 13;
            this.openModelViewerBtn.Text = "Open in Model Viewer";
            this.openModelViewerBtn.UseVisualStyleBackColor = true;
            this.openModelViewerBtn.Click += new System.EventHandler(this.openModelViewerBtn_Click);
            // 
            // gotoPositionBtn
            // 
            this.gotoPositionBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gotoPositionBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gotoPositionBtn.Location = new System.Drawing.Point(6, 532);
            this.gotoPositionBtn.Name = "gotoPositionBtn";
            this.gotoPositionBtn.Size = new System.Drawing.Size(124, 23);
            this.gotoPositionBtn.TabIndex = 12;
            this.gotoPositionBtn.Text = "Go to Position";
            this.gotoPositionBtn.UseVisualStyleBackColor = true;
            this.gotoPositionBtn.Click += new System.EventHandler(this.gotoPositionBtn_Click);
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 586);
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
            this.camXLabel.Location = new System.Drawing.Point(3, 599);
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
            this.camYLabel.Location = new System.Drawing.Point(3, 612);
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
            this.camZLabel.Location = new System.Drawing.Point(3, 625);
            this.camZLabel.Name = "camZLabel";
            this.camZLabel.Size = new System.Drawing.Size(13, 13);
            this.camZLabel.TabIndex = 13;
            this.camZLabel.Text = "0";
            // 
            // mapOpenDialog
            // 
            this.mapOpenDialog.Filter = "Engine file|engine.ps3";
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(0, 0);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(941, 657);
            this.glControl1.TabIndex = 14;
            this.glControl1.VSync = false;
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseUp);
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
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
            this.label18.Location = new System.Drawing.Point(133, 586);
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
            this.yawLabel.Location = new System.Drawing.Point(133, 599);
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
            this.pitchLabel.Location = new System.Drawing.Point(133, 612);
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
            this.objectTree.Location = new System.Drawing.Point(3, 11);
            this.objectTree.Name = "objectTree";
            treeNode1.Name = "mobyNode";
            treeNode1.Text = "Mobys";
            treeNode2.Name = "tieNode";
            treeNode2.Text = "Ties";
            treeNode3.Name = "shrubNode";
            treeNode3.Text = "Shrubs";
            treeNode4.Name = "splineNode";
            treeNode4.Text = "Splines";
            this.objectTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4});
            this.objectTree.Size = new System.Drawing.Size(306, 242);
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
            this.splitContainer1.Panel1.Controls.Add(this.glControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.toolLabel);
            this.splitContainer1.Panel2.Controls.Add(this.properies);
            this.splitContainer1.Panel2.Controls.Add(this.label8);
            this.splitContainer1.Panel2.Controls.Add(this.splineVertex);
            this.splitContainer1.Panel2.Controls.Add(this.deleteButton);
            this.splitContainer1.Panel2.Controls.Add(this.cloneButton);
            this.splitContainer1.Panel2.Controls.Add(this.objectTree);
            this.splitContainer1.Panel2.Controls.Add(this.label18);
            this.splitContainer1.Panel2.Controls.Add(this.pitchLabel);
            this.splitContainer1.Panel2.Controls.Add(this.openModelViewerBtn);
            this.splitContainer1.Panel2.Controls.Add(this.camZLabel);
            this.splitContainer1.Panel2.Controls.Add(this.label10);
            this.splitContainer1.Panel2.Controls.Add(this.gotoPositionBtn);
            this.splitContainer1.Panel2.Controls.Add(this.camXLabel);
            this.splitContainer1.Panel2.Controls.Add(this.yawLabel);
            this.splitContainer1.Panel2.Controls.Add(this.camYLabel);
            this.splitContainer1.Size = new System.Drawing.Size(1269, 657);
            this.splitContainer1.SplitterDistance = 944;
            this.splitContainer1.TabIndex = 17;
            // 
            // properies
            // 
            this.properies.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.properies.HelpVisible = false;
            this.properies.Location = new System.Drawing.Point(3, 259);
            this.properies.Name = "properies";
            this.properies.Size = new System.Drawing.Size(306, 267);
            this.properies.TabIndex = 19;
            this.properies.ToolbarVisible = false;
            this.properies.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(220, 586);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Spline Index";
            // 
            // splineVertex
            // 
            this.splineVertex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splineVertex.Location = new System.Drawing.Point(225, 599);
            this.splineVertex.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.splineVertex.Name = "splineVertex";
            this.splineVertex.Size = new System.Drawing.Size(60, 20);
            this.splineVertex.TabIndex = 17;
            this.splineVertex.ValueChanged += new System.EventHandler(this.splineVertex_ValueChanged);
            // 
            // deleteButton
            // 
            this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deleteButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.deleteButton.ForeColor = System.Drawing.Color.Red;
            this.deleteButton.Location = new System.Drawing.Point(6, 560);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(124, 23);
            this.deleteButton.TabIndex = 14;
            this.deleteButton.Text = "Delete Object";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // cloneButton
            // 
            this.cloneButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cloneButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cloneButton.Location = new System.Drawing.Point(136, 560);
            this.cloneButton.Name = "cloneButton";
            this.cloneButton.Size = new System.Drawing.Size(173, 23);
            this.cloneButton.TabIndex = 15;
            this.cloneButton.Text = "Clone Object";
            this.cloneButton.UseVisualStyleBackColor = true;
            this.cloneButton.Click += new System.EventHandler(this.cloneButton_Click);
            // 
            // mapSaveDialog
            // 
            this.mapSaveDialog.FileName = "gameplay_ntsc";
            // toolLabel
            // 
            this.toolLabel.AutoSize = true;
            this.toolLabel.Location = new System.Drawing.Point(225, 624);
            this.toolLabel.Name = "toolLabel";
            this.toolLabel.Size = new System.Drawing.Size(31, 13);
            this.toolLabel.TabIndex = 20;
            this.toolLabel.Text = "none";
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
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splineVertex)).EndInit();
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
        private System.Windows.Forms.Button openModelViewerBtn;
        private System.Windows.Forms.Button gotoPositionBtn;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label camXLabel;
        private System.Windows.Forms.Label camYLabel;
        private System.Windows.Forms.Label camZLabel;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog mapOpenDialog;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.Timer tickTimer;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label yawLabel;
        private System.Windows.Forms.Label pitchLabel;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mobyCheck;
        private System.Windows.Forms.ToolStripMenuItem tieCheck;
        private System.Windows.Forms.ToolStripMenuItem shrubCheck;
        private System.Windows.Forms.ToolStripMenuItem collCheck;
        private System.Windows.Forms.ToolStripMenuItem terrainCheck;
        private System.Windows.Forms.ToolStripMenuItem splineCheck;
        private System.Windows.Forms.TreeView objectTree;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button cloneButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.NumericUpDown splineVertex;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.PropertyGrid properies;
        private System.Windows.Forms.ToolStripMenuItem levelVariablesToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog mapSaveDialog;
        private System.Windows.Forms.Label toolLabel;
    }
}

