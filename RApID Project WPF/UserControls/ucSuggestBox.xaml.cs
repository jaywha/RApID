using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace RApID_Project_WPF.UserControls
{
    public enum SuggestModes
    {
        None,
        Append,
        Suggest,
        SuggestAppend
    }

    /// <summary>
    /// Interaction logic for SuggestBox.xaml
    /// Some code from https://www.c-sharpcorner.com/UploadFile/201fc1/autocomplete-textbox-in-wpf-using-only-net-and-wpf-librari/
    /// </summary>
    public partial class ucSuggestBox : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty _visibility = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(ucSuggestBox), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ucSuggestBox), new PropertyMetadata(""));
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ucSuggestBox), new PropertyMetadata(false));
        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(string), typeof(ucSuggestBox), new PropertyMetadata("Suggest Label:"));
        public static readonly DependencyProperty AutoCompleteSourceProperty = DependencyProperty.Register("AutoCompleteSource", typeof(List<String>), typeof(ucSuggestBox));
        public static readonly DependencyProperty AutoCompleteModeProperty = DependencyProperty.Register("AutoCompleteMode", typeof(SuggestModes), typeof(ucSuggestBox), new PropertyMetadata(SuggestModes.None));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(Window), typeof(ucSuggestBox));
        #endregion

        #region Properties
        [Description("The inner TextBox text"), Category("Common")]
        public string Text {
            get => (string) GetValue(TextProperty);
            set {
                SetValue(TextProperty, value);
            }
        }
        
        [Description("Will mark the TextBox as ReadOnly"), Category("Visibility")]
        public bool IsReadOnly {
            get => (bool)GetValue(IsReadOnlyProperty);
            
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        [Description("The inner Label text"), Category("Common")]
        public string LabelText
        {
            get => (string)GetValue(LabelTextProperty);
            set
            {
                SetValue(LabelTextProperty, value);
            }
        }

        [Description("The list of viable values that will be available in the box."), Category("Common")]
        public List<String> AutoCompleteSource {
            get => (List<String>)GetValue(AutoCompleteSourceProperty);
            set => SetValue(AutoCompleteSourceProperty, value);
        }

        [Description("Default None. Append will add to current input while Suggest will show the list."), Category("Common")]
        public SuggestModes AutoCompleteMode
        {
            get => (SuggestModes)GetValue(AutoCompleteModeProperty);
            set => SetValue(AutoCompleteModeProperty, value);
        }

        [Description("Sets the owner of the user control."), Category("Appearance")]
        public Window Owner
        {
            get => (Window)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }
        #endregion

        public static Window DialogHost;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="values">List of values for this Suggestion Box</param>
        public ucSuggestBox()
        {
            InitializeComponent();
            SuggestionBox.KeyUp += SuggestionBox_OnKeyUp;
            AutoCompleteSource = new List<string>(); // no nulls...
            brdSuggestionsBorder.Visibility = Visibility.Collapsed;
        }

        private string currentInput = "";
        private string currentSuggestion = ".";
        private string currentText = "";

        private int selectionStart;
        private int selectionLength;

        /// <summary>
        /// Will handle populating suggestions as needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuggestionBox_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool found = false;
            var border = (stkSuggestions.Parent as ScrollViewer).Parent as Border;

            string query = (sender as TextBox).Text;

            if (query.Length == 0)
            {
                // Clear   
                stkSuggestions.Children.Clear();
                border.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                border.Visibility = System.Windows.Visibility.Visible;
            }

            // Clear the list   
            stkSuggestions.Children.Clear();

            // Add the result   
            foreach (var obj in AutoCompleteSource)
            {
                if (obj.ToLower().StartsWith(query.ToLower()))
                {
                    // The word starts with this... Autocomplete must work   
                    addItem(obj);
                    found = true;
                }
            }

            if (!found)
            {
                stkSuggestions.Children.Add(new TextBlock() { Text = "No results found." });
            }
        }

        /// <summary>
        /// Delegate for handling adding items to the suggestion box.
        /// </summary>
        /// <param name="text"></param>
        private void addItem(string text)
        {
            var block = new TextBlock
            {
                // Add the text   
                Text = text,

                // A little style...   
                Margin = new Thickness(2, 3, 2, 3),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Mouse events   
            block.MouseLeftButtonUp += (sender, e) =>
            {
                SuggestionBox.Text = (sender as TextBlock).Text;
                brdSuggestionsBorder.Visibility = Visibility.Collapsed;
            };

            block.MouseEnter += (sender, e) =>
            {
                var b = sender as TextBlock;
                b.Background = Brushes.PeachPuff;
            };

            block.MouseLeave += (sender, e) =>
            {
                var b = sender as TextBlock;
                b.Background = Brushes.Transparent;
            };

            // Add to the panel   
            stkSuggestions.Children.Add(block);
        }

        /// <summary>
        /// Will append viable characters to the end of the input.
        /// </summary>
        /// <param name="sender">The TextBox who's text changed.</param>
        /// <param name="e">Any text changed event arguments</param>
        [Obsolete("Needs review to determine to delete.")]
        private void SuggestionBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var input = SuggestionBox.Text;
            if (input.Length > currentSuggestion.Length && input != currentSuggestion)
            {
                currentSuggestion = AutoCompleteSource.FirstOrDefault(val => val.StartsWith(input));
                if(!string.IsNullOrEmpty(currentSuggestion))
                {
                    currentText = currentSuggestion;
                    selectionStart = input.Length;
                    selectionLength = currentSuggestion.Length - input.Length;
                }
            }

            currentInput = input;
        }

        /// <summary>
        /// Selects all of the content of the inner textbox
        /// </summary>
        public void SelectAll() => SuggestionBox.SelectAll();

        /// <summary>
        /// Will implicitly convert this user control to expose it's <see cref="TextBox"/>
        /// </summary>
        /// <param name="sb">The SuggestBox to convert</param>
        public static implicit operator TextBox(ucSuggestBox sb) => sb.SuggestionBox;
    }
}
