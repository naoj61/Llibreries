using System;
using System.Windows.Forms;

namespace Comuns
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
        public NumericCell()
            : base()
        {
            // Assegurar que només es permeten números a la cel·la
            this.Style.Format = "N2"; // Format per als números (opcional)
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
