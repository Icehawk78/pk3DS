using System.ComponentModel;

namespace pk3DS
{
    partial class GameSummary7
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.RTB_Summary = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            this.RTB_Summary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RTB_Summary.Location = new System.Drawing.Point(0, 0);
            this.RTB_Summary.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.RTB_Summary.Name = "RTB_Summary";
            this.RTB_Summary.ReadOnly = true;
            this.RTB_Summary.Size = new System.Drawing.Size(522, 432);
            this.RTB_Summary.TabIndex = 8;
            this.RTB_Summary.Text = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 432);
            this.Controls.Add(this.RTB_Summary);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "GameSummary7";
            this.Text = "GameSummary7";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox RTB_Summary;
    }
}