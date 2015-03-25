using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Controls
{
    public class NumericTextBox2 : TextBox
    {
        public NumericTextBox2()
        {
            _Format = "#.#";
            TextAlign = HorizontalAlignment.Right;
            _PermetNegatius = true;
            _PermetDecimals = true;
            _PermetEspais = false;
        }

        // Desa el format original de Text, abans d'aplicar el format.
        private string vTextAnt = null;
        readonly static char DecimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        readonly static char GroupSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
        readonly static char NegativeSign = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NegativeSign);

        private double vValor;
        private bool vPaste;

        public string _Format { get; set; }

        [Browsable(false)]
        public int _IntValue
        {
            get { return (int) vValor; }
        }

        [Browsable(false)]
        public decimal _DecimalValue
        {
            get { return (decimal) vValor; }
        }

        [Browsable(false)]
        public double _DoubleValue
        {
            get { return vValor; }
        }

        public double Valor
        {
            get { return vValor; }
            set
            {
                // Deso el valor en base.Text, no ho faig a través de "Text" perquè he de dona diferents valors a 'base.Text' i 'vTextAnt'.
                base.Text = value.ToString(_Format);
                vTextAnt = value.ToString(CultureInfo.CurrentCulture);
                vValor = value;

                base.Text = value.ToString(_Format);
                vTextAnt = value.ToString(CultureInfo.CurrentCulture);
            }
        }

        public bool _PermetEspais { get; set; }

        public bool _PermetNegatius { get; set; }

        public bool _PermetDecimals { get; set; }


        /// <summary>
        /// Elimina tots els caràctes no numèrics excepte ".,-",
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string EliminaCaracterNoNumerics(string text)
        {
            return new String(text.Where(c => char.IsDigit(c) || c == DecimalSeparator || c == '-').ToArray());
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                Text = vTextAnt;

                e.SuppressKeyPress = true;
            }

            bool ctrlV = e.Modifiers == Keys.Control && e.KeyCode == Keys.V;
            bool shiftIns = e.Modifiers == Keys.Shift && e.KeyCode == Keys.Insert;

            vPaste = ctrlV || shiftIns;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (vPaste)
            {
                Paste();
                vPaste = false;
            }
        }


        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (vPaste)
            {
                Text = EliminaCaracterNoNumerics(base.Text);
                vPaste = false;
            }
        }

        
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == GroupSeparator)
                e.KeyChar = DecimalSeparator;

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (e.KeyChar == DecimalSeparator && Text.IndexOf(DecimalSeparator) == -1 && _PermetDecimals)
            {
                // Decimal separator is OK
            }
            else if (e.KeyChar.Equals(NegativeSign) && Text.IndexOf(NegativeSign) == -1 && _PermetNegatius)
            {
                // Negative sign is OK

                // Poso el signe al principi.
                int pos = SelectionStart + 1;
                Text = NegativeSign + Text;
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
                e.Handled = true;
            }
        }


        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            Text = base.Text;

            base.Text = Valor.ToString(_Format);
        }


        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            Text = vTextAnt ?? ""; // Text no pot ser null perque sinò no es dispara: OnLeave
            SelectAll();
        }


        public override string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                vTextAnt = value;

                var num = EliminaCaracterNoNumerics(value);
                vValor = String.IsNullOrEmpty(num) ? 0 : Double.Parse(num);
            }
        }

    }
}
