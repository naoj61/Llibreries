using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Controls
{
    public partial class Etiqueta1 : UserControl
    {
        public Etiqueta1()
        {
            InitializeComponent();
        }

        public string Titol
        {
            get { return groupBox1.Text; }
            set { groupBox1.Text = value; }
        }

        [Browsable(false)]
        public override Font Font
        {
            get { return base.Font; }
            set { base.Font = value; }
        }

        [Browsable(true)]
        public Font FontTitol
        {
            get { return groupBox1.Font; }
            set { groupBox1.Font = value; }
        }

        [Browsable(true)]
        public Font FontText
        {
            get { return label1.Font; }
            set { label1.Font = value; }
        }

        private decimal vTextN;

        [Browsable(true)]
        public string TextA
        {
            get { return label1.Text; }
            set
            {
                try
                {
                    vTextN = Convert.ToDecimal(value);
                    label1.Text = vTextN.ToString(Mascara);
                    label1.RightToLeft = RightToLeft.Yes;
                }
                catch (FormatException)
                {
                    label1.RightToLeft = RightToLeft.No;
                    label1.Text = value;
                    vTextN = 0;
                }
            }
        }


        private const int Alçada = 40;


        public override Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = new Size(value.Width, Alçada); }
        }

        public override Size MaximumSize
        {
            get { return base.MaximumSize; }
            set { base.MaximumSize = new Size(value.Width, Alçada); }
        }


        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public string Mascara { get; set; }

    }
}
