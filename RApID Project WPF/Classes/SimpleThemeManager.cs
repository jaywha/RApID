using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

    public class SimpleThemeManager : StyleSelector, INotifyPropertyChanged
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

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var st = new Style { TargetType = typeof(Panel) };
            var backGroundSetter = new Setter { Property = Panel.BackgroundProperty };
            var panel = (Panel) container;
            switch(ThemeAttachedProperty.GetThemeType(panel))
            {
                case Themes.Dark:
                    CurrentBackground = DarkThemeBackground;
                    break;
                case Themes.Light:
                    CurrentBackground = LightThemeBackground;
                    break;
                case Themes.Default:
                default:
                    CurrentBackground = DefaultThemeBackground;
                    break;
            }
            backGroundSetter.Value = CurrentBackground;
            st.Setters.Add(backGroundSetter);
            return st;
        }
    }
}
