using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Comuns;

namespace Controls
{
    public class NumericTextBox2 : TextBox, IValorControlRestaurable
    {
        public NumericTextBox2()
        {
            _CapturaEscape = true;
            _Format = "0.#";
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
        private static readonly char DecimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        private static readonly char GroupSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator);
        private static readonly char NegativeSign = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NegativeSign);

        /// <summary>
        /// Utilitzo la variable per saber quan s'ha de seleccionar tot el text al fer clic. 
        /// Aixó és perquè encara que faci SelectAll en Enter si enfoco el control amb un clic se deselecciona el text.
        /// En el mètode Enter se li dona un valor al moure el mous s'anirà restant, mentre sigui > 0 si es fa clic se seleccionarà tot el text.
        /// </summary>
        private int vFerSelectAll;

        // És on deso el valor de Text en format decimal.
        private Color? vForeCol;
        private bool vEditant;


        #region *** Atributs ***

        public string _Format { get; set; }

        public bool _NegatiusEnVermell { get; set; }

        [Browsable(false)]
        public int _IntValue
        {
            get { return (int) Valor; }
        }

        [Browsable(false)]
        public decimal _DecimalValue
        {
            get { return Valor; }
        }

        [Browsable(false)]
        public double _DoubleValue
        {
            get { return (double)Valor; }
        }

        public decimal Valor
        {
            get
            {
                decimal valorDecimal;
                return decimal.TryParse(EliminaCaracterNoNumerics(Text), out valorDecimal) ? valorDecimal : 0;
            }            
            set
            {
                Text = value.ToString(_Format);
            }
        }

        public bool _PermetEspais { get; set; }

        public bool _PermetNegatius { get; set; }

        public bool _PermetDecimals { get; set; }

        [Description("Si true, restaura valor inicial al premer ESC.")]
        public bool _CapturaEscape { get; set; }

        #endregion *** Atributs ***

        /// <summary>
        /// Elimina tots els caràctes no numèrics excepte ".,-" i posa el signe menys al principi si en té.
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
        /// Canvia el color del valor.
        /// </summary>
        private void colorNumero()
        {
            if (_NegatiusEnVermell && Valor < 0)
            {
                if (ForeColor != Color.Red)
                {
                    // Deso si ForeColor actual no és Vermell.
                    vForeCol = ForeColor;

                    ForeColor = Color.Red;
                }
            }
            else
            {
                ForeColor = vForeCol.GetValueOrDefault(Color.Black);
            }
        }


        /// <summary>
        /// Gestiona el Paste, fa les validacions.
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
                    textResultant = NegativeSign + textResultant.Replace(NegativeSign.ToString(), string.Empty);


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
            else if (e.KeyChar.Equals(NegativeSign))
            {
                if (!_PermetNegatius || Valor == 0 || Text.Contains(NegativeSign))
                {
                    // Si no permet negatius o valor = 0 o ja conté signe, salta la pulsació
                    e.Handled = true; // No escriu el signe
                    // Negative sign is KO
                }
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
                e.Handled = true; // El caracter no s'escriurà.
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // Posa el signe al principi.
            if (Text.Length > 0 && Text[0] != NegativeSign && Text.Contains(NegativeSign))
                Text = NegativeSign + Text.Replace(NegativeSign.ToString(), String.Empty);
            
            // Canvia el color si fa falta
            if (!vEditant)
                colorNumero();

            // Dispara l'event si fa falta
            if (ValorChanged != null)
            {
                ValorChanged(this, e);
            }
        }

        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);

            if (!ReadOnly)
            {
                vEditant = true;

                // Text sense Moneda.
                Text = EliminaCaracterNoNumerics(Text);

                ForeColor = vForeCol.GetValueOrDefault(Color.Black);
            }

            vFerSelectAll = 5;
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            vEditant = false;

            Text = Valor.ToString(_Format);

            colorNumero();
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