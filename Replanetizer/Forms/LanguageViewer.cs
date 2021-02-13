using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RatchetEdit
{
    public partial class LanguageViewer : Form
    {
        Main main;
        public LanguageViewer(Main main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void ShowLanguageText(Dictionary<int, String> languageData)
        {
            var items = new List<ListViewItem>(languageData.Count);

            foreach (KeyValuePair<int, String> entry in languageData)
            {
                ListViewItem item = new ListViewItem(entry.Key.ToString());
                item.SubItems.Add(entry.Value);
                items.Add(item);
            }

            languageTextList.Items.AddRange(items.ToArray());
        }

        private void UpdateList()
        {
            languageTextList.Items.Clear();

            switch (languageList.SelectedIndex)
            {
                case 0:
                    ShowLanguageText(main.level.english);
                    break;
                case 1:
                    ShowLanguageText(main.level.lang2);
                    break;
                case 2:
                    ShowLanguageText(main.level.french);
                    break;
                case 3:
                    ShowLanguageText(main.level.german);
                    break;
                case 4:
                    ShowLanguageText(main.level.spanish);
                    break;
                case 5:
                    ShowLanguageText(main.level.italian);
                    break;
                case 6:
                    ShowLanguageText(main.level.lang7);
                    break;
                case 7:
                    ShowLanguageText(main.level.lang8);
                    break;
            }
        }

        private void languageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void LanguageViewer_Load(object sender, EventArgs e)
        {
            UpdateList();
        }
    }
}
