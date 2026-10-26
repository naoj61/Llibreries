using Comuns;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Controls
{
    public class DataGridView3 : DataGridView
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

        private DataGridViewCellStyle vOriginalCellStyle;
        private TextBox vTextBoxEnEdicio;
        private static readonly Dictionary<CellPosition, string> Formules = new(); // Guardem les fórmules per posició
        public static readonly string CaractersTotsElsPossibles = "\b0123456789,.-+*/";
        public static readonly string CaractersAcceptatsNomesUnCop = ",.";
        public static readonly string CaractersAcceptatsNomesAlInici = "-+"; // Només si no s'accepten formules. Si s'accepten fórmules, el signe pot anar a qualsevol lloc.
        public static readonly string CaractersAcceptatsNomesFormules = "*/";


        /// <summary>
        /// Comprova si s'accepta el caracter.
        /// </summary>
        /// <param name="car"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        private bool caracterValid(char car, int? columnIndex = null)
        {
            if (vTextBoxEnEdicio == null)
                return true;

            if (CaractersTotsElsPossibles.IndexOf(car) == -1)
                return false; // S'ignora el caràcter

            // Si el caràcter és un dels que només es poden posar una vegada,
            // comprovar si ja hi ha algun d'aquests caràcters al text actual
            // i si el text seleccionat no conté cap d'aquests caràcters.
            if (CaractersAcceptatsNomesUnCop.IndexOf(car) != -1
                && vTextBoxEnEdicio.Text.IndexOfAny(CaractersAcceptatsNomesUnCop.ToCharArray()) != -1
                && vTextBoxEnEdicio.SelectedText.IndexOfAny(CaractersAcceptatsNomesUnCop.ToCharArray()) == -1)
                return false; // S'ignora el caràcter


            // *** No valido la resta de cararcteristiques si no hi ha columna vàlida.

            if (!columnIndex.HasValue)
                return true;

            var col = (NumericTextBoxColumn2)Columns[columnIndex.Value];

            if (!col._AcceptaFormules)
            {
                if (CaractersAcceptatsNomesAlInici.IndexOf(car) != -1)
                {
                    // Si el caràcter és un dels que només es poden posar al principi, comprovar si la selecció comença al principi del text
                    if (vTextBoxEnEdicio.SelectionStart > 0)
                    {
                        return false; // S'ignora el caràcter
                    }
                    // Si el text ja té algun caràcter, comprovar si el primer caràcter és un dels que només es poden posar al principi
                    if (CaractersAcceptatsNomesAlInici.IndexOf(vTextBoxEnEdicio.Text[0]) != -1)
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


        #region ***** Controla la edició de cel·les numèriques *****

        private void numericTextBoxEnEdicio_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.' || e.KeyChar == ',')
            {
                // Converteix el punt o la coma en el separador decimal actual
                e.KeyChar = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
            }

            if (!caracterValid(e.KeyChar, CurrentCell.ColumnIndex))
            {
                e.Handled = true; // S'ignora el caràcter
            }
        }

        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            if (CurrentCell.OwningColumn is NumericTextBoxColumn2)
            {
                // Obté el control d'edició actual
                vTextBoxEnEdicio = e.Control as TextBox;

                if (vTextBoxEnEdicio != null)
                {
                    // Només apliquem la validació a les cel·les de columnes del tipus"NumericTextBoxColumn"
                    // Afegim un esdeveniment de validació de text numèric quan es produeix l'entrada
                    vTextBoxEnEdicio.KeyPress += numericTextBoxEnEdicio_KeyPress;

                    CellPosition cellPos = new(CurrentCell);
                    if (Formules.ContainsKey(cellPos))
                    {
                        vTextBoxEnEdicio.Text = Formules[cellPos];
                    }
                }
            }
            else
                vTextBoxEnEdicio = null;

            base.OnEditingControlShowing(e);
        }

        protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            vOriginalCellStyle = CurrentCell.Style.Clone();
            CurrentCell.Style.ForeColor = Color.Black;
            CurrentCell.Style.Format = "G29";

            base.OnCellBeginEdit(e);
        }

        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            var cell = this[e.ColumnIndex, e.RowIndex];

            if (cell.OwningColumn is NumericTextBoxColumn2 col)
            {
                // Si vTextBoxEnEdicio != null, cancel·lem l'esdeveniment de validació de text numèric quan es produeix l'entrada
                //col.vTextBoxEnEdicio?.KeyPress -= numericTextBoxEnEdicio_KeyPress;

                // Restauro l'estil de la cel·la a l'original abans de formatar-la
                cell.Style = vOriginalCellStyle;
            }

            base.OnCellEndEdit(e);
        }

        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var cell = this[e.ColumnIndex, e.RowIndex];

            if (cell.Value != null && cell.OwningColumn is NumericTextBoxColumn2 && cell.ErrorText == String.Empty)
            {
                var esNumeric = Decimal.TryParse(Convert.ToString(cell.Value), NumberStyles.Number
                    , NumberFormatInfo.InvariantInfo, out decimal retNum);

                if (esNumeric && retNum < 0 && ((NumericTextBoxColumn2)cell.OwningColumn)._NegatiusEnVermell)
                    cell.Style.ForeColor = Color.Red;
                else
                    cell.Style.ForeColor = Color.Black;
            }

            base.OnCellFormatting(e);
        }

        protected override void OnCellParsing(DataGridViewCellParsingEventArgs e)
        {
            DataGridViewCell cela = this[e.ColumnIndex, e.RowIndex];

            if (cela.OwningColumn is NumericTextBoxColumn2 column && column._AcceptaFormules)
            {
                // 2. Agafem el que ha escrit l'usuari (ex: "5 + 3")
                string textIntroduit = e.Value?.ToString();

                // 3. Comprovem si és una fórmula vàlida (ex: "5 + 3" -> 8)
                var celPos = new CellPosition(e.RowIndex, e.ColumnIndex);
                if (Utilitats.EsFormulaValida(textIntroduit, out decimal resultat))
                {
                    Formules[celPos] = textIntroduit; // Guardem la fórmula per a aquesta cel·la

                    // 6. LA MÀGIA: Substituïm el "5 + 3" pel "8"
                    e.Value = resultat;

                    // 7. Avisem al DataGridView que ja hem fet nosaltres la feina i que no cal que faci res més
                    e.ParsingApplied = true;
                }
                else
                {
                    // Si no és una fórmula vàlida, eliminem qualsevol fórmula guardada per a aquesta cel·la
                    Formules.Remove(celPos);
                }
            }

            base.OnCellParsing(e);
        }

        protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
        {
            // Si vols controlar l'error i evitar que es propagui, pots fer-ho assignant la propietat Handled a true
            e.ThrowException = false; // Opcional: indiques que ja has gestionat l'error
            e.Cancel = true; // Opcional: indiques que cancel·les l'operació que va provocar l'error

            DataGridViewCell cella = this[e.ColumnIndex, e.RowIndex];

            if (e.Exception is FormatException && cella.OwningColumn is NumericTextBoxColumn2)
            {
                // Aquí pots gestionar l'error de dades com vulguis
                MessageBox.Show("Error de dades: " + e.Exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                base.OnDataError(displayErrorDialogIfNoHandler, e);
            }
        }

        #endregion ***** Controla la edició de cel·les numèriques *****

    }


    #region ***** NumericTextBoxColumn2 *****

    /// <summary>
    /// DataGridViewTextBoxColumn per cel·les numériques.
    /// </summary>
    public class NumericTextBoxColumn2 : DataGridViewTextBoxColumn
    {
        public NumericTextBoxColumn2()
        {
            _NegatiusEnVermell = true;
            _AcceptaFormules = false;

            // Assegurar que només es permeten números en la cel·la
            this.CellTemplate = new NumericCell2();
        }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool _NegatiusEnVermell { get; set; }

        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool _AcceptaFormules { get; set; }

        public override object Clone()
        {
            var clone = (NumericTextBoxColumn2)base.Clone();

            clone._NegatiusEnVermell = this._NegatiusEnVermell;
            clone._AcceptaFormules = this._AcceptaFormules;

            return clone;
        }
    }


    /// <summary>
    /// DataGridViewTextBoxCell per cel·les numériques.
    /// </summary>
    public class NumericCell2 : DataGridViewTextBoxCell
    {
        public override Type EditType
        {
            get { return typeof(NumericEditingControl2); }
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

    /// <summary>
    /// DataGridViewTextBoxEditingControl per cel·les numériques.
    /// </summary>
    public class NumericEditingControl2 : DataGridViewTextBoxEditingControl
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
                    return true;
                default:
                    return false;
            }
        }
    }

    #endregion
}