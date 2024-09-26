using System;
using System.Drawing;
using System.Windows.Forms;

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
        /// <summary>
        /// Formateja les columnes numèriques. Si l'AutoSizeMode de la columna és Fill, no funciona!!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var cell = ((DataGridView)sender)[e.ColumnIndex, e.RowIndex];

            // Si és negatiu, establim el color del text a vermell
            if (cell is NumericCell)
            {
                // ***** Si l'AutoSizeMode de la columna és Fill, no funciona!!!! *****
                cell.Style.ForeColor = Convert.ToDecimal(cell.Value) < 0 ? Color.Red : Color.Black;
            }
        }

        public override Type EditType
        {
            get { return typeof(NumericEditingControl); }
        }

        public override Type ValueType
        {
            get { return typeof(decimal); }
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
                case Keys.Decimal:  // Si vols permetre el punt decimal (.)
                    return true;
                default:
                    return false;
            }
        }
    }
}
