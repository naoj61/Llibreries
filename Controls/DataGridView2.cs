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
    public partial class DataGridView2 : DataGridView
    {
        public DataGridView2()
        {
            InitializeComponent();
        }

        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            base.OnEditingControlShowing(e);

            // Obté el control d'edició actual
            TextBox textBox = e.Control as TextBox;

            if (textBox != null)
            {
                // Només apliquem la validació a les cel·les de columnes del tipus"NumericTextBoxColumn"
                if (this.Columns[this.CurrentCell.ColumnIndex] is NumericTextBoxColumn)
                {
                    // Afegim un esdeveniment de validació de text quan es produeix l'entrada
                    textBox.KeyPress -= numericTextBox_KeyPress;
                    textBox.KeyPress += numericTextBox_KeyPress;
                }
                else
                {
                    textBox.KeyPress -= numericTextBox_KeyPress;
                }
            }
        }

        protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
        {
            DataGridViewCell cella = this[e.ColumnIndex, e.RowIndex];

            if (e.Exception is FormatException && cella.OwningColumn is NumericTextBoxColumn && cella.ValueType.IsNumericType())
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

    }


    /// <summary>
    /// Crea un mètode d'extensió de la classe Type
    /// </summary>
    public static class TypeExtensions
    {
        public static bool IsNumericType(this Type type)
        {
            if (type == null)
                return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
