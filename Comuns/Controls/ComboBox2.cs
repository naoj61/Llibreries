using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Controls
{
    /// <summary>
    /// Amplia ComboBox Amb la propietat "Modified" i el mètode "Undo()".
    /// </summary>
    public partial class ComboBox2 : ComboBox, IValorControlRestaurable
    {
        private int vIndexAnterior = -1;
        private bool vSeleccioUsuari = false;

        public override int SelectedIndex
        {
            get { return base.SelectedIndex; }
            set
            {
                if (vSeleccioUsuari)
                {
                    vSeleccioUsuari = false;
                }
                else
                {
                    vIndexAnterior = value;
                    Modified = false;
                }
                
                base.SelectedIndex = value; // Ha d'anar al final.
            }
        }


        /// <summary>
        /// Es dispara només quan l'usuari fa un canvi.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChangeCommitted(EventArgs e)
        {
            base.OnSelectionChangeCommitted(e);

            vSeleccioUsuari = true;
            Modified = true;
        }

        /// <summary>
        /// Indica si l'usuari ha fet algun canvi.
        /// </summary>
        [Browsable(false)]
        public bool Modified { get; private set; }

        /// <summary>
        /// Restaura a l'ultima selecció feta per programa.
        /// </summary>
        public void Undo()
        {
            base.SelectedIndex = vIndexAnterior;
        }
    }
}
