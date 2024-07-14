using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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

            // *** Per alguna raó, si no cambio primer el BackColor no es canvia el ForeColor si el control està readOnly o Disabled.
            //var xx = BackColor;
            //BackColor = Color.Blue;
            //BackColor = xx;
            //BackColor = BackColor;

        }

        public event EventHandler ValorChanged;
       
        private const int WM_PASTE = 0x0302;

        // Desa el format original de Text, abans d'aplicar el format.
        private string vTextAnt; // És el valor que té abans de cada pulsació mentre s'edita.
        private static readonly char DecimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        private static readonly char GroupSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
        private static readonly char NegativeSign = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NegativeSign);

        /// <summary>
        /// Utilitzo la variable per saber quan s'ha de seleccionar tot el text al fer clic. 
        /// Aixó és perquè encara que faci SelectAll en Enter si enfoco el control amb un clic se deselecciona el text.
        /// En el mètode Enter se li dona un valor al moure el mous s'anirà restant, mentre sigui > 0 si es fa clic se seleccionarà tot el text.
        /// </summary>
        private int vFerSelectAll;

        public string _Format { get; set; }


        public bool _NegatiusEnVermell { get; set; }

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



        private Color? vForeCol;

        public double ValorDouble
        {
            get { return _DoubleValue; }
            set { Valor = (decimal) value; }
        }

        public decimal Valor
        {
            get { return _DecimalValue; }
            set
            {
                Text = value.ToString(_Format);

                if (_NegatiusEnVermell)
                {
                    if (value < 0)
                    {
                        if (ForeColor != Color.Red)
                            // Si el valor anterior ja era negatiu, es desaria com ForeColor el vermell.
                            vForeCol = ForeColor;

                        ForeColor = Color.Red;
                    }
                    else if (vForeCol.HasValue)
                    {
                        ForeColor = vForeCol.Value;
                    }
                }
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
            if (text == null)
                return null;

            return new String(text.Where(c => char.IsDigit(c) || c == DecimalSeparator || c == '-').ToArray());
        }


        /// <summary>
        /// Gestiona el Paste.
        /// </summary>
        /// <returns></returns>
        private bool HandlePaste()
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();

                // Obtenir la posició inicial i la longitud de la selecció
                int selectionStart = this.SelectionStart;
                int selectionLength = this.SelectionLength;

                // Obtenir el text abans de la selecció
                string unselectedTextBefore = Text.Substring(0, selectionStart);

                // Obtenir el text després de la selecció
                string unselectedTextAfter = Text.Substring(selectionStart + selectionLength);

                // Combinar el text no seleccionat
                string textResultant = unselectedTextBefore + clipboardText + unselectedTextAfter;

                if (textResultant.Contains('-'))
                    // Si té el signe "-" el coloco al principi i si en te més d'un elimino la resta.
                    textResultant = "-" + textResultant.Replace("-", "");



                // *** Validacions textResultant.

                // Permetre números, una coma decimal i un signe
                const string pattern = @"^-?\d*[,]?\d*$";
                if (!Regex.IsMatch(textResultant, pattern))
                {
                    // Si entra és que hi ha lletres en el paste o més d'una coma o més d'un signe '-'.

                    MessageBox.Show("Format o caracters no permesos.");
                }
                else if (textResultant.Contains('-') && !_PermetNegatius)
                {
                    MessageBox.Show("No s'accepten negatius");
                }
                else
                {
                    Text = textResultant;
                    return true;
                }
            }
            return false;
        }


        #region *** Overrides ***

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_PASTE))
            {
                HandlePaste();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape && _CapturaEscape)
            {
                if (Modified)
                {
                    Undo();
                    Modified = false;
                }
            }

            if (e.KeyData == (Keys.Insert | Keys.Shift))
            {
                // Shift+Insert fa el paste automàticament i es dispara "OnTextChanged" abans de OnKeyUp".
                // Cancel·lo el Paste perquè el controlo des de "OnKeyUp".
                e.SuppressKeyPress = true;
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
            // Ctrl+V o Shift+Insert Paste.
            if (e.KeyData == (Keys.V | Keys.Control)
                || e.KeyData == (Keys.Insert | Keys.Shift))
            {
                e.Handled = HandlePaste();
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

            if (ValorChanged != null && !Equals(EliminaCaracterNoNumerics(Text), EliminaCaracterNoNumerics(vTextAnt)))
            {
                vTextAnt = Text;
                ValorChanged(this, e);
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            Text = Valor.ToString(_Format);
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (!ReadOnly)
            {
                var textSenseMoneda = EliminaCaracterNoNumerics(Text);

                vTextAnt = textSenseMoneda; // "vTextAnt" controla l'event "ValorChanged" aquí evito que es dispari només per entrar en el control.
                Text = textSenseMoneda;
            }

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

        #endregion *** Overrides ***
    }
}