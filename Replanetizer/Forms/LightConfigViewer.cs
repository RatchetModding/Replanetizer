using LibReplanetizer;
using System;
using System.Windows.Forms;


namespace RatchetEdit.Forms
{
    public partial class LightConfigViewer : Form
    {
        Level level;
        Main mainForm;
        public LightConfigViewer(Main main)
        {
            InitializeComponent();

            mainForm = main;
            level = main.level;

        }

        private void LightConfigViewer_Load(object sender, EventArgs e)
        {
            properties.SelectedObject = level.lightConfig;
        }
    }
}
