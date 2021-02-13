using LibReplanetizer;
using System;
using System.Windows.Forms;

namespace RatchetEdit.Forms
{
    public partial class LevelVariableViewer : Form
    {
        Level level;
        Main mainForm;
        public LevelVariableViewer(Main main)
        {
            InitializeComponent();

            mainForm = main;
            level = main.level;

        }

        private void LevelVariableViewer_Load(object sender, EventArgs e)
        {
            properties.SelectedObject = level.levelVariables;
        }
    }
}
