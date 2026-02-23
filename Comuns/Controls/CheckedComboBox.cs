//
// CheckedComboBoxControl.cs
// Control personalitzat WinForms amb Combo + CheckedListBox
// Desenvolupat per Copilot (Microsoft) per a Joan
// Versió: 2026-02
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Controls
{
    //
    // --- Enums i EventArgs ---
    //

    public enum DropDownCloseReason
    {
        Normal,
        Enter,
        Escape
    }

    public class DropDownClosedEventArgs : EventArgs
    {
        public DropDownCloseReason Reason { get; }

        public DropDownClosedEventArgs(DropDownCloseReason reason)
        {
            Reason = reason;
        }
    }

    //
    // --- DropDown personalitzat per capturar ESC i ENTER ---
    //

    public class CustomDropDown : ToolStripDropDown
    {
        public event EventHandler EscapePressed;
        public event EventHandler EnterPressed;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                EscapePressed?.Invoke(this, EventArgs.Empty);
                return true;
            }

            if (keyData == Keys.Enter)
            {
                EnterPressed?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    //
    // --- CONTROL PRINCIPAL ---
    //

    public class CheckedComboBoxControl : UserControl
    {
        private readonly CheckedListBox _checkedListBox;
        private readonly CustomDropDown _dropDown;
        private readonly TextBox _displayBox;
        private readonly Button _button;

        private bool[] _initialCheckedState;
        private DropDownCloseReason _closeReason = DropDownCloseReason.Normal;

        public event EventHandler<ItemCheckEventArgs> ItemCheckChanged;
        public event EventHandler<DropDownClosedEventArgs> DropDownClosed;
        public event EventHandler DropDownOpened;

        public CheckedComboBoxControl()
        {
            this.Height = 24;

            //
            // Caixa de text (display)
            //
            _displayBox = new TextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_displayBox);

            //
            // Botó desplegable
            //
            _button = new Button
            {
                Dock = DockStyle.Right,
                Width = 25,
                Text = "▼"
            };
            _button.Click += (s, e) => ShowDropDown();
            this.Controls.Add(_button);

            //
            // CheckedListBox
            //
            _checkedListBox = new CheckedListBox
            {
                CheckOnClick = true,
                BorderStyle = BorderStyle.None
            };

            _checkedListBox.ItemCheck += (s, e) =>
            {
                BeginInvoke(new Action(UpdateText));
                ItemCheckChanged?.Invoke(this, e);
            };

            //
            // DropDown personalitzat
            //
            _dropDown = new CustomDropDown
            {
                Padding = Padding.Empty
            };

            var host = new ToolStripControlHost(_checkedListBox)
            {
                AutoSize = false
            };

            _dropDown.Items.Add(host);

            //
            // Captura d'ESC i ENTER
            //
            _dropDown.EscapePressed += (s, e) =>
            {
                // Restaurar estat original
                for (int i = 0; i < _checkedListBox.Items.Count; i++)
                    _checkedListBox.SetItemChecked(i, _initialCheckedState[i]);

                _closeReason = DropDownCloseReason.Escape;
                _dropDown.Close();
            };

            _dropDown.EnterPressed += (s, e) =>
            {
                _closeReason = DropDownCloseReason.Enter;
                _dropDown.Close();
            };

            //
            // Tancament del DropDown
            //
            _dropDown.Closed += (s, e) =>
            {
                DropDownClosed?.Invoke(this, new DropDownClosedEventArgs(_closeReason));
                _closeReason = DropDownCloseReason.Normal;
                UpdateText();
            };

            UpdateText();
        }

        //
        // --- Mostrar desplegable ---
        //

        private void ShowDropDown()
        {
            _initialCheckedState = new bool[_checkedListBox.Items.Count];
            for (int i = 0; i < _checkedListBox.Items.Count; i++)
                _initialCheckedState[i] = _checkedListBox.GetItemChecked(i);

            _checkedListBox.Width = this.Width;
            _checkedListBox.Height = 150;

            _dropDown.Show(this, 0, this.Height);

            DropDownOpened?.Invoke(this, EventArgs.Empty);
        }

        //
        // --- DisplayMember / ValueMember ---
        //

        private string _displayMember = null;
        private string _valueMember = null;

        [Category("Data")]
        public string DisplayMember
        {
            get => _displayMember;
            set
            {
                _displayMember = value;
                _checkedListBox.DisplayMember = value;
                UpdateText();
            }
        }

        [Category("Data")]
        public string ValueMember
        {
            get => _valueMember;
            set => _valueMember = value;
        }

        private string GetDisplayText(object item)
        {
            if (item == null) return "";

            if (DisplayMember == null)
                return item.ToString();

            var prop = item.GetType().GetProperty(DisplayMember);
            return prop?.GetValue(item)?.ToString() ?? "";
        }

        private object GetValue(object item)
        {
            if (item == null) return null;

            if (ValueMember == null)
                return item;

            var prop = item.GetType().GetProperty(ValueMember);
            return prop?.GetValue(item);
        }

        //
        // --- Aparença ---
        //

        private string _separator = ", ";
        private string _placeholder = "Selecciona...";
        private bool _showPlaceholderAlways = false;
        private bool _showCountInsteadOfList = false;
        private string _countFormat = "{0} seleccionats";

        [Category("Appearance")]
        public string Separator
        {
            get => _separator;
            set { _separator = value; UpdateText(); }
        }

        [Category("Appearance")]
        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; UpdateText(); }
        }

        [Category("Appearance")]
        public bool ShowPlaceholderAlways
        {
            get => _showPlaceholderAlways;
            set { _showPlaceholderAlways = value; UpdateText(); }
        }

        [Category("Appearance")]
        public bool ShowCountInsteadOfList
        {
            get => _showCountInsteadOfList;
            set { _showCountInsteadOfList = value; UpdateText(); }
        }

        [Category("Appearance")]
        public string CountFormat
        {
            get => _countFormat;
            set { _countFormat = value; UpdateText(); }
        }

        //
        // --- API pública ---
        //

        public int AddItem(object item, bool isChecked)
        {
            int index = _checkedListBox.Items.Add(item);
            _checkedListBox.SetItemChecked(index, isChecked);
            UpdateText();
            return index;
        }

        public bool IsCheckedByValue(object value)
        {
            int index = GetIndexByValue(value);
            return index != -1 && _checkedListBox.GetItemChecked(index);
        }

        public int GetIndexByValue(object value)
        {
            for (int i = 0; i < _checkedListBox.Items.Count; i++)
            {
                if (Equals(GetValue(_checkedListBox.Items[i]), value))
                    return i;
            }
            return -1;
        }

        public object GetItemByValue(object value)
        {
            int index = GetIndexByValue(value);
            return index == -1 ? null : _checkedListBox.Items[index];
        }

        //
        // --- Mètodes que m'has demanat ---
        //

        public bool GetItemChecked(int index)
        {
            if (index < 0 || index >= _checkedListBox.Items.Count)
                return false;

            return _checkedListBox.GetItemChecked(index);
        }

        public void SetItemChecked(int index, bool isChecked)
        {
            if (index < 0 || index >= _checkedListBox.Items.Count)
                return;

            _checkedListBox.SetItemChecked(index, isChecked);
            UpdateText();
        }

        /// <summary>
        /// Retorna tots els elements del control en forma de llista.
        /// </summary>
        public List<object> GetItems()
        {
            return _checkedListBox.Items.Cast<object>().ToList();
        }

        //
        // --- Actualitzar text ---
        //

        private void UpdateText()
        {
            var selected = _checkedListBox.CheckedItems.Cast<object>()
                .Select(GetDisplayText)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            bool noSelection = selected.Count == 0;

            if (ShowPlaceholderAlways)
            {
                _displayBox.ForeColor = Color.Gray;
                _displayBox.Text = Placeholder;
                return;
            }

            if (noSelection)
            {
                _displayBox.ForeColor = Color.Gray;
                _displayBox.Text = Placeholder;
                return;
            }

            if (ShowCountInsteadOfList)
            {
                _displayBox.ForeColor = Color.Black;
                _displayBox.Text = string.Format(CountFormat, selected.Count);
                return;
            }

            _displayBox.ForeColor = Color.Black;
            _displayBox.Text = string.Join(Separator, selected);
        }
    }
}