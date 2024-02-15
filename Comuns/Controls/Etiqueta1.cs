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
        private const int Alçada = 40;
        private string vText;
        private double? vValor;


        public Etiqueta1()
        {
            InitializeComponent();

            Mascara = "#,##0.##";
        }

        [Browsable(true)]
        public string Titol
        {
            get { return groupBox1.Text; }
            set { groupBox1.Text = value; }
        }

        [Browsable(true)]
        public bool ReadOnly
        {
            get { return TextBox1.ReadOnly; }
            set { TextBox1.ReadOnly = value; }
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
            get { return TextBox1.Font; }
            set { TextBox1.Font = value; }
        }

        public double? Valor
        {
            get { return vValor; }
            set
            {
                if (value.HasValue)
                {
                    TextBox1.Text = value.Value.ToString(Mascara);
                    TextBox1.RightToLeft = RightToLeft.Yes;
                }
                else
                {
                    TextBox1.Text = null;
                }

                vText = TextBox1.Text;
                vValor = value;
            }
        }


        [Browsable(true)]
        public override string Text
        {
            get { return vText; }
            set
            {
                try
                {
                    Valor = Convert.ToDouble(value);
                }
                catch (FormatException)
                {
                    TextBox1.RightToLeft = RightToLeft.No;
                    TextBox1.Text = value;
                    vValor = null;
                }

                vText = value;
                //base.Text = value;
            }
        }

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
