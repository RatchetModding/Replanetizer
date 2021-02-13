using LibReplanetizer.LevelObjects;
using System;
using System.Windows.Forms;

namespace RatchetEdit
{
    public partial class UIViewer : Form
    {
        Main main;
        public UIViewer(Main main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void UIViewer_Load(object sender, EventArgs e)
        {
            foreach (UiElement uiElem in main.level.uiElements)
            {
                listBox1.Items.Add(uiElem.id.ToString("X"));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateList();
        }

        private void updateList()
        {
            textureImages.Images.Clear();
            textureView.Items.Clear();
            int index = 0;
            foreach (int spritenum in main.level.uiElements[listBox1.SelectedIndex].sprites)
            {
                textureImages.Images.Add(main.level.textures[spritenum].getTextureImage());
                ListViewItem itm = new ListViewItem(spritenum.ToString());
                itm.ImageIndex = index;
                textureView.Items.Add(itm);
                index++;
            }
        }

        private void textureView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            using (var form = new TextureViewer(main))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    int val = form.returnVal;
                    main.level.uiElements[listBox1.SelectedIndex].sprites[textureView.SelectedIndices[0]] = val;
                    updateList();
                }
            }
        }
    }
}
