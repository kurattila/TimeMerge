using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TimeMerge.Controls
{
    /// <summary>
    /// Interaction logic for InterruptionCellControl.xaml
    /// </summary>
    public partial class InterruptionCellControl : UserControl, INotifyPropertyChanged
    {
        public InterruptionCellControl()
        {
            InitializeComponent();

            // this.LayoutRoot.DataContext = this;

            this.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(InterruptionCellControl_GotKeyboardFocus);
        }

        private TextBox m_EditText;
        void InterruptionCellControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this.m_EditText);
        }




        public bool IsInEditMode
        {
            get { return (bool)GetValue(IsInEditModeProperty); }
            set { SetValue(IsInEditModeProperty, value); }
        }
        public static readonly DependencyProperty IsInEditModeProperty =
            DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(InterruptionCellControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(onIsInEditModeChanged)));


        private static void onIsInEditModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            InterruptionCellControl thisInstance = sender as InterruptionCellControl;
            if (thisInstance.m_EditText == null)
            {
                thisInstance.m_EditText = new TextBox();
                thisInstance.LayoutRoot.Children.Add(thisInstance.m_EditText);
                thisInstance.m_EditText.Style = TimeMerge.App.Current.Resources["gridCellTextBoxStyle"] as Style;
                thisInstance.m_EditText.Text = thisInstance.TextContent;
                thisInstance.m_EditText.TextChanged += thisInstance.editText_TextChanged;
                thisInstance.m_EditText.GotKeyboardFocus += thisInstance.editText_GotKeyboardFocus;
            }

            if (thisInstance != null && thisInstance.m_EditText != null)
            {
                thisInstance.m_EditText.Visibility = (bool)e.NewValue == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        

        public bool IsNoWorkDay
        {
            get { return (bool)GetValue(IsNoWorkDayProperty); }
            set { SetValue(IsNoWorkDayProperty, value); }
        }
        public static readonly DependencyProperty IsNoWorkDayProperty =
            DependencyProperty.Register("IsNoWorkDay", typeof(bool), typeof(InterruptionCellControl));

        

        public bool HasCorrections
        {
            get { return (bool)GetValue(HasCorrectionsProperty); }
            set { SetValue(HasCorrectionsProperty, value); }
        }
        public static readonly DependencyProperty HasCorrectionsProperty =
            DependencyProperty.Register("HasCorrections", typeof(bool), typeof(InterruptionCellControl));


        public TimeMerge.Model.WorkInterruption.WorkInterruptionType InterruptType
        {
            get { return (TimeMerge.Model.WorkInterruption.WorkInterruptionType)GetValue(InterruptTypeProperty); }
            set { SetValue(InterruptTypeProperty, value); }
        }
        public static readonly DependencyProperty InterruptTypeProperty =
            DependencyProperty.Register("InterruptType", typeof(TimeMerge.Model.WorkInterruption.WorkInterruptionType), typeof(InterruptionCellControl),
                                        new UIPropertyMetadata(TimeMerge.Model.WorkInterruption.WorkInterruptionType.OTHER));




        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }
        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(InterruptionCellControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(onTextContentPropertyChanged)));

        private static void onTextContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            InterruptionCellControl thisControl = sender as InterruptionCellControl;
            if (e.NewValue != null && thisControl.m_EditText != null)
            {
                thisControl.m_EditText.Text = (string)e.NewValue;
            }
            thisControl.NotifyPropertyChanged("DisplayText");
        }




        public bool IsEndOfInterval
        {
            get { return (bool)GetValue(IsEndOfIntervalProperty); }
            set { SetValue(IsEndOfIntervalProperty, value); }
        }
        public static readonly DependencyProperty IsEndOfIntervalProperty =
            DependencyProperty.Register("IsEndOfInterval", typeof(bool), typeof(InterruptionCellControl));





        public bool IsInterruptWorkSpan
        {
            get { return (bool)GetValue(IsInterruptWorkSpanProperty); }
            set { SetValue(IsInterruptWorkSpanProperty, value); }
        }
        public static readonly DependencyProperty IsInterruptWorkSpanProperty =
            DependencyProperty.Register("IsInterruptWorkSpan", typeof(bool), typeof(InterruptionCellControl), new UIPropertyMetadata(true));






        public string VirtualTimeShown
        {
            get { return (string)GetValue(VirtualTimeShownProperty); }
            set { SetValue(VirtualTimeShownProperty, value); }
        }
        public static readonly DependencyProperty VirtualTimeShownProperty =
            DependencyProperty.Register("VirtualTimeShown", typeof(string), typeof(InterruptionCellControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(onVirtualTimeShownPropertyChanged)));

        private static void onVirtualTimeShownPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            InterruptionCellControl thisControl = sender as InterruptionCellControl;
            thisControl.NotifyPropertyChanged("DisplayText");
        }

        public string DisplayText
        {
            get
            {
                if (string.IsNullOrEmpty(this.VirtualTimeShown))
                    return this.TextContent;
                else
                    return this.VirtualTimeShown;
            }
        }

        
        private void editText_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.SetCurrentValue(InterruptionCellControl.TextContentProperty, this.m_EditText.Text);
            // this.Text = this.editText.Text;
        }

        private void editText_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // We want to achieve a behaviour that typing will immediately replace contained text
            this.m_EditText.SelectAll();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
