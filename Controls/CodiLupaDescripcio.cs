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


        /// <summary>
        /// Obre la finestra de seleccio del ERP corresponent a "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filtreSeleccio">Filtre que s'aplicarà a la selecció.</param>
        public void ObreFinestraSeleccio<T>(string filtreSeleccio = "") where T : ICodiLupaDesc
        {
            var cli = (ICodiLupaDesc) typeof (T).GetMethod("Seleccionar").Invoke(null, new object[] {filtreSeleccio});
            if (cli != null) // Si és null és perquè s'ha cancelat la cerca de proveidor.
            {
                tbCodiText.Text = cli._Clau;
                tbDescripcio.Text = cli._Desc;
            }
        }


        /// <summary>
        /// Cerca un element del tipus "T" en el ERP.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codi">Clau de cerca.</param>
        /// <param name="obreFinestraSeleccioSiNoTrobaCodi">Indica si s'obrirà la finestra de selecció en cas de no trobar l'element.</param>
        /// <param name="filtreSeleccio">Filtre que s'aplicarà a la selecció.</param>
        public void BuscaElement<T>(string codi, bool obreFinestraSeleccioSiNoTrobaCodi, string filtreSeleccio = "") where T : ICodiLupaDesc
        {
            var cli = (ICodiLupaDesc) typeof (T).GetMethod("Buscar").Invoke(null, new object[] {codi});
            if (cli == null)
            {
                tbDescripcio.Text = null;

                if (obreFinestraSeleccioSiNoTrobaCodi)
                {
                    if(MessageBox.Show("Código no existe. Quiere abrir la ventana de selección?", "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        ObreFinestraSeleccio<T>(filtreSeleccio);
                }
                else
                    MessageBox.Show("Código no existe", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                tbCodiText.Text = cli._Clau;
                tbDescripcio.Text = cli._Desc;
            }
        }
    }




    public partial class CodiLupaDescripcioNum : ACodiLupaDescripcio
    {
        public CodiLupaDescripcioNum() : base()
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

        public override int AmpladaControlCodi
        {
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

