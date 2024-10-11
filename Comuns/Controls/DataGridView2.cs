using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Comuns;

namespace Controls
{
    internal partial class DataGridView2 : DataGridView
    {
        public DataGridView2()
        {
            InitializeComponent();
        }

        private TextBox vTextBoxEnEdicio;
        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            base.OnEditingControlShowing(e);

            if (Columns[CurrentCell.ColumnIndex] is NumericTextBoxColumn)
            {
                // Obté el control d'edició actual
                vTextBoxEnEdicio = e.Control as TextBox;

                if (vTextBoxEnEdicio != null)
                {
                    // Només apliquem la validació a les cel·les de columnes del tipus"NumericTextBoxColumn"
                    // Afegim un esdeveniment de validació de text numèric quan es produeix l'entrada
                    vTextBoxEnEdicio.KeyPress += numericTextBox_KeyPress;
                }
            }
            else
                vTextBoxEnEdicio = null;
        }

        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            base.OnCellEndEdit(e);

            if (vTextBoxEnEdicio != null)
                // Cancelem l'esdeveniment de validació de text numèric quan es produeix l'entrada
                vTextBoxEnEdicio.KeyPress -= numericTextBox_KeyPress;
        }


        /// <summary>
        /// Només permetem dígits, coma i tecla de retrocés
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Només permetem dígits, coma i tecla de retrocés
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true; // S'ignora el caràcter
            }

            if (e.KeyChar == '.')
                e.KeyChar = ','; // Converteix el punt en coma

            // Només permetem una coma decimal
            if (e.KeyChar == ',')
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null && textBox.Text.Contains(","))
                {
                    e.Handled = true; // S'ignora la coma si ja existeix una
                }
            }
        }

        protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
        {
            DataGridViewCell cella = this[e.ColumnIndex, e.RowIndex];

            if (e.Exception is FormatException && cella.OwningColumn is NumericTextBoxColumn && cella.ValueType.EsTipusNumeric())
            {
                // Aquí pots gestionar l'error de dades com vulguis
                MessageBox.Show("Error de dades: " + e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Si vols controlar l'error i evitar que es propagui, pots fer-ho assignant la propietat Handled a true
                e.ThrowException = false; // Opcional: indiques que ja has gestionat l'error
                e.Cancel = true; // Opcional: indiques que cancel·les l'operació que va provocar l'error
            }
            else
            {
                base.OnDataError(displayErrorDialogIfNoHandler, e);
            }
        }

    }
}
