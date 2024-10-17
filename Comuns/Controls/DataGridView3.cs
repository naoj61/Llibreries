using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Comuns;
using NCalc;

namespace Controls
{
    public class DataGridView3 : DataGridView
    {
        private struct CellPosition
        {
            public int Row { get; set; }
            public int Column { get; set; }

            public CellPosition(int row, int column)
                : this()
            {
                Row = row;
                Column = column;
            }

            // Sobreescrivim GetHashCode i Equals perquè funcioni correctament com a clau del diccionari
            public override bool Equals(object obj)
            {
                if (obj is CellPosition)
                {
                    var other = (CellPosition) obj;
                    return this.Row == other.Row && this.Column == other.Column;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Row.GetHashCode() ^ Column.GetHashCode();
            }
        }

        private static readonly Dictionary<CellPosition, string> Formules = new Dictionary<CellPosition, string>(); // Guardem les fórmules per posició
        private TextBox vTextBoxEnEdicio;

        #region ***** Controla la edició de cel·les numèriques *****

        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            base.OnEditingControlShowing(e);

            if (CurrentCell.OwningColumn is NumericTextBoxColumn2)
            {
                // Obté el control d'edició actual
                vTextBoxEnEdicio = e.Control as TextBox;

                if (vTextBoxEnEdicio != null)
                {
                    // Només apliquem la validació a les cel·les de columnes del tipus"NumericTextBoxColumn"
                    // Afegim un esdeveniment de validació de text numèric quan es produeix l'entrada
                    //vTextBoxEnEdicio.KeyPress += numericTextBox_KeyPress;
                }
            }
            else
                vTextBoxEnEdicio = null;
        }

        protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            base.OnCellBeginEdit(e);

            // Si hi ha una fórmula guardada per aquesta cel·la, la restaurem per a l'edició
            CellPosition cellPos = new CellPosition(e.RowIndex, e.ColumnIndex);
            if (Formules.ContainsKey(cellPos))
            {
                Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Formules[cellPos];
            }

            CurrentCell.Style.ForeColor = Color.Black;
        }

        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            base.OnCellEndEdit(e);

            //if (vTextBoxEnEdicio != null)
            // Cancelem l'esdeveniment de validació de text numèric quan es produeix l'entrada
            //vTextBoxEnEdicio.KeyPress -= numericTextBox_KeyPress;

            var cell = this[e.ColumnIndex, e.RowIndex];

            if (cell.Value != null && cell.OwningColumn is NumericTextBoxColumn2)
            {
                string cellValue = cell.Value.ToString();

                if (cellValue.StartsWith("=") || cellValue.StartsWith("+") || cellValue.StartsWith("-") || Char.IsDigit(cellValue[0]))
                {
                    // Guardem la fórmula
                    CellPosition cellPos = new CellPosition(e.RowIndex, e.ColumnIndex);
                    Formules[cellPos] = cellValue;

                    try
                    {
                        string expression = cellValue;

                        if (cellValue.StartsWith("=") || cellValue.StartsWith("+"))
                            expression = expression.Substring(1); // Treu el signe "=" o "+".

                        expression = expression.Replace(',', '.'); // Converteix la comma en punt.

                        Expression formula = new Expression(expression);
                        var result = Convert.ToDecimal(formula.Evaluate());
                        Rows[e.RowIndex].Cells[e.ColumnIndex].Value = result;
                        Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = String.Empty;

                        cell.Style.ForeColor = Convert.ToDecimal(cell.Value) < 0 ? Color.Red : Color.Black;
                    }
                    catch (Exception ex)
                    {
                        Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = "#Error";
                    }
                }
                else
                {
                    // Si no és una fórmula, eliminem el valor guardat
                    CellPosition cellPos = new CellPosition(e.RowIndex, e.ColumnIndex);
                    if (Formules.ContainsKey(cellPos))
                    {
                        Formules.Remove(cellPos);
                    }
                }
            }
        }

        /*
        private void numericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!caracterValid(e.KeyChar, CurrentCell.ColumnIndex))
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

        /// <summary>
        /// Comprova si s'accepta el caracter.
        /// </summary>
        /// <param name="car"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        private bool caracterValid(char car, int columnIndex)
        {
            var col = (NumericTextBoxColumn2) Columns[columnIndex];

            if (col._AcceptaFormules)
            {
                if (!char.IsControl(car) && !char.IsDigit(car) && car != ',' && car != '.' && car != '-'
                    && car != '=' && car != '+' && car != '*' && car != '/')
                {
                    return false; // S'ignora el caràcter
                }
            }
            else
            {
                if (!char.IsControl(car) && !char.IsDigit(car) && car != ',' && car != '.' && car != '-')
                {
                    return false; // S'ignora el caràcter
                }
            }
            return true;
        }
        */

        #endregion ***** Controla la edició de cel·les numèriques *****


        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            base.OnCellFormatting(e);

            var cell = this[e.ColumnIndex, e.RowIndex];

            if (cell.Value != null
                && cell.OwningColumn is NumericTextBoxColumn2 && cell.ErrorText == String.Empty)
            {
                decimal retNum;
                var esNumeric = Decimal.TryParse(Convert.ToString(cell.Value), NumberStyles.Number, NumberFormatInfo.InvariantInfo, out retNum);
                if (esNumeric && retNum < 0 && ((NumericTextBoxColumn2) this.Columns[e.ColumnIndex])._NegatiusEnVermell)
                    cell.Style.ForeColor = Color.Red;
                else
                    cell.Style.ForeColor = Color.Black;
            }
        }

        protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
        {
            DataGridViewCell cella = this[e.ColumnIndex, e.RowIndex];

            if (e.Exception is FormatException && cella.OwningColumn is NumericTextBoxColumn2) // && cella.ValueType.EsTipusNumeric())
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


    #region ***** NumericTextBoxColumn2 *****

    /// <summary>
    /// DataGridViewTextBoxColumn per cel·les numériques.
    /// </summary>
    public class NumericTextBoxColumn2 : DataGridViewTextBoxColumn
    {
        public NumericTextBoxColumn2() : base()
        {
            _NegatiusEnVermell = true;

            // Assegurar que només es permeten números en la cel·la
            this.CellTemplate = new NumericCell2();
        }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool _NegatiusEnVermell { get; set; }

        public override object Clone()
        {
            var clone = (NumericTextBoxColumn2) base.Clone();
            
// ReSharper disable once PossibleNullReferenceException
            clone._NegatiusEnVermell = this._NegatiusEnVermell;

            return clone;
        }
    }

    /// <summary>
    /// DataGridViewTextBoxColumn per cel·les numériques que admeten fómules.
    /// </summary>
    public class NumericTextBoxColumn2F : NumericTextBoxColumn2
    {
        public NumericTextBoxColumn2F()
            : base()
        {
            // Assegurar que només es permeten números en la cel·la
            this.CellTemplate = new NumericCell2F();
        }
    }


    /// <summary>
    /// DataGridViewTextBoxCell per cel·les numériques.
    /// </summary>
    public class NumericCell2 : DataGridViewTextBoxCell
    {
        public override Type EditType
        {
            get { return typeof (NumericEditingControl2); }
        }

        public override Type ValueType
        {
            get { return typeof (decimal); }
        }

        public override object DefaultNewRowValue
        {
            get { return null; } // Valor per defecte per a noves files
        }
    }

    /// <summary>
    /// DataGridViewTextBoxCell per cel·les numériques que admeten fómules.
    /// </summary>
    public class NumericCell2F : NumericCell2
    {
        public override Type EditType
        {
            get { return typeof (NumericEditingControl2F); }
        }

        public override Type ValueType
        {
            get { return typeof (string); }
        }
    }

    /// <summary>
    /// DataGridViewTextBoxEditingControl per cel·les numériques.
    /// </summary>
    public class NumericEditingControl2 : DataGridViewTextBoxEditingControl
    {
        protected override bool IsInputKey(Keys keyData)
        {
            // Permetre que les tecles de navegació i edició siguin acceptades
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                case Keys.Delete:
                case Keys.Back:
                    return true;
                default:
                    return false;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.' && e.KeyChar != '-')
            {
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// DataGridViewTextBoxEditingControl per cel·les numériques que admeten fómules.
    /// </summary>
    public class NumericEditingControl2F : NumericEditingControl2
    {
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            /*
             * Si e.Handled = true. Vol dir que no ha passat el filtre base, ara comprovo si passa el filtre Formula.
             * Si e.Handled = false. Vol dir que ha passat el filtre base, aquesta línia també donarà false.
             */

            e.Handled = e.Handled && e.KeyChar != '=' && e.KeyChar != '+' && e.KeyChar != '*' && e.KeyChar != '/';
        }
    }

    #endregion

}