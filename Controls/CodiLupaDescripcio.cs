using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Controls
{
    [DefaultEvent("Click")]
    public abstract partial class ACodiLupaDescripcio : UserControl
    {
        public abstract int AmpladaControlCodi { get; set; }
        public abstract void focusCodi();
        public abstract string _Codi { get; }

        public event EventHandler Click;
        public new event EventHandler Leave;
        public event KeyEventHandler BuscaCodi;
        public new event EventHandler Canviat;

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

        [Browsable(true)]
        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                btLupa.Visible = value;
            }
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


        #region *** Events ***

        private void btLupa_Click(object sender, EventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }

            /* 
             * Després de clicar la lupa torno a enfocar el codi.
             * Això ho faig per si el codi no és correcte i la lupa es cancela no s'escapi el codi erroni.
             */
            focusCodi();
        }

        private void tb_Leave(object sender, EventArgs e)
        {
            if (ActiveControl != null && ActiveControl.Equals(btLupa))
            {
                // Si s'ha clicat la lupa, no llença el Leave.
                // Perquè funcioni: btLupa.TabStop = false
                return;
            }

            if (Leave != null)
            {
                Leave(this, e);
            }
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (BuscaCodi != null)
                {
                    BuscaCodi(this, e);
                }
            }
        }

        private void tbCodiText_TextChanged(object sender, EventArgs e)
        {
            if (Canviat != null)
            {
                Canviat(this, e);
            }
        }

        private void tbCodiNumeric_TextChanged(object sender, EventArgs e)
        {
            if (Canviat != null)
            {
                Canviat(this, e);
            }
        }


        private string vCodiTextAct = null;
        private void tbCodiText_Enter(object sender, EventArgs e)
        {
            vCodiTextAct = tbCodiText.Text;
        }

        private double vCodiNumericAct = 0;
        private void tbCodiNumeric_Enter(object sender, EventArgs e)
        {
            vCodiNumericAct = tbCodiNumeric.Valor;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                // *** Procesa la tecla ESC, per alguna raó, no es dispara "KeyDown".
                // *** Restaura valor codi.
                if (tbCodiNumeric.Focused)
                    tbCodiNumeric.Valor = vCodiNumericAct;
                else if (tbCodiText.Focused)
                    tbCodiText.Text = vCodiTextAct;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


#endregion *** Events ***

        /// <summary>
        /// Obre la finestra de seleccio del ERP corresponent a "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filtreSeleccio">Filtre que s'aplicarà a la selecció.</param>
        public bool ObreFinestraSeleccio<T>(string filtreSeleccio = "") where T : ICodiLupaDesc
        {
            var cursor = this.Cursor;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                bool result = false;
                var cli = (ICodiLupaDesc)typeof(T).GetMethod("Seleccionar").Invoke(null, new object[] { filtreSeleccio });
                if (cli != null) // Si és null és perquè s'ha cancelat la cerca de proveidor.
                {
                    tbCodiText.Text = cli._Clau;
                    tbDescripcio.Text = cli._Desc;
                    result = true;
                }
                return result;
            }
            finally
            {
                this.Cursor = cursor;
            }
        }


        /// <summary>
        /// Cerca un element del tipus "T" en el ERP.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obreFinestraSeleccioSiNoTrobaCodi">Indica si s'obrirà la finestra de selecció en cas de no trobar l'element.</param>
        /// <param name="filtreSeleccio">Filtre que s'aplicarà a la selecció.</param>
        public bool BuscaElement<T>(bool obreFinestraSeleccioSiNoTrobaCodi, string filtreSeleccio = "") where T : ICodiLupaDesc
        {
            if (String.IsNullOrWhiteSpace(_Codi))
            {
                tbDescripcio.Text = null;
                return true;
            }

            bool result = false;
            var cursor = this.Cursor;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                var cli = (ICodiLupaDesc)typeof(T).GetMethod("Buscar").Invoke(null, new object[] { _Codi, filtreSeleccio });
                if (cli == null)
                {
                    tbDescripcio.Text = null;

                    var missatge = String.Format("Código de {0} no existe.", Titol);
                    if (obreFinestraSeleccioSiNoTrobaCodi)
                    {
                        if (MessageBox.Show(missatge + " Quiere abrir la ventana de selección?", "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            result = ObreFinestraSeleccio<T>(filtreSeleccio);
                    }
                    else
                        MessageBox.Show(missatge, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    tbCodiText.Text = cli._Clau;
                    tbDescripcio.Text = cli._Desc;
                    result = true;
                }
            }
            finally
            {
                this.Cursor = cursor;
            }

            return result;
        }

    }




    public class CodiLupaDescripcioNum : ACodiLupaDescripcio
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

        public override void focusCodi()
        {
            tbCodiNumeric.Focus();
        }

        public override string _Codi
        {
            get { return tbCodiNumeric._DoubleValue.ToString(); }
        }
    }

    public class CodiLupaDescripcioText : ACodiLupaDescripcio
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

        public override void focusCodi()
        {
            tbCodiText.Focus();
        }

        public override string _Codi
        {
            get { return tbCodiText.Text; }
        }
    }
}

