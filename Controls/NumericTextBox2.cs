using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Controls
{
    public class NumericTextBox2 : TextBox, IValorControlRestaurable
    {
        public NumericTextBox2()
        {
            _CapturaEscape = true;
            _Format = "#.#";
            TextAlign = HorizontalAlignment.Right;
            _PermetNegatius = true;
            _PermetDecimals = true;
            _PermetEspais = false;
        }

        public event EventHandler ValorChanged;

        // Desa el format original de Text, abans d'aplicar el format.
        private string vTextAnt = null;
        private static readonly char DecimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        private static readonly char GroupSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
        private static readonly char NegativeSign = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NegativeSign);

        private bool vPaste;
        
        /// <summary>
        /// Utilitzo la variable per saber quan s'ha de seleccionar tot el text al fer clic. 
        /// Aixó és perquè encara que faci SelectAll en Enter si enfoco el control amb un clic se deselecciona el text.
        /// En el mètode Enter se li dona un valor al moure el mous s'anirà restant, mentre sigui > 0 si es fa clic se seleccionarà tot el text.
        /// </summary>
        private int vFerSelectAll;


        public string _Format { get; set; }

        [Browsable(false)]
        public int _IntValue
        {
            get
            {
                string num = EliminaCaracterNoNumerics(base.Text);
                return String.IsNullOrEmpty(num) ? 0 : Int32.Parse(num);
            }
        }

        [Browsable(false)]
        public decimal _DecimalValue
        {
            get
            {
                string num = EliminaCaracterNoNumerics(base.Text);
                return String.IsNullOrEmpty(num) ? 0 : Decimal.Parse(num);
            }
        }

        [Browsable(false)]
        public double _DoubleValue
        {
            get
            {
                string num = EliminaCaracterNoNumerics(base.Text);
                return String.IsNullOrEmpty(num) ? 0 : Double.Parse(num);
            }
        }

        public double Valor
        {
            get { return _DoubleValue; }
            set
            {
                // Deso el valor en base.Text, no ho faig a través de "Text" perquè he de dona diferents valors a 'base.Text' i 'vTextAnt'.
                base.Text = value.ToString(_Format);
                vTextAnt = value.ToString(CultureInfo.CurrentCulture);
            }
        }

        public bool _PermetEspais { get; set; }

        public bool _PermetNegatius { get; set; }

        public bool _PermetDecimals { get; set; }

        [Description("Si true, restaura valor inicial al premer ESC.")]
        public bool _CapturaEscape { get; set; }


        /// <summary>
        /// Elimina tots els caràctes no numèrics excepte ".,-",
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string EliminaCaracterNoNumerics(string text)
        {
            return new String(text.Where(c => char.IsDigit(c) || c == DecimalSeparator || c == '-').ToArray());
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


        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape && _CapturaEscape)
            {
                if (Modified)
                {
                    Undo(); //Text = vTextAnt;
                    Modified = false;
                }
                //e.SuppressKeyPress = true;
            }

            if (e.KeyData == (Keys.Insert | Keys.Shift))
            {
                // Ha d'anar aquí perquè Shift+Insert fa el paste automàticament i es dispara "OnTextChanged" abans de OnKeyUp".
                vPaste = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            // Ctrl+X Cut. (Shift+Supr ja ho fa sol)
            if (e.KeyData == (Keys.X | Keys.Control))
            {
                Cut();
            }
            // Ctrl+C Copy. (Ctrl+Insert ja ho fa sol)
            if (e.KeyData == (Keys.C | Keys.Control))
            {
                Copy();
            }
            // Ctrl+V Paste. (Shift+Insert ja ho fa sol)
            if (e.KeyData == (Keys.V | Keys.Control))
            {
                vPaste = true;
                Paste();
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
            else if (e.KeyChar == DecimalSeparator && _PermetDecimals && Text.IndexOf(DecimalSeparator) == -1)
            {
                // Decimal separator is OK
            }
            else if (e.KeyChar.Equals(NegativeSign) && _PermetNegatius && Text.IndexOf(NegativeSign) == -1)
            {
                // Negative sign is OK

                // Poso el signe al principi.
                int pos = SelectionStart + 1;
                Text = NegativeSign + Text;
                Select(pos, 0);
                e.Handled = true;
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


        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (vPaste)
            {
                Text = EliminaCaracterNoNumerics(base.Text);
                vPaste = false;
            }

            if (!Equals(Text, vTextAnt) && ValorChanged != null)
                ValorChanged(this, e);
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

            if (!ReadOnly)
                Text = vTextAnt ?? ""; // Text no pot ser null perque sinò no es dispara: OnLeave
            
            //vFerSelectAll = ReadOnly ? 5 : 0;
            vFerSelectAll = 5;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (vFerSelectAll > 0)
            {
                SelectAll();
                vFerSelectAll = 0;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
           
            vFerSelectAll--;
        }
    }
}