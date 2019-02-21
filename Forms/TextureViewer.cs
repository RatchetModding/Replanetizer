using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RatchetEdit.DataFunctions;

namespace RatchetEdit
{
    public partial class TextureViewer : Form
    {
        public Main main;
        public TextureConfig conf;
        public ModelViewer mod;

        public List<ListViewItem> virtualCache = new List<ListViewItem>();

        public TextureViewer(Main main)
        {
            this.main = main;
            InitializeComponent();
        }

        public TextureViewer(Main main, ModelViewer mod, TextureConfig conf)
        {
            InitializeComponent();

            this.mod = mod;
            this.conf = conf;
            this.main = main;

            button1.Visible = true;
            button2.Visible = true;
        }

        private void TextureViewer_Load(object sender, EventArgs e)
        {
            updateTextureList();
        }

        public void updateTextureList()
        {
            texAmountLabel.Text = "Texture Count: " + main.level.textures.Count();
            updateTextureGrid();
        }

        public void updateTextureImage(Texture tex)
        {
            textureImage.Image = tex.getTextureImage();

            if (tex.height > textureImage.Height || tex.width > textureImage.Width)
                textureImage.SizeMode = PictureBoxSizeMode.StretchImage;
            else
                textureImage.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        public void updateTextureGrid()
        {
            texListView.Items.Clear();
            texImages.Images.Clear();
            virtualCache.Clear();

            texListView.VirtualListSize = main.level.textures.Count;

            for (int i = 0; i < main.level.textures.Count; i++)
            {
                virtualCache.Add(new ListViewItem("tex_" + i, i));
            }

            ThreadStart tstart = new ThreadStart(delegate ()
            {
                loadForGrid(null, -1, main.level.textures.Count);
            });

            Thread thread = new Thread(tstart);
            thread.Start();
        }

        public void loadForGrid(Image image, int index, int test)
        {
            if (InvokeRequired)
            {
                for (int i = 0; i < test; i++)
                {
                    Image images = main.level.textures[i].getTextureImage();
                    this.Invoke(new MethodInvoker(delegate { loadForGrid(images, i, -1); }));
                }
                return;
            }
            texImages.Images.Add("tex_" + index, image);
            if (index >= main.level.textures.Count - 1)
                texListView.Refresh();
        }

        private void texListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (main.level.textures != null && e.ItemIndex >= 0 && e.ItemIndex < virtualCache.Count)
            {
                //A cache hit, so get the ListViewItem from the cache instead of making a new one.
                e.Item = virtualCache[e.ItemIndex];
            }
            else
            {
                //A cache miss, so create a new ListViewItem and pass it back.
                int x = e.ItemIndex * e.ItemIndex;
                e.Item = new ListViewItem(x.ToString());
            }
        }

        private void texListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(texListView.SelectedIndices.Count != 0)
                updateTextureImage(main.level.textures[texListView.SelectedIndices[0]]);
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            if (addTextureDialog.ShowDialog() == DialogResult.OK)
            {
                short width = 0;
                short height = 0;
                byte[] img = File.ReadAllBytes(addTextureDialog.FileName);

                string extension = Path.GetExtension(addTextureDialog.FileName).ToLower();
                Console.WriteLine(addTextureDialog.FileName);
                switch (extension)
                {
                    case ".bmp":
                    case ".png":
                    case ".jpg":
                        Console.WriteLine("Adding new PNG texture");
                        using (MagickImage image = new MagickImage(addTextureDialog.FileName))
                        {
                            image.Format = MagickFormat.Dxt5;
                            image.HasAlpha = true;
                            addNewTexture(removeHeader(image.ToByteArray()), (short)image.Width, (short)image.Height);
                        }
                        break;
                    case ".dds":
                        Console.WriteLine("Adding new DDS texture");
                        width = ReadShort(img, 0x10);
                        height = ReadShort(img, 0x0C);
                        addNewTexture(removeHeader(img), width, height);
                        break;

                }
            }
        }

        //Removes DDS header
        public byte[] removeHeader(byte[] input)
        {
            byte[] newData = new byte[input.Length - 0x80];
            for (int i = 0; i < input.Length - 0x80; i++)
            {
                newData[i] = input[0x80 + i];
            }
            return newData;
        }

        public void addNewTexture(byte[] image, short width, short height)
        {
            Texture newTex = new Texture(main.level.textures.Count, height, width, image);

            main.level.textures.Add(newTex);
            updateTextureList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection col = texListView.SelectedIndices;
            conf.ID = texListView.Items[col[0]].ImageIndex;
            mod.UpdateModel();
            this.Close();
        }

        private void texListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListView.SelectedIndexCollection col = texListView.SelectedIndices;
            conf.ID = texListView.Items[col[0]].ImageIndex;
            mod.UpdateModel();
            this.Close();
        }
    }
}
