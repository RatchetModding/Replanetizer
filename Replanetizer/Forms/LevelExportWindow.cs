using LibReplanetizer;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace RatchetEdit.Forms
{
    public partial class LevelExportWindow : Form
    {
        private Level level;
        private Main main;
        private ModelWriter.WriterLevelSettings settings;

        public LevelExportWindow(Main main)
        {
            InitializeComponent();

            level = main.level;
            this.main = main;
            settings = new ModelWriter.WriterLevelSettings();

            meshModeCombobox.SelectedIndex = (int)settings.mode;
            tiesCheckbox.Checked = settings.writeTies;
            shrubsCheckbox.Checked = settings.writeShrubs;
            mobiesCheckbox.Checked = settings.writeMobies;
            chunk0Checkbox.Checked = settings.chunksSelected[0];
            chunk1Checkbox.Checked = settings.chunksSelected[1];
            chunk2Checkbox.Checked = settings.chunksSelected[2];
            chunk3Checkbox.Checked = settings.chunksSelected[3];
            chunk4Checkbox.Checked = settings.chunksSelected[4];
            mtlCheckbox.Checked = settings.exportMTLFile;

            if (level.terrainChunks.Count < 5)
            {
                chunk4Checkbox.Enabled = false;
                chunk4Checkbox.Checked = false;
            }

            if (level.terrainChunks.Count < 4)
            {
                chunk3Checkbox.Enabled = false;
                chunk3Checkbox.Checked = false;
            }

            if (level.terrainChunks.Count < 3)
            {
                chunk2Checkbox.Enabled = false;
                chunk2Checkbox.Checked = false;
            }

            if (level.terrainChunks.Count < 2)
            {
                chunk1Checkbox.Enabled = false;
                chunk1Checkbox.Checked = false;
            }
        }

        private void exportLevel()
        {
            if (exportLevelDialog.ShowDialog() != DialogResult.OK) return;

            string fileName = exportLevelDialog.FileName;

            settings.mode = (ModelWriter.WriterLevelMode)meshModeCombobox.SelectedIndex;
            settings.writeTies = tiesCheckbox.Checked;
            settings.writeShrubs = shrubsCheckbox.Checked;
            settings.writeMobies = mobiesCheckbox.Checked;
            settings.chunksSelected[0] = chunk0Checkbox.Checked;
            settings.chunksSelected[1] = chunk1Checkbox.Checked;
            settings.chunksSelected[2] = chunk2Checkbox.Checked;
            settings.chunksSelected[3] = chunk3Checkbox.Checked;
            settings.chunksSelected[4] = chunk4Checkbox.Checked;
            settings.exportMTLFile = mtlCheckbox.Checked;

            ModelWriter.WriteObj(fileName, level, settings);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;
            exportProgressStatus.Text = "Export in Progress...";
            Enabled = false;
            Application.DoEvents();

            try
            {
                exportLevel();
            }
            finally
            {
                Application.UseWaitCursor = false;
                exportProgressStatus.Text = "";
                Enabled = true;
            }
            
        }

        private void enableMain(object sender, FormClosingEventArgs e)
        {
            main.Enabled = true;
        }
    }
}
