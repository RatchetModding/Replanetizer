using LibReplanetizer;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace RatchetEdit.Forms
{
    public partial class LevelExportWindow : Form
    {
        Main main;

        public LevelExportWindow(Main main)
        {
            InitializeComponent();

            this.main = main;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (exportLevelDialog.ShowDialog() != DialogResult.OK) return;

            string fileName = exportLevelDialog.FileName;

            ModelWriter.WriterLevelSettings settings = new ModelWriter.WriterLevelSettings();

            settings.chunksSelected = new bool[5];
            settings.chunksSelected[0] = true;
            settings.chunksSelected[1] = true;
            settings.chunksSelected[2] = true;
            settings.chunksSelected[3] = true;
            settings.chunksSelected[4] = true;

            settings.combineMeshes = true;

            ModelWriter.WriteObj(fileName, main.level, settings);
        }
    }
}
