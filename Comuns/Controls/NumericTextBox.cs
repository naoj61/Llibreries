using System;
using System.Globalization;
using System.Windows.Forms;

namespace Controls
{
    public class NumericTextBox : TextBox
    {
        public NumericTextBox()
        {
            _Format = "#.#";
            TextAlign = HorizontalAlignment.Right;
            _PermetNegatius = true;
            _PermetDecimals = true;
            _PermetEspais = false;
        }

        // Desa el format original de Text, abans d'aplicar el format.
        private string vTextAnt = null;

        public string _Format { get; set; }

        public int _IntValue
        {
            get { return String.IsNullOrEmpty(Text) ? 0 : Int32.Parse(Text); }
            set { Text = value.ToString(); }
        }

        public decimal _DecimalValue
        {
            get { return String.IsNullOrEmpty(Text) ? 0 : Decimal.Parse(Text); }
            set { Text = value.ToString(); }
        }

        public double _DoubleValue
        {
            get { return String.IsNullOrEmpty(Text) ? 0 : Double.Parse(Text); }
            set { Text = value.ToString(); }
        }

        public bool _PermetEspais { get; set; }

        public bool _PermetNegatius { get; set; }

        public bool _PermetDecimals { get; set; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                Text = vTextAnt;

                e.SuppressKeyPress = true;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            NumberFormatInfo numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
            char decimalSeparator = Convert.ToChar(numberFormatInfo.NumberDecimalSeparator);
            char groupSeparator = Convert.ToChar(numberFormatInfo.NumberGroupSeparator);
            char negativeSign = Convert.ToChar(numberFormatInfo.NegativeSign);

            if (e.KeyChar == groupSeparator)
                e.KeyChar = decimalSeparator;

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (e.KeyChar == decimalSeparator && Text.IndexOf(decimalSeparator) == -1 && _PermetDecimals)
            {
                // Decimal separator is OK
            }
            else if (e.KeyChar.Equals(negativeSign) && Text.IndexOf(negativeSign) == -1 && _PermetNegatius)
            {
                // Negative sign is OK
             
                // Poso el signe al principi.
                int pos = SelectionStart + 1;
                base.Text = negativeSign + Text;
                Select(pos, 0);
                e.Handled = true;
                return;
            }
            else if (e.KeyChar == '\b')
            {
                // Backspace key is OK
            }
            else if (_PermetEspais && e.KeyChar == ' ')
            {
                // Space key is OK
            }
            else
            {
                // 
                e.Handled = true;
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            vTextAnt = Text;
            base.Text = _DecimalValue.ToString(_Format);
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            base.Text = vTextAnt;
        }

        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                vTextAnt = value;
            }
        }

    }
}
