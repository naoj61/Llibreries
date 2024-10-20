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
            _NegatiusEnVermell = true;
            _PermetTextNull = false;

            // *** Per alguna raó, si no cambio primer el BackColor no es canvia el ForeColor si el control està readOnly o Disabled.
            var xx = BackColor;
            BackColor = Color.Blue;
            BackColor = xx;
            //BackColor = BackColor;
        }


        #region *** Variables ***

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
        private decimal vValor;
        
        private Color vForeColor;
        private bool vTextModificat;
        private bool vInhabilitaOnTextChanged; // L'activo per evitar l'execució de "OnTextChanged"
        private Color vBackColorOrig;

        #endregion *** Variables ***


        #region *** Atributs ***

        [Browsable(false)]
        public int _IntValue
        {
            get { return (int) vValor; }
        }

        [Browsable(false)]
        public decimal _DecimalValue
        {
            get { return vValor; }
        }

        [Browsable(false)]
        public double _DoubleValue
        {
            get { return (double)vValor; }
        }

        [Browsable(true)]
        public decimal Valor
        {
            get { return vValor; }
            set
            {
                vValor = value;
                
                Text = value.ToString(_Format);

                if (_NegatiusEnVermell && vValor < 0)
                    base.ForeColor = Color.Red;
                else
                    base.ForeColor = vForeColor;
            }
        }

        [Browsable(true)]
        public string _Format { get; set; }

        [Browsable(true)]
        public bool _NegatiusEnVermell { get; set; }

        [Browsable(true)]
        public bool _PermetNegatius { get; set; }

        [Browsable(true)]
        public bool _PermetDecimals { get; set; }

        [Browsable(true)]
        public bool _PermetTextNull { get; set; }

        [Description("Si true, restaura valor inicial al premer ESC.")]
        [Browsable(true)]
        public bool _CapturaEscape { get; set; }

        #endregion *** Atributs ***


        #region *** Mètodes ***

        /// <summary>
        /// Retorna el text que no està seleccionat i inserta el text del paràmetre en la posició del cursor.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string textNoSeleccionat(string text)
        {
            // Obtenir la posició inicial i la longitud de la selecció
            int selectionStart = this.SelectionStart;
            int selectionLength = this.SelectionLength;

            // Obtenir el text abans de la selecció
            string unselectedTextBefore = Text.Substring(0, selectionStart);

            // Obtenir el text després de la selecció
            string unselectedTextAfter = Text.Substring(selectionStart + selectionLength);

            // Combinar el text no seleccionat
            return unselectedTextBefore + text + unselectedTextAfter;
        }

        /// <summary>
        /// Gestiona el Paste, fa les validacions.
        /// </summary>
        /// <returns></returns>
        private bool HandlePaste()
        {
            if (Clipboard.ContainsText())
            {
                string textClipboard = Clipboard.GetText().Replace(".", ""); // Elimino el punt de milers.
                
                textClipboard = Utilitats.RemoveCurrencySymbols(textClipboard); // Elimino el simbol de moneda.

                // Comprovo que el Clipboard conté unvalor numèric.
                decimal valorDecimal;
                if (!decimal.TryParse(textClipboard, out valorDecimal))
                {
                    MessageBox.Show("Format o caracters no permesos.");
                    return false;
                }

                // Valida si Clipboard conté signe.
                if (textClipboard.Contains(NegativeSign))
                {
                    if (!_PermetNegatius)
                    {
                        MessageBox.Show("No s'accepten negatius");
                        return false;
                    }

                    if (Text.Contains(NegativeSign) && SelectionStart > 1 || !Text.Contains(NegativeSign) && SelectionStart > 0)
                    {
                        // No permet Paste si te '-', però el cursor no està al principi.
                        MessageBox.Show("Format o caracters no permesos.");
                        return false;
                    }
                }

                string textFinal = textNoSeleccionat(textClipboard);

                if (textFinal.Count(c => c == DecimalSeparator) > 1)
                {
                    // Hi ha més d'una coma.
                    MessageBox.Show("Format o caracters no permesos.");
                    return false;
                }

                if (textFinal.Count(c => c == NegativeSign) > 1)
                    // Hi ha més d'un signe.
                    base.Text = NegativeSign + textFinal.Replace("-", "");
                else
                    base.Text = textFinal;

                return true;
            }
            return false;
        }

        #endregion *** Mètodes ***


        #region *** Overrides ***

        /// <summary>
        /// Sobreescric per cridar el mètode 'HandlePaste'
        /// </summary>
        /// <param name="m"></param>
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

        /// <summary>
        /// Sobreescric per evitar que es processi 'OnTextChanged' quan es modifica Text.
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                vInhabilitaOnTextChanged = true;
                base.Text = value;
                vInhabilitaOnTextChanged = false;
            }
        }

        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                vBackColorOrig = value;
                base.BackColor = value;
            }
        }

        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set
            {
                vForeColor = value;
                base.ForeColor = value;
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.KeyCode == Keys.Enter)
            {
                // S'executa abans que AcceptButton s'activi automàticament
                // Evitem que AcceptButton s'activi automàticament
                //e.IsInputKey = true;
                vValor = Utilitats.TextADecimal(Text);
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

            var textNoSeleccionat = this.textNoSeleccionat(null);

            if (Char.IsDigit(e.KeyChar))
            {
                // Digits are OK
            }
            else if (e.KeyChar == DecimalSeparator && _PermetDecimals && textNoSeleccionat.IndexOf(DecimalSeparator) == -1)
            {
                // Decimal separator is OK
            }
            else if (e.KeyChar.Equals(NegativeSign))
            {
                if (!_PermetNegatius || textNoSeleccionat.Contains(NegativeSign))
                {
                    // Si no permet negatius o ja conté signe, salta la pulsació
                    e.Handled = true; // No escriu el signe
                    // Negative sign is KO
                }
            }
            else if (e.KeyChar == '\b')
            {
                // Backspace key is OK
            }
            else
            {
                e.Handled = true; // El caracter no s'escriurà.
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (vInhabilitaOnTextChanged)
                return;

            // Posa el signe al principi.
            if (Text.Length > 0 && Text[0] != NegativeSign && Text.Contains(NegativeSign))
            {
                Text = NegativeSign + Text.Replace(NegativeSign.ToString(), String.Empty);
            }

            vTextModificat = true;

            base.OnTextChanged(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            vTextModificat = false;

            if (ReadOnly)
            {
                // Al entrar no selecciona el contingut.
                this.SelectionStart = 0;
            }
            else
            {
                // * No vull que surtin ceros a la dreta de la coma.
                Text = vValor.ToString("0.###############");

                base.ForeColor = Color.Black;
            }

            vFerSelectAll = 5;

            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            if (vTextModificat)
                vValor = Utilitats.TextADecimal(Text);

            if (!_PermetTextNull && String.IsNullOrEmpty(Text))
                Text = "0";

            if (vValor == 0 && !Regex.IsMatch(Text, @"\d"))
                //*  Si vValor = 0 i Text no te cap digit numèric, deixo Text buit. Ni 0 ni simbol de moneda.
                Text = String.Empty;
            else
                Text = vValor.ToString(_Format);

            if (_NegatiusEnVermell && vValor < 0)
                base.ForeColor = Color.Red;
            else
                base.ForeColor = vForeColor;
        }

        protected override void OnReadOnlyChanged(EventArgs e)
        {
            base.OnReadOnlyChanged(e);

            if (!this.DesignMode) // Comprovació per evitar canvis en el dissenyador
            {
                if (ReadOnly && BackColor == SystemColors.Window)
                {
                    vBackColorOrig = BackColor;
                    base.BackColor = Color.Gainsboro;
                }
                else
                {
                    base.BackColor = vBackColorOrig;
                }
            }
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