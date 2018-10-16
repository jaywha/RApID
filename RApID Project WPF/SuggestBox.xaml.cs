using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace RApID_Project_WPF
{
    /// <summary>
    /// Interaction logic for SuggestBox.xaml
    /// </summary>
    public partial class SuggestBox : UserControl
    {
        public List<string> SuggestionValues { get; set; }

        public static readonly DependencyProperty _visibility = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(SuggestBox), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty _text = DependencyProperty.Register("Text", typeof(string), typeof(SuggestBox), new PropertyMetadata(""));
        public static readonly DependencyProperty _isReadOnly = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SuggestBox), new PropertyMetadata(false));

        public string Text {
            get => (string) GetValue(_text);
            set {
                SetValue(_text, value);
            }
        }
        
        public bool IsReadOnly {
            get => (bool)GetValue(_isReadOnly);
            
            set
            {
                SetValue(_isReadOnly, value);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="values">List of values for this Suggestion Box</param>
        public SuggestBox()
        {
            InitializeComponent();
            SuggestionBox.TextChanged += SuggestionBox_OnTextChanged;
        }

        private string currentInput = "";
        private string currentSuggestion = "";
        private string currentText = "";

        private int selectionStart;
        private int selectionLength;

        /*public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));*/

        private void SuggestionBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var input = SuggestionBox.Text;
            if(input.Length > currentInput.Length && input != currentSuggestion)
            {
                currentSuggestion = SuggestionValues.FirstOrDefault(val => val.StartsWith(input));
                if(currentSuggestion != null)
                {
                    currentText = currentSuggestion;
                    selectionStart = input.Length;
                    selectionLength = currentSuggestion.Length - input.Length;
                }
            }

            currentInput = input;
        }

        public void SelectAll() => SuggestionBox.SelectAll();

        public static implicit operator TextBox(SuggestBox sb) => sb.SuggestionBox;
    }
}
