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
        /*
         * Be aware that textures may come from different sources like from the engine or armor files
         * If other files like gadgets etc. are parsed, their textures need to be handled separately aswell
         * Since this tool is supposed to be able for modding the game, merging the textures into one is probably out of reach
         * (Though we could keep a separate list containing all textures but that is probably also a mess to maintain)
         */

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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

        private int GetTotalTextureCount()
        {
            int count = main.level.textures.Count;

            foreach (List<Texture> list in main.level.armorTextures)
            {
                count += list.Count;
            }

            return count;
        }

        public void UpdateTextureList()
        {
            texAmountLabel.Text = "Texture Count: " + GetTotalTextureCount();
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

            textureView.VirtualListSize = GetTotalTextureCount();

            int index = 0;

            for (int i = 0; i < main.level.textures.Count; i++)
            {
                virtualCache.Add(new ListViewItem("tex_" + i, index++));
            }

            for (int i = 0; i < main.level.armorTextures.Count; i++)
            {
                List<Texture> textures = main.level.armorTextures[i];
                for (int j = 0; j < textures.Count; j++)
                {
                    virtualCache.Add(new ListViewItem("tex_armor" + i + "_" + j, index++));
                }
            }

            ThreadStart tstart = new ThreadStart(delegate ()
            {
                LoadForGrid();
            });

            Thread thread = new Thread(tstart);
            thread.Start();

            texImages.Disposed += (object sender, EventArgs args) => { thread.Abort(); };
        }

        private void AddImage(Image image, int index, string infix)
        {
            if (InvokeRequired)
            {
                this?.Invoke(new MethodInvoker(delegate { texImages.Images.Add("tex_" + infix + index, image); }));
            } else
            {
                texImages.Images.Add("tex_" + infix + index, image);
            }
            
        }

        public void LoadForGrid()
        {
            for (int i = 0; i < main.level.textures.Count; i++)
            {
                Image image = main.level.textures[i].getTextureImage();
                AddImage(image, i, "");
            }

            for (int i = 0; i < main.level.armorTextures.Count; i++)
            {
                List<Texture> textures = main.level.armorTextures[i];
                string infix = "armor" + i + "_";
                for (int j = 0; j < textures.Count; j++)
                {
                    Image image = textures[j].getTextureImage();
                    AddImage(image, j, infix);
                }
            }

            if (InvokeRequired)
            {
                this?.Invoke(new MethodInvoker(delegate { textureView.Refresh(); }));
            }
            else
            {
                textureView.Refresh();
            }
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
            {           
                int index = textureView.Items[col[0]].ImageIndex;

                if (index >= main.level.textures.Count)
                {
                    index -= main.level.textures.Count;

                    foreach (List<Texture> list in main.level.armorTextures)
                    {
                        if (index >= list.Count)
                        {
                            index -= list.Count;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                returnVal = index;
            }      

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
                    Logger.Info("Adding new image texture");
                    using (MagickImage image = new MagickImage(fileName))
                    {
                        image.Format = MagickFormat.Dxt5;
                        image.HasAlpha = true;
                        AddNewTexture(RemoveHeader(image.ToByteArray()), (short)image.Width, (short)image.Height);
                    }
                    break;
                case ".dds":
                    Logger.Info("Adding new DDS texture");
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

        private void ActionOnTextureByIndex(int index, Action<Texture> action)
        {
            if (index >= main.level.textures.Count)
            {
                index -= main.level.textures.Count;

                foreach (List<Texture> list in main.level.armorTextures)
                {
                    if (index >= list.Count)
                    {
                        index -= list.Count;
                    }
                    else
                    {
                        action(list[index]);
                        return;
                    }
                }
            }
            else
            {
                action(main.level.textures[index]);
            }
        }

        private void TexListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textureView.SelectedIndices.Count != 0)
            {
                ActionOnTextureByIndex(textureView.SelectedIndices[0], (t) => { UpdateTextureImage(t); });
            }              
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
            if (textureView.SelectedIndices.Count == 0) return;

            if (saveTextureFileDialog.ShowDialog() != DialogResult.OK) return;

            string fileName = saveTextureFileDialog.FileName;

            ActionOnTextureByIndex(textureView.SelectedIndices[0], (t) => { t.getTextureImage().Save(fileName); });
        }

        private void exportAllButton_Click(object sender, EventArgs e)
        {
            if (exportFolderBrowserDialog.ShowDialog() != DialogResult.OK) return;

            Enabled = false;
            Application.DoEvents();

            try
            {
                string path = exportFolderBrowserDialog.SelectedPath;

                for (int i = 0; i < main.level.textures.Count; i++)
                {
                    Bitmap image = main.level.textures[i].getTextureImage();
                    image.Save(path + "/" + i.ToString() + ".png");
                }

                for (int i = 0; i < main.level.armorTextures.Count; i++)
                {
                    List<Texture> textures = main.level.armorTextures[i];
                    for (int j = 0; j < textures.Count; j++)
                    {
                        Bitmap image = textures[j].getTextureImage();
                        image.Save(path + "/armor" + i + "_" + j.ToString() + ".png");
                    }
                }
            }
            finally
            {
                Enabled = true;
            }


        }
    }
}
