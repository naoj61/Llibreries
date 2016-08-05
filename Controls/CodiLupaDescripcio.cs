using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Controls
{
    [DefaultEvent("Click")]
    public abstract partial class ACodiLupaDescripcio : UserControl
    {
        public event EventHandler Click;
        public new event EventHandler Leave;
        public event KeyEventHandler BuscaCodi;

        private const int Alçada = 56;

        protected ACodiLupaDescripcio()
        {
            InitializeComponent();
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

        public string Titol
        {
            get { return groupBox1.Text; }
            set { groupBox1.Text = value; }
        }

        public abstract int AmpladaControlCodi { get; set; }

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
            get { return tbCodiNumeric.Font; }
            set
            {
                tbCodiNumeric.Font = value;
                tbCodiText.Font = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Not used anymore", true)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }


        private void btLupa_Click(object sender, EventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

        private void tbCodiText_Leave(object sender, EventArgs e)
        {
            if (Leave != null)
            {
                Leave(this, e);
            }
        }

        private void tbCodiNumeric_Leave(object sender, EventArgs e)
        {
            if (Leave != null)
            {
                Leave(this, e);
            }
        }

        private void tbCodiText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (BuscaCodi != null)
                {
                    BuscaCodi(this, e);
                }
            }
        }

        private void tbCodiNumeric_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (BuscaCodi != null)
                {
                    BuscaCodi(this, e);
                }
            }
        }
    }




    public partial class CodiLupaDescripcioNum : ACodiLupaDescripcio
    {
        public CodiLupaDescripcioNum() :base()
        {
            base.Titol = "CodiLupaDescripcioNum";
            tbCodiNumeric.Visible = true;
            tbCodiText.Visible = false;
        }

        public double? CodiNumeric
        {
            get { return tbCodiNumeric._DoubleValue; }
            set { tbCodiNumeric.Valor = value.HasValue ? value.Value : 0; }
        }

        public override int AmpladaControlCodi {
            get { return tbCodiNumeric.Width; }
            set { tbCodiNumeric.Width = value; }
        }
    }


    public partial class CodiLupaDescripcioText : ACodiLupaDescripcio
    {
        public CodiLupaDescripcioText()
        {
            base.Titol = "CodiLupaDescripcioText";
            tbCodiNumeric.Visible = false;
            tbCodiText.Visible = true;
        }

        public string TextCodi
        {
            get { return tbCodiText.Text; }
            set { tbCodiText.Text = value; }
        }

        public string TextDescripcio
        {
            get { return tbDescripcio.Text; }
            set { tbDescripcio.Text = value; }
        }

        public override int AmpladaControlCodi
        {
            get { return tbCodiText.Width; }
            set { tbCodiText.Width = value; }
        }
    }
}
