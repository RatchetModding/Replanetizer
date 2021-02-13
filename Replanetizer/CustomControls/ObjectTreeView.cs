using LibReplanetizer;
using LibReplanetizer.LevelObjects;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RatchetEdit
{
    public partial class ObjectTreeView : TreeView
    {
        public TreeNode mobyNode = new TreeNode("Mobys");
        public TreeNode tieNode = new TreeNode("Ties");
        public TreeNode shrubNode = new TreeNode("Shrubs");
        public TreeNode splineNode = new TreeNode("Splines");
        public TreeNode cameraNode = new TreeNode("Cameras");
        public TreeNode cuboidNode = new TreeNode("Cuboids");
        public TreeNode type0CNode = new TreeNode("Type 0C's");

        Dictionary<int, string> mobNames, tieNames;

        public ObjectTreeView()
        {
            InitializeComponent();
        }

        public void Init(Dictionary<int, string> mobNames, Dictionary<int, string> tieNames)
        {
            // Get the name lists
            this.mobNames = mobNames;
            this.tieNames = tieNames;

            // Add all the nodes to the tree
            Nodes.Add(mobyNode);
            Nodes.Add(tieNode);
            Nodes.Add(shrubNode);
            Nodes.Add(splineNode);
            Nodes.Add(cameraNode);
            Nodes.Add(cuboidNode);
            Nodes.Add(type0CNode);
        }

        public void UpdateEntries(Level level)
        {
            // Clear all the nodes to prepare for new data
            foreach (TreeNode node in Nodes)
                node.Nodes.Clear();

            // Add new data from the level
            foreach (Moby moby in level.mobs)
            {
                int modelId = moby.modelID;
                int mobyId = level.mobs.IndexOf(moby);

                TreeNode parentNode = GetTreeNode(mobyNode, modelId, mobNames);

                TreeNode childNode = new TreeNode(mobyId.ToString());
                childNode.Tag = mobyId;
                parentNode.Nodes.Add(childNode);
            }
            foreach (Tie tie in level.ties)
            {
                int modelId = tie.modelID;
                int tieId = level.ties.IndexOf(tie);

                TreeNode parentNode = GetTreeNode(tieNode, modelId, tieNames);

                TreeNode childNode = new TreeNode(tieId.ToString());
                childNode.Tag = tieId;
                parentNode.Nodes.Add(childNode);
            }
            foreach (Shrub levelObject in level.shrubs)
            {
                shrubNode.Nodes.Add(levelObject.modelID.ToString("X"));
            }
            foreach (Spline spline in level.splines)
            {
                string splineName = spline.name.ToString("X");
                splineNode.Nodes.Add(splineName);
            }
            foreach (GameCamera gameCamera in level.gameCameras)
            {
                string name = gameCamera.id.ToString("X");
                cameraNode.Nodes.Add(name);
            }
            foreach (Cuboid spawnPoints in level.cuboids)
            {
                string name = level.cuboids.IndexOf(spawnPoints).ToString("X");
                cuboidNode.Nodes.Add(name);
            }
            foreach (Type0C objs in level.type0Cs)
            {
                string name = level.type0Cs.IndexOf(objs).ToString("X");
                type0CNode.Nodes.Add(name);
            }
        }

        private TreeNode GetTreeNode(TreeNode treeNode, int modelId, Dictionary<int, string> names)
        {
            TreeNode parentNode;
            string nodeName = modelId.ToString("X");

            if (treeNode.Nodes.ContainsKey(nodeName))
            {
                parentNode = treeNode.Nodes[nodeName];
            }
            else
            {
                parentNode = new TreeNode(nodeName);

                if (names.ContainsKey(modelId))
                    parentNode.Text = names[modelId];

                parentNode.Name = nodeName;
                treeNode.Nodes.Add(parentNode);
            }

            return parentNode;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
