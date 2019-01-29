using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RApID_Project_WPF.Classes
{
    public enum Themes
    {
        /// <summary> Prefers muted tones and shadier UI suited for evening use.</summary>
        Dark = -1,
        /// <summary> Default color scheme and font styles inherent to HB programs.</summary>
        Default,
        /// <summary> Prefers vibrant tones and smoother UI suited for early in the day.</summary>
        Light    
    }

    public class SimpleThemeManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Brush DefaultThemeBackground { get; private set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0x46, 0x38, 0x38));
        public Brush LightThemeBackground { get; private set; } = new SolidColorBrush(Color.FromArgb(0xFF,0xAC,0xAC,0xAC));
        public Brush DarkThemeBackground { get; private set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x30, 0x30));

        private Brush _currentBackground;
        public Brush CurrentBackground {
            get => _currentBackground;
            set {
                _currentBackground = value;
                OnPropertyChanged();
            }
        }

        private Themes _currentThemeType;
        public Themes CurrentThemeType
        {
            get => _currentThemeType;
            set {
                _currentThemeType = value;
                OnPropertyChanged();
            }
        }

        public SimpleThemeManager() { }        
    }
}
