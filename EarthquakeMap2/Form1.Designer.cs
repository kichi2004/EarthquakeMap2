namespace EarthquakeMap2
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripInformationTestIndex = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripInformationTest = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripInformationTestEew = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1296, 749);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripInformationTestIndex});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(125, 26);
            // 
            // toolStripInformationTestIndex
            // 
            this.toolStripInformationTestIndex.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripInformationTest,
            this.toolStripInformationTestEew});
            this.toolStripInformationTestIndex.Name = "toolStripInformationTestIndex";
            this.toolStripInformationTestIndex.Size = new System.Drawing.Size(124, 22);
            this.toolStripInformationTestIndex.Text = "情報テスト";
            // 
            // toolStripInformationTest
            // 
            this.toolStripInformationTest.Name = "toolStripInformationTest";
            this.toolStripInformationTest.Size = new System.Drawing.Size(146, 22);
            this.toolStripInformationTest.Text = "地震情報";
            // 
            // toolStripInformationTestEew
            // 
            this.toolStripInformationTestEew.Name = "toolStripInformationTestEew";
            this.toolStripInformationTestEew.Size = new System.Drawing.Size(146, 22);
            this.toolStripInformationTestEew.Text = "緊急地震速報";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1296, 749);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox pictureBox1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripInformationTestIndex;
        private ToolStripMenuItem toolStripInformationTest;
        private ToolStripMenuItem toolStripInformationTestEew;
    }
}