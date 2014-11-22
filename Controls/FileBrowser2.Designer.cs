namespace Controls
{
    partial class FileBrowser2
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileBrowser2));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvDirectoris = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.lvFitxers = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLastModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvDirectoris);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvFitxers);
            this.splitContainer1.Size = new System.Drawing.Size(736, 424);
            this.splitContainer1.SplitterDistance = 246;
            this.splitContainer1.TabIndex = 0;
            // 
            // tvDirectoris
            // 
            this.tvDirectoris.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDirectoris.ImageIndex = 0;
            this.tvDirectoris.ImageList = this.imageList1;
            this.tvDirectoris.Location = new System.Drawing.Point(0, 0);
            this.tvDirectoris.Name = "tvDirectoris";
            this.tvDirectoris.SelectedImageIndex = 0;
            this.tvDirectoris.Size = new System.Drawing.Size(246, 424);
            this.tvDirectoris.TabIndex = 0;
            this.tvDirectoris.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvDirectoris_NodeMouseClick);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Folder");
            this.imageList1.Images.SetKeyName(1, "File");
            // 
            // lvFitxers
            // 
            this.lvFitxers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderType,
            this.columnHeaderLastModified});
            this.lvFitxers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFitxers.Location = new System.Drawing.Point(0, 0);
            this.lvFitxers.Name = "lvFitxers";
            this.lvFitxers.Size = new System.Drawing.Size(486, 424);
            this.lvFitxers.SmallImageList = this.imageList1;
            this.lvFitxers.TabIndex = 0;
            this.lvFitxers.UseCompatibleStateImageBehavior = false;
            this.lvFitxers.View = System.Windows.Forms.View.Details;
            this.lvFitxers.SelectedIndexChanged += new System.EventHandler(this.lvFitxers_SelectedIndexChanged);
            this.lvFitxers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvFitxers_MouseDoubleClick);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Type";
            // 
            // columnHeaderLastModified
            // 
            this.columnHeaderLastModified.Text = "Last Modified";
            // 
            // FileBrowser2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FileBrowser2";
            this.Size = new System.Drawing.Size(736, 424);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvDirectoris;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView lvFitxers;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderLastModified;
    }
}
