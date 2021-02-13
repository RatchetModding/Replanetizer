using ImageMagick;
using LibReplanetizer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static LibReplanetizer.DataFunctions;

namespace RatchetEdit
{
    public partial class TextureViewer : Form
    {
        public Main main;
        public TextureConfig conf;
        public ModelViewer mod;
        public UIViewer uiView;

        public int returnVal;

        public List<ListViewItem> virtualCache = new List<ListViewItem>();

        public TextureViewer(Main main)
        {
            InitializeComponent();
            this.main = main;
        }

        public void UpdateTextureList()
        {
            texAmountLabel.Text = "Texture Count: " + main.level.textures.Count;
            UpdateTextureGrid();
        }

        public void UpdateTextureImage(Texture tex)
        {
            textureImage.Image = tex.getTextureImage();
        }

        public void UpdateTextureGrid()
        {
            textureView.Items.Clear();
            texImages.Images.Clear();
            virtualCache.Clear();

            textureView.VirtualListSize = main.level.textures.Count;

            for (int i = 0; i < main.level.textures.Count; i++)
            {
                virtualCache.Add(new ListViewItem("tex_" + i, i));
            }

            ThreadStart tstart = new ThreadStart(delegate ()
            {
                LoadForGrid(null, -1, main.level.textures.Count);
            });

            Thread thread = new Thread(tstart);
            thread.Start();
        }

        public void LoadForGrid(Image image, int index, int test)
        {
            if (InvokeRequired)
            {
                for (int i = 0; i < test; i++)
                {
                    Image images = main.level.textures[i].getTextureImage();
                    this?.Invoke(new MethodInvoker(delegate { LoadForGrid(images, i, -1); }));
                }
                return;
            }
            texImages.Images.Add("tex_" + index, image);
            if (index >= main.level.textures.Count - 1)
                textureView.Refresh();
        }

        //Removes DDS header
        public byte[] RemoveHeader(byte[] input)
        {
            byte[] newData = new byte[input.Length - 0x80];
            Array.Copy(input, 0x80, newData, 0, newData.Length);

            return newData;
        }

        public void AddNewTexture(byte[] image, short width, short height)
        {
            main.level.textures.Add(new Texture(main.level.textures.Count, height, width, image));
            UpdateTextureList();
        }

        private void CloseForm()
        {
            ListView.SelectedIndexCollection col = textureView.SelectedIndices;
            if (col.Count > 0)
                returnVal = textureView.Items[col[0]].ImageIndex;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            if (openTextureDialog.ShowDialog() != DialogResult.OK) return;

            string fileName = openTextureDialog.FileName;
            string extension = Path.GetExtension(fileName).ToLower();

            switch (extension)
            {
                case ".bmp":
                case ".png":
                case ".jpg":
                    Console.WriteLine("Adding new image texture");
                    using (MagickImage image = new MagickImage(fileName))
                    {
                        image.Format = MagickFormat.Dxt5;
                        image.HasAlpha = true;
                        AddNewTexture(RemoveHeader(image.ToByteArray()), (short)image.Width, (short)image.Height);
                    }
                    break;
                case ".dds":
                    Console.WriteLine("Adding new DDS texture");
                    byte[] img = File.ReadAllBytes(fileName);
                    short width = ReadShort(img, 0x10);
                    short height = ReadShort(img, 0x0C);
                    AddNewTexture(RemoveHeader(img), width, height);
                    break;
            }
        }

        private void TexListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
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

        private void TextureViewer_Load(object sender, EventArgs e)
        {
            UpdateTextureList();
        }

        private void TexListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textureView.SelectedIndices.Count != 0)
                UpdateTextureImage(main.level.textures[textureView.SelectedIndices[0]]);
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void exportBtn_Click(object sender, EventArgs e)
        {
            Bitmap image = main.level.textures[textureView.SelectedIndices[0]].getTextureImage();
            image.Save(textureView.SelectedIndices[0].ToString() + ".png");
        }
    }
}
