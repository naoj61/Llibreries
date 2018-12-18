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
        private bool vEsTab = false; // Indica al control que rep el focus, si s'ha fet Tab.
        private bool vSelectAll = false; // Indica al event "OnMouseClick" si s'ha de seleccionar el text.


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


        /// <summary>
        /// Troba el cobtrol següent o anterior.
        /// </summary>
        /// <param name="seguent">False: Control anterior. True: Control següent.</param>
        /// <returns></returns>
        private Control trobaElSeguentControl(bool seguent)
        {
            var form = FindForm();

            if (form == null)
                return null;

            var controlSeguent = form.GetNextControl(this, seguent);

            while (controlSeguent != null && !controlSeguent.TabStop)
            {
                controlSeguent = form.GetNextControl(controlSeguent, seguent);
            }
            return controlSeguent;
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

            if (!Equals(Text, vTextAnt) && ValorChanged != null)
                ValorChanged(this, e);
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

        
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.KeyData == Keys.Tab || e.KeyData == (Keys.Tab | Keys.Shift))
            {
                Control tabSeguent = trobaElSeguentControl(e.KeyData == Keys.Tab);

                if (tabSeguent is NumericTextBox2)
                    // Indico al NumericTextBox2 que rebrà el focus, que sha fet Tab.
                    ((NumericTextBox2)tabSeguent).vEsTab = true;
            }
        }


        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (!ReadOnly)
                Text = vTextAnt ?? ""; // Text no pot ser null perque sinò no es dispara: OnLeave
            
            vSelectAll = !vEsTab;

            if (vEsTab)
            {
                // S'ha fet Tab en el NumericTextBox2 anterior.
                SelectAll();
                vEsTab = false;
            }
        }


        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (vSelectAll)
            {
                SelectAll();
                vSelectAll = false;
            }
        }
    }
}