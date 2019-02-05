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
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged([CallerMemberName] string propName = "")
            => StaticPropertyChanged?.Invoke(typeof(SimpleThemeManager), new PropertyChangedEventArgs(propName));

        public static Brush DefaultThemeBackground { get; private set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0x46, 0x38, 0x38));
        public static Brush LightThemeBackground { get; private set; } = new SolidColorBrush(Color.FromArgb(0xFF,0xAC,0xAC,0xAC));
        public static Brush DarkThemeBackground { get; private set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0x30, 0x30, 0x30));

        private static Brush _currentBackground;
        public static Brush CurrentBackground {
            get => _currentBackground;
            set {
                _currentBackground = value;
                OnStaticPropertyChanged();
            }
        }

        private static Themes _currentThemeType;
        public static Themes CurrentThemeType
        {
            get => _currentThemeType;
            set {
                _currentThemeType = value;
                OnStaticPropertyChanged();
            }
        }

        public SimpleThemeManager() { }        
    }
}
