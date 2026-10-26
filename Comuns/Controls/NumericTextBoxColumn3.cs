using Comuns;
using NCalc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Windows.Forms;

namespace Controls
{
    public class NumericTextBoxColumn3 : DataGridViewTextBoxColumn
    {
        public NumericTextBoxColumn3()
        {
            _NegatiusEnVermell = true;
            _AcceptaFormules = false;

            // Assegurar que només es permeten números en la cel·la
            this.CellTemplate = new NumericCell3();
        }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool _NegatiusEnVermell { get; set; }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool _AcceptaFormules { get; set; }

        public override object Clone()
        {
            var clone = (NumericTextBoxColumn3)base.Clone();

            clone._NegatiusEnVermell = this._NegatiusEnVermell;
            clone._AcceptaFormules = this._AcceptaFormules;

            return clone;
        }

        /// <summary>
        /// Aquest mètode s'executa automàticament quan la columna s'afegeix a un DataGridView
        /// </summary>Aquest mètode s'executa automàticament quan la columna s'afegeix a un DataGridView
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();

            if (this.DataGridView != null)
            {
                // Ens desconnectem primer per evitar duplicitats si la columna es mou
                this.DataGridView.CellValidating -= DataGridView_CellValidating;
                // Ens connectem al veritable esdeveniment que bloqueja la sortida de la cel·la
                this.DataGridView.CellValidating += DataGridView_CellValidating;
            }
        }

        private void DataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // 1. Ens assegurem que s'està validant AQUESTA columna en concret
            if (e.ColumnIndex == this.Index)
            {
                string textIntroduit = e.FormattedValue?.ToString();

                if (string.IsNullOrWhiteSpace(textIntroduit))
                {
                    return; // Buit és vàlid
                }

                bool esValid = false;

                if (decimal.TryParse(textIntroduit, NumberStyles.Any, CultureInfo.CurrentCulture, out _))
                {
                    // 2. Comprovem si és un número net
                    esValid = true;
                }
                else if (this._AcceptaFormules && Utilitats.EsFormulaValida(textIntroduit, out _))
                {
                    // 3. Comprovem si és una fórmula vàlida
                    esValid = true;
                }

                // 4. VEREDICTE
                if (!esValid)
                {
                    // Aquest e.Cancel SÍ que el respecta el DataGridView!
                    e.Cancel = true;
                    System.Media.SystemSounds.Beep.Play();

                    // (Opcional) Pintem de vermell el TextBox si encara està obert
                    if (this.DataGridView.EditingControl is TextBox txt)
                    {
                        txt.BackColor = Color.LightPink;
                    }
                }
            }
        }
    }


    /// <summary>
    /// DataGridViewTextBoxCell per cel·les numériques.
    /// </summary>
    public class NumericCell3 : DataGridViewTextBoxCell
    {
        private struct CellPosition
        {
            public CellPosition(int row, int column)
                : this()
            {
                Row = row;
                Column = column;
            }

            public CellPosition(DataGridViewCell cell)
                : this()
            {
                Row = cell.RowIndex;
                Column = cell.ColumnIndex;
            }

            public int Row { get; set; }
            public int Column { get; set; }


            // Sobreescrivim GetHashCode i Equals perquè funcioni correctament com a clau del diccionari
            public override bool Equals(object obj)
            {
                if (obj is CellPosition)
                {
                    var other = (CellPosition)obj;
                    return this.Row == other.Row && this.Column == other.Column;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Row.GetHashCode() ^ Column.GetHashCode();
            }
        }
        private static readonly Dictionary<CellPosition, string> Formules = []; // Guardem les fórmules per posició

        public override Type EditType
        {
            get { return typeof(NumericEditingControl3); }
        }

        public override Type ValueType
        {
            get { return typeof(decimal); }
        }

        public override object DefaultNewRowValue
        {
            get { return null; } // Valor per defecte per a noves files
        }


        #region **** Events ****

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if (Utilitats.EsNumeric(value))
            {
                // Fem la conversió segura. Si és un int, double, o decimal, funcionarà igual.
                decimal valorDecimal = Convert.ToDecimal(value);

                if (valorDecimal < 0 && ((NumericTextBoxColumn3)this.OwningColumn)._NegatiusEnVermell)
                {
                    cellStyle.ForeColor = Color.Red;
                }
                else
                {
                    cellStyle.ForeColor = Color.Black;
                }
            }

            return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
        }

        /// <summary>
        /// Analitza el valor formatat d'una cel·la i admet l'avaluació de fórmules i la conversió de valors segons l'estil de la cel·la 
        /// i els convertidors de tipus.
        /// </summary>
        /// <param name="formattedValue">The value to parse, typically user input.</param>
        /// <param name="cellStyle">The cell style to use for parsing.</param>
        /// <param name="formattedValueTypeConverter">The type converter for the formatted value.</param>
        /// <param name="valueTypeConverter">The type converter for the cell value.</param>
        /// <returns>The parsed value, a formula result, the original input, or DBNull.Value if parsing fails.</returns>
        public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            // 2. Agafem el que ha escrit l'usuari (ex: "5 + 3")
            string textIntroduit = formattedValue?.ToString();

            if (((NumericTextBoxColumn3)OwningColumn)._AcceptaFormules)
            {
                // 3. Comprovem si és una fórmula vàlida (ex: "5 + 3" -> 8)
                var celPos = new CellPosition(RowIndex, ColumnIndex);
                if (Utilitats.EsFormulaValida(textIntroduit, out decimal resultat))
                {
                    Formules[celPos] = textIntroduit; // Guardem la fórmula per a aquesta cel·la

                    // 7. Avisem al DataGridView que ja hem fet nosaltres la feina i que no cal que faci res més
                    return resultat;
                }
                else
                {
                    // Si no és una fórmula vàlida, eliminem qualsevol fórmula guardada per a aquesta cel·la
                    Formules.Remove(celPos);
                    return textIntroduit;
                }
            }

            try
            {
                return base.ParseFormattedValue(formattedValue, cellStyle, formattedValueTypeConverter, valueTypeConverter);
            }
            catch (Exception)
            {
                return DBNull.Value;
            }

        }

        override public void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

            if (this.DataGridView.EditingControl is TextBox vTextBoxEnEdicio)
            {
                vTextBoxEnEdicio.ForeColor = Color.Black;

                if (vTextBoxEnEdicio != null && Utilitats.EsNumeric(this.Value))
                {
                    CellPosition cellPos = new(this);
                    if (Formules.ContainsKey(cellPos))
                    {
                        vTextBoxEnEdicio.Text = Formules[cellPos];
                    }
                    else
                    {
                        vTextBoxEnEdicio.Text = Convert.ToDecimal(this.Value).ToString("G29");
                    }
                }
            }
        }

        #endregion **** Events ****
    }

    /// <summary>
    /// DataGridViewTextBoxEditingControl per cel·les numériques.
    /// </summary>
    public class NumericEditingControl3 : DataGridViewTextBoxEditingControl
    {
        public static readonly string CaractersTotsElsPossibles = "\b0123456789,.-+*/()";
        public static readonly string CaractersAcceptatsNomesUnCop = ",.";
        public static readonly string CaractersAcceptatsNomesAlInici = "-+"; // Només si no s'accepten formules. Si s'accepten fórmules, el signe pot anar a qualsevol lloc.
        public static readonly string CaractersAcceptatsNomesFormules = "*/()";

        /// <summary>
        /// Determina si el caràcter especificat és vàlid per a l'entrada, basant-se en regles sobre caràcters permesos, 
        /// únics i específics d'una posició.        
        /// </summary>
        /// <param name="car">The character to validate.</param>
        /// <returns>true if the character is valid for input; otherwise, false.</returns>
        private bool caracterValid(char car)
        {
            if (CaractersTotsElsPossibles.IndexOf(car) == -1)
                return false; // No és cap dels caràcters acceptats

            //// Si el caràcter és un dels que només es poden posar una vegada,
            //// comprovar si ja hi ha algun d'aquests caràcters al text actual
            //// i si el text seleccionat no conté cap d'aquests caràcters.
            if (CaractersAcceptatsNomesUnCop.IndexOf(car) != -1
                && Text.IndexOfAny(CaractersAcceptatsNomesUnCop.ToCharArray()) != -1
                && SelectedText.IndexOfAny(CaractersAcceptatsNomesUnCop.ToCharArray()) == -1)
                return false; // S'ignora el caràcter

            
            var col = (NumericTextBoxColumn3)EditingControlDataGridView.CurrentCell.OwningColumn;

            // Si la columna no accepta fórmules, aplicar regles addicionals per a caràcters específics
            if (col != null && !col._AcceptaFormules)
            {
                // Si és un caràcter que només es pot posar al principi, comprovar si el text ja té algun caràcter
                if (CaractersAcceptatsNomesAlInici.IndexOf(car) != -1)
                {
                    // Si el caràcter és un dels que només es poden posar al principi, comprovar si la selecció comença al principi del text
                    if (SelectionStart > 0)
                    {
                        return false; // S'ignora el caràcter
                    }

                    // Si el text ja té algun caràcter, comprovar si el primer caràcter és un dels que només es poden posar al principi
                    if (Text.Length > 0 && CaractersAcceptatsNomesAlInici.IndexOf(Text[0]) != -1)
                    {
                        return false; // S'ignora el caràcter
                    }
                }
                if (CaractersAcceptatsNomesFormules.IndexOf(car) != -1)
                {
                    return false; // S'ignora el caràcter
                }
            }

            return true;
        }

        //protected override bool IsInputKey(Keys keyData)
        //{
        //    // Permetre que les tecles de navegació i edició siguin acceptades
        //    switch (keyData & Keys.KeyCode)
        //    {
        //        case Keys.Left:
        //        case Keys.Right:
        //        case Keys.Up:
        //        case Keys.Down:
        //        case Keys.Home:
        //        case Keys.End:
        //        case Keys.Delete:
        //        case Keys.Back:
        //            return true;
        //        default:
        //            return false;
        //    }
        //}

        /// <summary>
        /// Handles key press events, converting period or comma to the current culture's decimal separator and
        /// suppressing invalid characters.
        /// </summary>
        /// <param name="e">Event data for the key press event.</param>
        override protected void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == '.' || e.KeyChar == ',')
            {
                // Converteix el punt o la coma en el separador decimal actual
                e.KeyChar = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
            }

            if (!caracterValid(e.KeyChar))
            {
                e.Handled = true; // S'ignora el caràcter
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            // 1. Cridem el base al principi perquè el TextBox processi la tecla 
            // introduïda o el text enganxat amb normalitat.
            base.OnTextChanged(e);

            // 2. Si el TextBox estava marcat amb error (rosa), el tornem 
            // al seu color blanc per defecte en el primer canvi.
            if (this.BackColor == Color.LightPink)
            {
                this.BackColor = SystemColors.Window;
            }
        }
    }
}