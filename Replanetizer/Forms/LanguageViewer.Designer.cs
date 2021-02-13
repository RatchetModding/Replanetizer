namespace RatchetEdit
{
    partial class LanguageViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.languageList = new System.Windows.Forms.ListBox();
            this.languageTextList = new System.Windows.Forms.ListView();
            this.textId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textData = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // languageList
            // 
            this.languageList.Dock = System.Windows.Forms.DockStyle.Left;
            this.languageList.FormattingEnabled = true;
            this.languageList.Items.AddRange(new object[] {
            "English",
            "Language 02",
            "French",
            "German",
            "Spanish",
            "Italian",
            "Language 07",
            "Language 08"});
            this.languageList.Location = new System.Drawing.Point(0, 0);
            this.languageList.Name = "languageList";
            this.languageList.Size = new System.Drawing.Size(119, 507);
            this.languageList.TabIndex = 0;
            this.languageList.SelectedIndexChanged += new System.EventHandler(this.languageList_SelectedIndexChanged);
            // 
            // languageTextList
            // 
            this.languageTextList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.textId,
            this.textData});
            this.languageTextList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.languageTextList.HideSelection = false;
            this.languageTextList.Location = new System.Drawing.Point(119, 0);
            this.languageTextList.Name = "languageTextList";
            this.languageTextList.Size = new System.Drawing.Size(681, 507);
            this.languageTextList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.languageTextList.TabIndex = 1;
            this.languageTextList.UseCompatibleStateImageBehavior = false;
            this.languageTextList.View = System.Windows.Forms.View.Details;
            // 
            // textId
            // 
            this.textId.Text = "Text ID";
            // 
            // textData
            // 
            this.textData.Text = "Text";
            this.textData.Width = 580;
            // 
            // LanguageViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 507);
            this.Controls.Add(this.languageTextList);
            this.Controls.Add(this.languageList);
            this.Name = "LanguageViewer";
            this.Text = "Language Data";
            this.Load += new System.EventHandler(this.LanguageViewer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox languageList;
        private System.Windows.Forms.ListView languageTextList;
        private System.Windows.Forms.ColumnHeader textId;
        private System.Windows.Forms.ColumnHeader textData;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}