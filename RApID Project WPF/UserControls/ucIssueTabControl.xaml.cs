using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace RApID_Project_WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucIssueTabControl.xaml
    /// </summary>
    public partial class ucIssueTabControl : UserControl, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ucIssueTabControl), new PropertyMetadata(false));
        public static readonly DependencyProperty IsRepairProperty = DependencyProperty.Register("IsRepair", typeof(bool), typeof(ucIssueTabControl), new PropertyMetadata(false));
        public static readonly DependencyProperty HeaderBrushProperty = DependencyProperty.Register("HeaderBrush", typeof(Brush), typeof(ucIssueTabControl), new PropertyMetadata(Brushes.DimGray));
        public static readonly DependencyProperty StaticVarsInstanceProperty = DependencyProperty.Register("StaticVarsInstance", typeof(StaticVars), typeof(ucUnitIssue));
        public static readonly DependencyProperty DropDownClosedHandlerProperty = DependencyProperty.Register("DropDownClosedHandler", typeof(EventHandler), typeof(ucIssueTabControl));
        #endregion

        #region Fields
        private static bool once = true;
        private ucUnitIssue PrevRemovedIssue = null;
        private ObservableCollection<ucUnitIssue> _issues = new ObservableCollection<ucUnitIssue>();
        #endregion

        #region Properties
        /// <summary>
        /// Deterimines if users can edit data.
        /// </summary>
        [Description("Deterimines if users can edit data."), Category("Common")]
        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set {
                SetValue(ReadOnlyProperty, value);                
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Determine what data to present.
        /// </summary>
        [Description("Determine what data to present."),Category("Common")]
        public bool IsRepair
        {
            get => (bool)GetValue(IsRepairProperty);
            set {
                SetValue(IsRepairProperty, value);
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Color of all the headers in the inner TabControl.
        /// </summary>
        [Description("Color of all the headers in the inner TabControl."),Category("Brush")]
        public Brush HeaderBrush
        {
            get => (Brush)GetValue(HeaderBrushProperty);
            set {
                SetValue(HeaderBrushProperty, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The parent instance of StaticVars for logging, if available.
        /// </summary>
        [Description("The parent instance of StaticVars for logging, if available."), Category("Automation")]
        public StaticVars StaticVarsInstance
        {
            get => (StaticVars)GetValue(StaticVarsInstanceProperty);
            set
            {
                SetValue(StaticVarsInstanceProperty, value);
                OnPropertyChanged();
            }
        }

        [Description("Propogates the DropDown control event to the next control responsible for it."), Category("Automation")]
        public EventHandler DropDownClosedHandler
        {
            get { return (EventHandler)GetValue(DropDownClosedHandlerProperty); }
            set { SetValue(DropDownClosedHandlerProperty, value); }
        }

        /// <summary>
        /// Main backing collection for this <see cref="TabControl"/> UserControl
        /// </summary>
        public ObservableCollection<ucUnitIssue> Issues
        {
            get => _issues;
            set {
                _issues = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Gets the <see cref="ucUnitIssue"/> at the given index.
        /// </summary>
        /// <param name="index">Index as shown in the <see cref="TabControl"/></param>
        /// <returns>An instance of a <see cref="ucUnitIssue"/></returns>
        public ucUnitIssue this[int index]
        {
            get
            {
                return Issues[index];
            }
            private set
            {
                Issues[index] = value;
            }
        }

        /// <summary>
        /// Basic constructor that initializes with a single <see cref="ucUnitIssue"/>.
        /// </summary>
        public ucIssueTabControl()
        {
            InitializeComponent();

            tcTabs.NewItemFactory =()=> new ucUnitIssue();

            try
            { if (once)
                {
                    Issues.CollectionChanged += Issues_CollectionChanged;
                    Issues.Add(new ucUnitIssue());
                    DataContext = this;
                    once = false;
                }
            } catch(Exception e)
            {
                #if DEBUG
                    Console.WriteLine($"UIC_InitError -> [{e.StackTrace}]");
                #else
                    csExceptionLogger.csExceptionLogger.Write("UnitIssueContainer_InitError", e);
                #endif
            }
        }

        private void Issues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ucUnitIssue item in e.NewItems)
                {
                    item.Width = tcTabs.Width;

                    ApplyBinding(item, ucUnitIssue.StaticVarsProperty, "StaticVarsInstance");
                    ApplyBinding(item, ucUnitIssue.ReadOnlyProperty, "ReadOnly");
                    ApplyBinding(item, ucUnitIssue.DropDownEventProperty, "DropDownClosedHandler");
                    ApplyBinding(item, ucUnitIssue.IsRepairFormProperty, "IsRepair");
                }
            }

            if (e.OldItems != null)
            {
                PrevRemovedIssue = e.OldItems[0] as ucUnitIssue;
            }
        }

        public ucUnitIssue Last() => Issues.Last();

        /// <summary> Applys a binding using the static <see cref="BindingOperations.SetBinding(DependencyObject, DependencyProperty, BindingBase)"/> for this class. </summary>
        /// <param name="obj">The affected object</param>
        /// <param name="dp">The dependency property to apply binding towards</param>
        /// <param name="pathValue">The property name of a property in this class</param>        
        protected internal BindingExpressionBase ApplyBinding(DependencyObject obj, DependencyProperty dp, string pathValue)
            => BindingOperations.SetBinding(obj, dp, new Binding()
            {
                Source = uccIssueTabControl,
                Path = new PropertyPath(pathValue),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
        
    }
}
