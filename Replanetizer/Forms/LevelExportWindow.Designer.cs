using System.Drawing;
using System.Windows.Forms;

namespace RatchetEdit.Forms
{
    partial class LevelExportWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.exportButton = new System.Windows.Forms.Button();
            this.exportLevelDialog = new System.Windows.Forms.SaveFileDialog();
            this.meshModeCombobox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.exportProgressBar = new System.Windows.Forms.ProgressBar();
            this.tiesCheckbox = new System.Windows.Forms.CheckBox();
            this.shrubsCheckbox = new System.Windows.Forms.CheckBox();
            this.mobiesCheckbox = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.chunk0Checkbox = new System.Windows.Forms.CheckBox();
            this.chunk1Checkbox = new System.Windows.Forms.CheckBox();
            this.chunk2Checkbox = new System.Windows.Forms.CheckBox();
            this.chunk3Checkbox = new System.Windows.Forms.CheckBox();
            this.chunk4Checkbox = new System.Windows.Forms.CheckBox();
            this.mtlCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // exportButton
            // 
            this.exportButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exportButton.Location = new System.Drawing.Point(264, 625);
            this.exportButton.MaximumSize = new System.Drawing.Size(200, 39);
            this.exportButton.MinimumSize = new System.Drawing.Size(200, 39);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(200, 39);
            this.exportButton.TabIndex = 2;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // exportLevelDialog
            // 
            this.exportLevelDialog.FileName = "Level.obj";
            this.exportLevelDialog.Filter = "Wavefront OBJ file|*.obj";
            // 
            // meshModeCombobox
            // 
            this.meshModeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.meshModeCombobox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.meshModeCombobox.FormattingEnabled = true;
            this.meshModeCombobox.Items.AddRange(new object[] {
            "Separated",
            "Combined",
            "Typewise",
            "Materialwise"});
            this.meshModeCombobox.Location = new System.Drawing.Point(264, 39);
            this.meshModeCombobox.Margin = new System.Windows.Forms.Padding(0);
            this.meshModeCombobox.MaximumSize = new System.Drawing.Size(200, 0);
            this.meshModeCombobox.MinimumSize = new System.Drawing.Size(200, 0);
            this.meshModeCombobox.Name = "meshModeCombobox";
            this.meshModeCombobox.Size = new System.Drawing.Size(200, 39);
            this.meshModeCombobox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(25, 39);
            this.label1.MaximumSize = new System.Drawing.Size(200, 39);
            this.label1.MinimumSize = new System.Drawing.Size(200, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 39);
            this.label1.TabIndex = 4;
            this.label1.Text = "Mesh Mode";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(25, 97);
            this.label2.MaximumSize = new System.Drawing.Size(200, 39);
            this.label2.MinimumSize = new System.Drawing.Size(200, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(200, 39);
            this.label2.TabIndex = 5;
            this.label2.Text = "Include Ties";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(25, 155);
            this.label3.MaximumSize = new System.Drawing.Size(200, 39);
            this.label3.MinimumSize = new System.Drawing.Size(200, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(200, 39);
            this.label3.TabIndex = 6;
            this.label3.Text = "Include Shrubs";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(25, 213);
            this.label4.MaximumSize = new System.Drawing.Size(200, 39);
            this.label4.MinimumSize = new System.Drawing.Size(200, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(200, 39);
            this.label4.TabIndex = 7;
            this.label4.Text = "Include Mobies";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(25, 271);
            this.label5.MaximumSize = new System.Drawing.Size(250, 39);
            this.label5.MinimumSize = new System.Drawing.Size(250, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(250, 39);
            this.label5.TabIndex = 8;
            this.label5.Text = "Include Chunk 0";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // exportProgressBar
            // 
            this.exportProgressBar.BackColor = System.Drawing.SystemColors.Control;
            this.exportProgressBar.Location = new System.Drawing.Point(25, 625);
            this.exportProgressBar.MaximumSize = new System.Drawing.Size(200, 39);
            this.exportProgressBar.MinimumSize = new System.Drawing.Size(200, 39);
            this.exportProgressBar.Name = "exportProgressBar";
            this.exportProgressBar.Size = new System.Drawing.Size(200, 39);
            this.exportProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.exportProgressBar.TabIndex = 9;
            // 
            // tiesCheckbox
            // 
            this.tiesCheckbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.tiesCheckbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tiesCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.tiesCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tiesCheckbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.tiesCheckbox.Location = new System.Drawing.Point(425, 97);
            this.tiesCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.tiesCheckbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.tiesCheckbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.tiesCheckbox.Name = "tiesCheckbox";
            this.tiesCheckbox.Size = new System.Drawing.Size(39, 39);
            this.tiesCheckbox.TabIndex = 10;
            this.tiesCheckbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tiesCheckbox.UseVisualStyleBackColor = false;
            // 
            // shrubsCheckbox
            // 
            this.shrubsCheckbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.shrubsCheckbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.shrubsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.shrubsCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.shrubsCheckbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.shrubsCheckbox.Location = new System.Drawing.Point(425, 155);
            this.shrubsCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.shrubsCheckbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.shrubsCheckbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.shrubsCheckbox.Name = "shrubsCheckbox";
            this.shrubsCheckbox.Size = new System.Drawing.Size(39, 39);
            this.shrubsCheckbox.TabIndex = 11;
            this.shrubsCheckbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.shrubsCheckbox.UseVisualStyleBackColor = false;
            // 
            // mobiesCheckbox
            // 
            this.mobiesCheckbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.mobiesCheckbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.mobiesCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.mobiesCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mobiesCheckbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.mobiesCheckbox.Location = new System.Drawing.Point(425, 213);
            this.mobiesCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.mobiesCheckbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.mobiesCheckbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.mobiesCheckbox.Name = "mobiesCheckbox";
            this.mobiesCheckbox.Size = new System.Drawing.Size(39, 39);
            this.mobiesCheckbox.TabIndex = 12;
            this.mobiesCheckbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.mobiesCheckbox.UseVisualStyleBackColor = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(25, 329);
            this.label6.MaximumSize = new System.Drawing.Size(250, 39);
            this.label6.MinimumSize = new System.Drawing.Size(250, 39);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(250, 39);
            this.label6.TabIndex = 13;
            this.label6.Text = "Include Chunk 1";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(25, 387);
            this.label7.MaximumSize = new System.Drawing.Size(250, 39);
            this.label7.MinimumSize = new System.Drawing.Size(250, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(250, 39);
            this.label7.TabIndex = 14;
            this.label7.Text = "Include Chunk 2";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(25, 445);
            this.label8.MaximumSize = new System.Drawing.Size(250, 39);
            this.label8.MinimumSize = new System.Drawing.Size(250, 39);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(250, 39);
            this.label8.TabIndex = 15;
            this.label8.Text = "Include Chunk 3";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(25, 503);
            this.label9.MaximumSize = new System.Drawing.Size(250, 39);
            this.label9.MinimumSize = new System.Drawing.Size(250, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(250, 39);
            this.label9.TabIndex = 16;
            this.label9.Text = "Include Chunk 4";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(25, 561);
            this.label10.MaximumSize = new System.Drawing.Size(250, 39);
            this.label10.MinimumSize = new System.Drawing.Size(250, 39);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(250, 39);
            this.label10.TabIndex = 17;
            this.label10.Text = "Include MTL File";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chunk0Checkbox
            // 
            this.chunk0Checkbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.chunk0Checkbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.chunk0Checkbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chunk0Checkbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chunk0Checkbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.chunk0Checkbox.Location = new System.Drawing.Point(425, 271);
            this.chunk0Checkbox.Margin = new System.Windows.Forms.Padding(0);
            this.chunk0Checkbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.chunk0Checkbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.chunk0Checkbox.Name = "chunk0Checkbox";
            this.chunk0Checkbox.Size = new System.Drawing.Size(39, 39);
            this.chunk0Checkbox.TabIndex = 18;
            this.chunk0Checkbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chunk0Checkbox.UseVisualStyleBackColor = false;
            // 
            // chunk1Checkbox
            // 
            this.chunk1Checkbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.chunk1Checkbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.chunk1Checkbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chunk1Checkbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chunk1Checkbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.chunk1Checkbox.Location = new System.Drawing.Point(425, 329);
            this.chunk1Checkbox.Margin = new System.Windows.Forms.Padding(0);
            this.chunk1Checkbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.chunk1Checkbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.chunk1Checkbox.Name = "chunk1Checkbox";
            this.chunk1Checkbox.Size = new System.Drawing.Size(39, 39);
            this.chunk1Checkbox.TabIndex = 19;
            this.chunk1Checkbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chunk1Checkbox.UseVisualStyleBackColor = false;
            // 
            // chunk2Checkbox
            // 
            this.chunk2Checkbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.chunk2Checkbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.chunk2Checkbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chunk2Checkbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chunk2Checkbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.chunk2Checkbox.Location = new System.Drawing.Point(425, 387);
            this.chunk2Checkbox.Margin = new System.Windows.Forms.Padding(0);
            this.chunk2Checkbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.chunk2Checkbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.chunk2Checkbox.Name = "chunk2Checkbox";
            this.chunk2Checkbox.Size = new System.Drawing.Size(39, 39);
            this.chunk2Checkbox.TabIndex = 20;
            this.chunk2Checkbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chunk2Checkbox.UseVisualStyleBackColor = false;
            // 
            // chunk3Checkbox
            // 
            this.chunk3Checkbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.chunk3Checkbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.chunk3Checkbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chunk3Checkbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chunk3Checkbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.chunk3Checkbox.Location = new System.Drawing.Point(425, 445);
            this.chunk3Checkbox.Margin = new System.Windows.Forms.Padding(0);
            this.chunk3Checkbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.chunk3Checkbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.chunk3Checkbox.Name = "chunk3Checkbox";
            this.chunk3Checkbox.Size = new System.Drawing.Size(39, 39);
            this.chunk3Checkbox.TabIndex = 21;
            this.chunk3Checkbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chunk3Checkbox.UseVisualStyleBackColor = false;
            // 
            // chunk4Checkbox
            // 
            this.chunk4Checkbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.chunk4Checkbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.chunk4Checkbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chunk4Checkbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chunk4Checkbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.chunk4Checkbox.Location = new System.Drawing.Point(425, 503);
            this.chunk4Checkbox.Margin = new System.Windows.Forms.Padding(0);
            this.chunk4Checkbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.chunk4Checkbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.chunk4Checkbox.Name = "chunk4Checkbox";
            this.chunk4Checkbox.Size = new System.Drawing.Size(39, 39);
            this.chunk4Checkbox.TabIndex = 22;
            this.chunk4Checkbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.chunk4Checkbox.UseVisualStyleBackColor = false;
            // 
            // mtlCheckbox
            // 
            this.mtlCheckbox.Appearance = System.Windows.Forms.Appearance.Button;
            this.mtlCheckbox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.mtlCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.mtlCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mtlCheckbox.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.mtlCheckbox.Location = new System.Drawing.Point(425, 561);
            this.mtlCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.mtlCheckbox.MaximumSize = new System.Drawing.Size(39, 39);
            this.mtlCheckbox.MinimumSize = new System.Drawing.Size(39, 39);
            this.mtlCheckbox.Name = "mtlCheckbox";
            this.mtlCheckbox.Size = new System.Drawing.Size(39, 39);
            this.mtlCheckbox.TabIndex = 23;
            this.mtlCheckbox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.mtlCheckbox.UseVisualStyleBackColor = false;
            // 
            // LevelExportWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 681);
            this.Controls.Add(this.mtlCheckbox);
            this.Controls.Add(this.chunk4Checkbox);
            this.Controls.Add(this.chunk3Checkbox);
            this.Controls.Add(this.chunk2Checkbox);
            this.Controls.Add(this.chunk1Checkbox);
            this.Controls.Add(this.chunk0Checkbox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.mobiesCheckbox);
            this.Controls.Add(this.shrubsCheckbox);
            this.Controls.Add(this.tiesCheckbox);
            this.Controls.Add(this.exportProgressBar);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.meshModeCombobox);
            this.Controls.Add(this.exportButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 720);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 720);
            this.Name = "LevelExportWindow";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Level Export Window";
            this.FormClosing += new FormClosingEventHandler(enableMain);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.SaveFileDialog exportLevelDialog;
        private System.Windows.Forms.ComboBox meshModeCombobox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ProgressBar exportProgressBar;
        private System.Windows.Forms.CheckBox tiesCheckbox;
        private System.Windows.Forms.CheckBox shrubsCheckbox;
        private System.Windows.Forms.CheckBox mobiesCheckbox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chunk0Checkbox;
        private System.Windows.Forms.CheckBox chunk1Checkbox;
        private System.Windows.Forms.CheckBox chunk2Checkbox;
        private System.Windows.Forms.CheckBox chunk3Checkbox;
        private System.Windows.Forms.CheckBox chunk4Checkbox;
        private System.Windows.Forms.CheckBox mtlCheckbox;
    }
}
