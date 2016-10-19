namespace Controls
{
    abstract partial class ACodiLupaDescripcio
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ACodiLupaDescripcio));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tbCodiText = new System.Windows.Forms.TextBox();
            this.tbDescripcio = new System.Windows.Forms.TextBox();
            this.tbCodiNumeric = new Controls.NumericTextBox2();
            this.btLupa = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "MAG01A.ICO");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(300, 56);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CodiLupaDescripcio";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tbCodiText, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbDescripcio, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbCodiNumeric, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btLupa, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(294, 35);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tbCodiText
            // 
            this.tbCodiText.Location = new System.Drawing.Point(5, 7);
            this.tbCodiText.Margin = new System.Windows.Forms.Padding(5, 7, 3, 4);
            this.tbCodiText.Name = "tbCodiText";
            this.tbCodiText.Size = new System.Drawing.Size(86, 22);
            this.tbCodiText.TabIndex = 0;
            this.tbCodiText.TextChanged += new System.EventHandler(this.tbCodiText_TextChanged);
            this.tbCodiText.Enter += new System.EventHandler(this.tbCodiText_Enter);
            this.tbCodiText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_KeyDown);
            this.tbCodiText.Leave += new System.EventHandler(this.tb_Leave);
            // 
            // tbDescripcio
            // 
            this.tbDescripcio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDescripcio.Location = new System.Drawing.Point(232, 7);
            this.tbDescripcio.Margin = new System.Windows.Forms.Padding(4, 7, 5, 3);
            this.tbDescripcio.Name = "tbDescripcio";
            this.tbDescripcio.ReadOnly = true;
            this.tbDescripcio.Size = new System.Drawing.Size(57, 22);
            this.tbDescripcio.TabIndex = 3;
            // 
            // tbCodiNumeric
            // 
            this.tbCodiNumeric._Format = "#.#";
            this.tbCodiNumeric._PermetDecimals = true;
            this.tbCodiNumeric._PermetEspais = false;
            this.tbCodiNumeric._PermetNegatius = true;
            this.tbCodiNumeric.Location = new System.Drawing.Point(99, 7);
            this.tbCodiNumeric.Margin = new System.Windows.Forms.Padding(5, 7, 3, 4);
            this.tbCodiNumeric.Name = "tbCodiNumeric";
            this.tbCodiNumeric.Size = new System.Drawing.Size(86, 22);
            this.tbCodiNumeric.TabIndex = 1;
            this.tbCodiNumeric.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tbCodiNumeric.Valor = 0D;
            this.tbCodiNumeric.Visible = false;
            this.tbCodiNumeric.TextChanged += new System.EventHandler(this.tbCodiNumeric_TextChanged);
            this.tbCodiNumeric.Enter += new System.EventHandler(this.tbCodiNumeric_Enter);
            this.tbCodiNumeric.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tb_KeyDown);
            this.tbCodiNumeric.Leave += new System.EventHandler(this.tb_Leave);
            // 
            // btLupa
            // 
            this.btLupa.ImageKey = "MAG01A.ICO";
            this.btLupa.ImageList = this.imageList1;
            this.btLupa.Location = new System.Drawing.Point(191, 3);
            this.btLupa.Name = "btLupa";
            this.btLupa.Size = new System.Drawing.Size(34, 29);
            this.btLupa.TabIndex = 2;
            this.btLupa.TabStop = false;
            this.btLupa.UseVisualStyleBackColor = true;
            this.btLupa.Click += new System.EventHandler(this.btLupa_Click);
            // 
            // ACodiLupaDescripcio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.MaximumSize = new System.Drawing.Size(0, 56);
            this.MinimumSize = new System.Drawing.Size(300, 56);
            this.Name = "ACodiLupaDescripcio";
            this.Size = new System.Drawing.Size(300, 56);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btLupa;
        protected System.Windows.Forms.TextBox tbCodiText;
        protected NumericTextBox2 tbCodiNumeric;
        protected System.Windows.Forms.TextBox tbDescripcio;

    }
}
