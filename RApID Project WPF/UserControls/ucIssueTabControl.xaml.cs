﻿using System;
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
                }
            }

            if (e.OldItems != null)
            {
                PrevRemovedIssue = e.OldItems[0] as ucUnitIssue;
            }
        }

        public ucUnitIssue Last() => Issues.Last();
    }
}
