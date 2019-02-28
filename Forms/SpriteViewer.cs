using System;
using System.Windows.Forms;

namespace RatchetEdit
{
    public partial class SpriteViewer : Form
    {
        Main main;

        public SpriteViewer(Main main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void SpriteViewer_Load(object sender, EventArgs e)
        {
            foreach (int g in main.level.textureConfigMenus)
            {
                listBox1.Items.Add(g);
            }
        }

        private void UpdateImage()
        {
            pictureBox1.Image = main.level.textures[main.level.textureConfigMenus[listBox1.SelectedIndex]].getTextureImage();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateImage();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            using (var form = new TextureViewer(main))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    int val = form.returnVal;
                    main.level.textureConfigMenus[listBox1.SelectedIndex] = val;
                    UpdateImage();
                }
            }
        }
    }
}
