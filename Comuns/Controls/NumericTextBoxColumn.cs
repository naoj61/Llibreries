using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NCalc;

namespace Controls
{
    public class NumericTextBoxColumn : DataGridViewTextBoxColumn
    {
        public NumericTextBoxColumn()
            : base()
        {
            // Assegurar que només es permeten números en la cel·la
            this.CellTemplate = new NumericCell();
        }
    }



    public class NumericCell : DataGridViewTextBoxCell
    {
        #region ***** Events *****

        public struct CellPosition
        {
            public int Row { get; set; }
            public int Column { get; set; }

            public CellPosition(int row, int column) : this()
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

        private static readonly Dictionary<CellPosition, string> Formules = new Dictionary<CellPosition,string>(); // Guardem les fórmules per posició

        public static void CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            ValorTipus = typeof(string);

            var dataGridView = (DataGridView)sender;

            // Si hi ha una fórmula guardada per aquesta cel·la, la restaurem per a l'edició
            CellPosition cellPos = new CellPosition(e.RowIndex, e.ColumnIndex);
            if (Formules.ContainsKey(cellPos))
            {
                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Formules[cellPos];
            }
        }

        public static void CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var dataGridView = (DataGridView) sender;

            var cell = dataGridView[e.ColumnIndex, e.RowIndex];

            if (cell.Value != null)
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
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = result;
                        
                        cell.Style.ForeColor = Convert.ToDecimal(cell.Value) < 0 ? Color.Red : Color.Black;
                    }
                    catch (Exception ex)
                    {
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "#Error";
                        //MessageBox.Show("Error en la fórmula: " + ex.Message);
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

            ValorTipus = typeof(decimal);

        }

        /// <summary>
        /// Formateja les columnes numèriques. Si l'AutoSizeMode de la columna és Fill, no funciona!!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            FormatejaCela(((DataGridView) sender)[e.ColumnIndex, e.RowIndex]);
        }

        public static void FormatejaCela(DataGridViewCell cell)
        {
            // Si és negatiu, establim el color del text a vermell
            if (cell.Value != null && cell is NumericCell && Char.IsDigit(cell.Value.ToString()[0]))
            {
                cell.Style.ForeColor = Convert.ToDecimal(cell.Value) < 0 ? Color.Red : Color.Black;
            }
        }

        #endregion

        public override Type EditType
        {
            get { return typeof (NumericEditingControl); }
        }

        private static Type ValorTipus = typeof (decimal); // Quan edito ha de ser String per posar formules, però ha de ser decimal pel format final.
        public override Type ValueType
        {
            get { return ValorTipus; }
        }

        public override object DefaultNewRowValue
        {
            get { return null; } // Valor per defecte per a noves files
        }
    }

    public class NumericEditingControl : DataGridViewTextBoxEditingControl
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
                case Keys.OemMinus: // Si vols permetre el signe negatiu (-)
                case Keys.Decimal: // Si vols permetre el punt decimal (.)
                    return true;
                default:
                    return false;
            }
        }
    }
}
