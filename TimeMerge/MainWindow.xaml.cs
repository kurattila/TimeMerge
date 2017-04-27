using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TimeMerge.ViewModel;
using System.Windows.Interop;
using System.ComponentModel;
using System.Net;
using System.IO;
using TimeMerge.TimeBalanceServers;
using TimeMerge.Utils;

namespace TimeMerge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TimeBalanceServer _windowTitleServer;
        private MainViewModel _mainWindowVM;
        private SingleMonthViewModel _monthVM;
        private IDeskBandInfoFormatter _deskBandInfoFormatter;
        private IAppRestarter m_AppRestarter;

        public MainWindow()
        {
            InitializeComponent();

            // Citrix / Terminal Server / Remote Desktop connections can be limited to e.g. 1024x768, so that TimeMerge's height won't fit inside.
            // If so, a restricted MaxHeight will make the scrollbars appear, so that the app remains usable (does not overlap Taskbar).
            this.MaxHeight = System.Windows.SystemParameters.WorkArea.Height;

            _mainWindowVM = new TimeMerge.ViewModel.MainViewModel();
            _mainWindowVM.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(mainWindowVM_PropertyChanged);
            this.DataContext = _mainWindowVM;

            this.Closed += new EventHandler(MainWindow_Closed);

            if (Application.Current != null)
                m_AppRestarter = (Application.Current as App).GetAppRestarter();

            // Set up "Minimize" button: will hide app window to system tray
            this.minimizeButton.Click += minimizeButton_Click;
            // Enable "minimize to tray" behavior for this Window
            TimeMerge.Utils.MinimizeToTray.Enable(this);

            // Set up "Close" button and also how to handle "ApplicationCommands.Close"
            this.closeButton.Command = ApplicationCommands.Close;
            this.CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Close,
                (sender, e) => m_AppRestarter.ShutDown(),
                (sender, e) => e.CanExecute = true));

            // Set up "Refresh" button for a keyboard shortcut of 'F5'
            var refreshKeyGesture = new KeyGesture(Key.F5);
            var refreshKeyBinding = new KeyBinding(_mainWindowVM.JumpToYearMonthCommand, refreshKeyGesture);
            this.InputBindings.Add(refreshKeyBinding);

            // Set up "Minimize" button for a keyboard shortcut of 'ESC'
            var minimizeKeyGesture = new KeyGesture(Key.Escape);
            var minimizeKeyBinding = new KeyBinding(new RelayCommand((param) => { minimizeWindow(); }), minimizeKeyGesture);
            this.InputBindings.Add(minimizeKeyBinding);

            _mainWindowVM.HomeOfficeDayNotificationCommand = new RelayCommand((param) => homeOfficeStartedTrayBalloonNotification(param));
            _mainWindowVM.Init(); // only after handler for HomeOfficeDayNotificationCommand has been installed
            installEventHandlers();

            _windowTitleServer = new TimeBalanceServer(_mainWindowVM);
            _windowTitleServer.TimeBalanceChanged += new EventHandler<TimeBalanceServer.TimeBalanceChangedEventArgs>(_windowTitleServer_TimeBalanceChanged);

            ensureDeskBandInfoFormatterCreated();
            _mainWindowVM.MonthViewModel.NotifyPropertyChanged("BalanceWholeMonth");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(WndProc);

            TimeMerge.App app = Application.Current as TimeMerge.App;
            if (app.Context.MinimizedStartupRequested)
                minimizeWindow();
        }

        void minimizeWindow()
        {
            MinimizeToTray.MinimizeWindow(this);
        }

        void restoreWindow()
        {
            MinimizeToTray.RestoreWindow(this);
        }

        void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            minimizeWindow();
        }

        public void RequestRestoreWindow()
        {
            // 'Invoke()' uses CheckAccess() to make a switch if and only if we're not yet on the Dispatcher thread
            this.Dispatcher.Invoke(() => { restoreWindow(); });
        }

        public event EventHandler<string> RequestHomeOfficeStartedTrayBalloonNotification;
        private void homeOfficeStartedTrayBalloonNotification(object param)
        {
            if (param != null && param is string)
            {
                string balloonText = param as string;

                if (RequestHomeOfficeStartedTrayBalloonNotification != null)
                    RequestHomeOfficeStartedTrayBalloonNotification(this, balloonText);
            }
        }

        public void FocusToDataGrid()
        {
            this.dataGrid.Focus();
        }

        private void _windowTitleServer_TimeBalanceChanged(object sender, TimeBalanceServer.TimeBalanceChangedEventArgs e)
        {
            string balanceAsHumanString = TimeMerge.Utils.Calculations.MonthBalanceAsHumanString(_mainWindowVM.MonthViewModel.BalanceWholeMonth);
            _mainWindowVM.TitleText = string.Format("{0} TimeMerge", balanceAsHumanString);
            updateDeskBandInfo(_deskBandInfoFormatter.GetDeskBandString(_mainWindowVM.MonthViewModel));
        }

        private void updateDeskBandInfo(string balanceAsHumanString)
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource == null && _windowClosedForAppExit == false)
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { updateDeskBandInfo(balanceAsHumanString); }));
            }
            else
            {
                Int64 mainWindowHwnd = 0x0;
                if (hwndSource != null)
                    mainWindowHwnd = hwndSource.Handle.ToInt64();

                string serializedMessageToSend = string.Format("{0}|0x{1:X}|{2}"
                                                             , VersionInfo.AssemblyVersion
                                                             , mainWindowHwnd
                                                             , balanceAsHumanString);

                IntPtr deskBandWindow = Utils.DeskBandFinder.FindTimeMergeDeskBandWindow();
                Utils.WmCopyDataSender.SendWmCopyData(deskBandWindow, serializedMessageToSend);
            }
        }

        private void installEventHandlers()
        {
            _mainWindowVM.MonthViewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(MonthViewModel_PropertyChanged);
            _mainWindowVM.MonthViewModel.GuiMessageRequest += new SingleMonthViewModel.GuiMessageRequestHandler(MonthViewModel_GuiMessageRequest);
        }
        private void uninstallEventHandlers()
        {
            _monthVM.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(MonthViewModel_PropertyChanged);
            _mainWindowVM.MonthViewModel.GuiMessageRequest -= new SingleMonthViewModel.GuiMessageRequestHandler(MonthViewModel_GuiMessageRequest);
        }

        void mainWindowVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is MainViewModel && e.PropertyName == "MonthViewModel")
            {
                if (_monthVM != null)
                    uninstallEventHandlers();

                _monthVM = _mainWindowVM.MonthViewModel;
                if (_mainWindowVM.MonthViewModel != null)
                    installEventHandlers();
            }
        }

        void MonthViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBusy")
            {

            }
        }

        void MonthViewModel_GuiMessageRequest(object sender, string message)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                _mainWindowVM.MessageBarVM.AddRow(message);
            }));
        }

        private bool _windowClosedForAppExit = false;

        void MainWindow_Closed(object sender, EventArgs e)
        {
            // Declare that balance is not known any more (app exited) and also stop reporting any TimeBalance changes (they might accidentally overwrite our "---" message):
            if (_windowTitleServer != null)
                _windowTitleServer.TimeBalanceChanged -= new EventHandler<TimeBalanceServer.TimeBalanceChangedEventArgs>(_windowTitleServer_TimeBalanceChanged);
            _windowClosedForAppExit = true;
            updateDeskBandInfo("---");

            if (_mainWindowVM != null && _mainWindowVM.MonthViewModel != null)
                _mainWindowVM.MonthViewModel.SaveData(_mainWindowVM.MonthViewModel.BalanceWholeMonth);

            Properties.Settings.Default.Save();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (_mainWindowVM != null && (e.Key == Key.PageUp || e.Key == Key.PageDown))
            {
                if (e.Key == Key.PageUp)
                {
                    doIncrementalJump(false);
                }
                else if (e.Key == Key.PageDown)
                {
                    doIncrementalJump(true);
                }
            }
        }

        private void doIncrementalJump(bool jumpToNextMonth = true)
        {
            if (_mainWindowVM == null)
            {
                System.Diagnostics.Debug.Fail("doIncrementalJump(): Main Window ViewModel should be created already");
                return;
            }

            int currentYear = _mainWindowVM.Year;
            int currentMonth = _mainWindowVM.Month;

            if (jumpToNextMonth)
            {
                // Going to jump to next month in time
                currentMonth++;
                if (currentMonth > 12)
                {
                    currentMonth = 1;
                    currentYear++;
                }
            }
            else
            {
                // Going to jump to previous month in time
                currentMonth--;
                if (currentMonth < 1)
                {
                    currentYear--;
                    currentMonth = 12;
                }
            }

            // Execute the jump now
            _mainWindowVM.Year = currentYear;
            _mainWindowVM.Month = currentMonth;
            if (_mainWindowVM.JumpToYearMonthCommand != null && _mainWindowVM.JumpToYearMonthCommand.CanExecute(null))
                _mainWindowVM.JumpToYearMonthCommand.Execute(null);
        }

        private System.Windows.Threading.DispatcherTimer _checkVersionOnWebTimer;
        private System.Windows.Threading.DispatcherTimer _appRestartCheckTimer; // native WPF memory leaks cannot be handled otherwise than restarting the app from time to time
        private DateTime _appStartupDate = DateTime.Now;
        private BackgroundWorker _checkVersionOnWebBackgroundWorker;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Can close the Splash Screen now, since main window is already up
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.SystemIdle,
                                        new Action(() => (App.Current as App).CloseSplashScreen()));

            // Make sure main window is in foreground
            this.Activate();

            _checkVersionOnWebBackgroundWorker = new BackgroundWorker();
            _checkVersionOnWebBackgroundWorker.DoWork += new DoWorkEventHandler(_checkVersionOnWebBackgroundWorker_DoWork);
            _checkVersionOnWebBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_checkVersionOnWebBackgroundWorker_RunWorkerCompleted);
            _checkVersionOnWebBackgroundWorker.WorkerSupportsCancellation = true;
            _checkVersionOnWebBackgroundWorker.RunWorkerAsync();

            // Repeat version check each 24 hours (in case TimeMerge would run constantly for several days)
            _checkVersionOnWebTimer = new System.Windows.Threading.DispatcherTimer();
            _checkVersionOnWebTimer.Tick += new EventHandler(_checkVersionOnWebTimer_Tick);
            _checkVersionOnWebTimer.Interval = TimeSpan.FromHours(24);
            _checkVersionOnWebTimer.Start();

            _appRestartCheckTimer = new System.Windows.Threading.DispatcherTimer();
            _appRestartCheckTimer.Tick += _appRestartCheckTimer_Tick;
            _appRestartCheckTimer.Interval = TimeSpan.FromHours(1);
            _appRestartCheckTimer.Start();

            // Automatically do a 'Refresh' on startup (requested by Lojzo Durech)
            this._mainWindowVM.JumpToYearMonthCommand.Execute(null);
        }

        void _appRestartCheckTimer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (   _mainWindowVM.IsCellEditingNow == false  // do not restart when user is currently editing any cell
                && _appStartupDate.Date != now.Date         // let TimeMerge run at least a day before auto-restarting due to DataGrid's memory leaks
                && now.Hour >= 1 && now.Hour <= 4)          // restart only between 1:00 and 4:00
            {
                string startupArg = null;
                if (this.IsVisible == false) // TimeMerge window minimized?
                    startupArg = TimeMerge.App.MinimizedStartupArgString;

                m_AppRestarter.ShutDownAndRestart(startupArg);
            }
        }

        void _checkVersionOnWebTimer_Tick(object sender, EventArgs e)
        {
            _checkVersionOnWebBackgroundWorker.RunWorkerAsync();
        }

        string getTelemetryString()
        {
            bool homeOffice = Properties.Settings.Default.IsHomeOfficeDetectionOn;
            bool deskBandIsMonthType = Properties.Settings.Default.DeskBandShowsMonthBalance;
            bool deskBandIsShown = DeskBandFinder.FindTimeMergeDeskBandWindow() != IntPtr.Zero;

            var data = new TelemetryData();
            data.Add("HomeOffice", homeOffice ? "yes" : "no");
            data.Add("DeskBandType", deskBandIsMonthType ? "month" : "day");
            data.Add("DeskBandShown", deskBandIsShown ? "yes" : "no");
            return data.Serialize();
        }

        string waitForWebRequest(string urlRequest)
        {
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(urlRequest);
            WebReq.Method = "GET";
            HttpWebResponse webResp = null;
            try
            {
                webResp = (HttpWebResponse)WebReq.GetResponse();
            }
            catch (WebException /*ex*/)
            {
                return "";
            }

            Stream answerStream = webResp.GetResponseStream();
            StreamReader answerStreamReader = new StreamReader(answerStream, Encoding.GetEncoding(1250));
            string responseString = answerStreamReader.ReadToEnd();

            // Cleanup
            webResp.Close();

            return responseString.Trim();
        }

        void _checkVersionOnWebBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string checkNewVersionRequest = waitForWebRequest(Properties.Settings.Default.UrlCheckNewVersion);
            if (string.IsNullOrEmpty(checkNewVersionRequest))
                return;

            string computerName = System.Environment.MachineName;
            string formattedTelemetryData = getTelemetryString();
            var connectionString = string.Format("{0}?computer={1}&version={2}&telemetry={3}"
                                                , checkNewVersionRequest
                                                , computerName
                                                , VersionInfo.AssemblyVersion
                                                , formattedTelemetryData);
            var response = waitForWebRequest(connectionString);
            if (!string.IsNullOrEmpty(response))
            {
                string newVersionNumber = TimeMerge.Utils.Calculations.ParseForAppVersionNumber(response);
                if (!string.IsNullOrEmpty(newVersionNumber) && newVersionNumber.CompareTo(TimeMerge.VersionInfo.AssemblyVersion) != 0)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        _mainWindowVM.MessageBarVM.AddRow(string.Format("\nNová verzia TimeMerge {0} je prístupná \n    https://github.com/kurattila", newVersionNumber));
                    }));
                }
            }
        }

        void _checkVersionOnWebBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private static readonly int _firstWorkSpanDataGridColumnIndex = 2;
        private static readonly int _firstInterruptionDataGridColumnIndex = 22;
        private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ContextMenu interruptTypeContextMenu = this.Resources["interruptionTypeContextMenu"] as ContextMenu;
            interruptTypeContextMenu.Items.Clear();

            DataGridColumn currentCol = this.dataGrid.CurrentColumn;
            SingleDayViewModel currentDay = this.dataGrid.CurrentItem as SingleDayViewModel;
            // DataGridCellInfo currentCellInfo = this.dataGrid.CurrentCell;

            var notesPanelVM = new NotesPanelViewModel(_mainWindowVM.MonthViewModel, currentDay.Day);
            string noteMenuItemText = notesPanelVM.NotesTitleText;
            var noteForDayMenuItem = new MenuItem() { Header = noteMenuItemText };
            noteForDayMenuItem.Click += (noteClickSender, noteClickArgs) => onNotesClicked(currentDay);
            interruptTypeContextMenu.Items.Add(noteForDayMenuItem);
            interruptTypeContextMenu.Items.Add(new Separator());

            if (currentCol != null && currentDay != null) // do no context menu when right clicking e.g. on border between two cells
            {
                int clickedWorkSpanIndex = (currentCol.DisplayIndex - _firstWorkSpanDataGridColumnIndex) / 2;
                int clickedInterruptionIndex = (currentCol.DisplayIndex - _firstInterruptionDataGridColumnIndex) / 2;

                if (currentCol.DisplayIndex >= _firstInterruptionDataGridColumnIndex) // right-click over any of the "Interruptions" columns
                {
                    // bool isStartClicked = (currentCol.DisplayIndex - _firstInterruptionDataGridColumnIndex) % 2 == 0;

                    foreach (TimeMerge.Model.WorkInterruption.WorkInterruptionType interruptionType in Enum.GetValues(typeof(TimeMerge.Model.WorkInterruption.WorkInterruptionType)))
                    {
                        var menuItem = new MenuItem() { Header = interruptionType, IsCheckable = true };
                        Binding b = new Binding(string.Format("Interrupt{0}Type", clickedInterruptionIndex));
                        b.Source = currentDay;
                        b.Converter = new InterruptTypeConverter();
                        b.ConverterParameter = interruptionType;
                        b.Mode = BindingMode.TwoWay;
                        menuItem.SetBinding(MenuItem.IsCheckedProperty, b);

                        interruptTypeContextMenu.Items.Add(menuItem);
                    }

                    interruptTypeContextMenu.Items.Add(new Separator());
                }

                if (currentCol.DisplayIndex >= _firstWorkSpanDataGridColumnIndex)
                {
                    // "Add Lunch Time" menu item
                    var lunchTimeMenuItem = new MenuItem() { Header = "Pridať prestávku na obed 20 min." };
                    var lunchTimeCommandArgs = new SingleDayViewModel.AddLunchTimeInterruptionArgs();
                    lunchTimeCommandArgs.IsInterruption = currentCol.DisplayIndex >= _firstInterruptionDataGridColumnIndex;
                    lunchTimeCommandArgs.WorkSpanIndex = lunchTimeCommandArgs.IsInterruption ? clickedInterruptionIndex : clickedWorkSpanIndex;
                    lunchTimeCommandArgs.FailAction = new Action<string>((errorMessage) =>
                    {
                        _mainWindowVM.MessageBarVM.AddRow( errorMessage );
                    });
                    lunchTimeMenuItem.Click += (clickSender, clickArgs) => currentDay.AddLunchTimeInterruptionCommand.Execute(lunchTimeCommandArgs);
                    interruptTypeContextMenu.Items.Add(lunchTimeMenuItem);

                    interruptTypeContextMenu.Items.Add(new Separator());

                    // "Clear Correction" menu item
                    var menuItem = new MenuItem() { Header = "Zrušiť túto korekciu" };
                    var commandArgs = new SingleDayViewModel.ClearWorkSpanCorrectionArgs();
                    commandArgs.IsInterruption = currentCol.DisplayIndex >= _firstInterruptionDataGridColumnIndex;
                    commandArgs.WorkSpanIndex = commandArgs.IsInterruption ? clickedInterruptionIndex : clickedWorkSpanIndex;
                    menuItem.Click += (clickSender, clickArgs) => currentDay.ClearWorkSpanCommand.Execute(commandArgs);
                    interruptTypeContextMenu.Items.Add(menuItem);
                }
            }

            if (interruptTypeContextMenu.Items.Count == 0)
                e.Handled = true;
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // When user starts typing (typing a digit), we need to activate the selected cell's CellEditingTemplate
            // and also simulate a keypress of TAB, in order to move focus onto the contained 'InterruptionCellControl'
            if (!_mainWindowVM.IsCellEditingNow && isNumKey(e.Key))
            {
                var focusedElement = Keyboard.FocusedElement;
                if (focusedElement is TextBox)
                {
                    _mainWindowVM.IsCellEditingNow = true;
                    return; // do _not_ traverse focus when selecting characters inside the TextBox with mouse
                }

                this.dataGrid.BeginEdit();
                _mainWindowVM.IsCellEditingNow = true;

                UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                if (elementWithFocus != null)
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        private static bool isNumKey(Key key)
        {
            return (
                       key == Key.NumPad0
                    || key == Key.NumPad1
                    || key == Key.NumPad2
                    || key == Key.NumPad3
                    || key == Key.NumPad4
                    || key == Key.NumPad5
                    || key == Key.NumPad6
                    || key == Key.NumPad7
                    || key == Key.NumPad8
                    || key == Key.NumPad9
                    || key == Key.D0
                    || key == Key.D1
                    || key == Key.D2
                    || key == Key.D3
                    || key == Key.D4
                    || key == Key.D5
                    || key == Key.D6
                    || key == Key.D7
                    || key == Key.D8
                    || key == Key.D9
                    );
        }

        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _mainWindowVM.IsCellEditingNow = false;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e)
        {
            this.doIncrementalJump(false);
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e)
        {
            this.doIncrementalJump(true);
        }

        private Window m_AnyModalPanel;
        private void onSettingsClicked(object sender, RoutedEventArgs e)
        {
            m_AnyModalPanel = new TimeMerge.View.SettingsPanel();
            prepareModalPanel(m_AnyModalPanel, _mainWindowVM.SettingsPanelVM);

            // Position the SettingsPanel according to current placement of the 'Settings' button
            Vector closeButtonVector = VisualTreeHelper.GetOffset(this.closeButton);
            m_AnyModalPanel.Left = this.Left + closeButtonVector.X - m_AnyModalPanel.Width + 15;
            m_AnyModalPanel.Top = this.Top + closeButtonVector.Y + 60;

            // Show the SettingsPanel
            m_AnyModalPanel.Show();
        }

        void onAnyPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && (bool)e.NewValue == true)
            {
                this._mainWindowVM.IsAnyPanelShownNow = true;

                var blurEffect = new System.Windows.Media.Effects.BlurEffect() { Radius = 1 };
                this.mainGrid.Effect = blurEffect;

                blurEffect.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, new System.Windows.Media.Animation.DoubleAnimation(8.0, TimeSpan.FromMilliseconds(100)));

                this.mainGridOverlayBorder.Opacity = 0;
                this.mainGridOverlayBorder.Visibility = Visibility.Visible;
                this.mainGridOverlayBorder.BeginAnimation(UIElement.OpacityProperty, new System.Windows.Media.Animation.DoubleAnimation(0.1, TimeSpan.FromMilliseconds(100)));
            }

            if (e.NewValue != null && (bool)e.NewValue == false)
            {
                // the 'SettingsPanel' can also close itself; we need to cancel our blurring effects also in that case
                this.mainGridOverlayBorder.Visibility = Visibility.Collapsed;
                this.mainGrid.Effect = null;

                this._mainWindowVM.IsAnyPanelShownNow = false;

                // automatically do a 'Refresh': user might have changed the UserGUID in the settings panel
                this._mainWindowVM.JumpToYearMonthCommand.Execute(null);

                // switching between Month-based and CurrentDay-based DeskBand text
                ensureDeskBandInfoFormatterCreated();
            }
        }

        private void ensureDeskBandInfoFormatterCreated()
        {
            if (Properties.Settings.Default.DeskBandShowsMonthBalance)
                _deskBandInfoFormatter = new DeskBandInfoDefaultFormatter();
            else
                _deskBandInfoFormatter = new DeskBandInfoCurrentDayOnlyFormatter();
        }

        private void onMainGridOverlayClick(object sender, MouseButtonEventArgs e)
        {
            m_AnyModalPanel.Hide();
        }

//         internal const int WM_SYSCOMMAND = 0x0112;
//         internal const int SC_MINIMIZE   = 0xF020;

        // http://web.archive.org/web/20091019124817/http://www.steverands.com/2009/03/19/custom-wndproc-wpf-apps/
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Utils.WmCopyDataSender.WM_COPYDATA:
                    restoreWindow();
                    break;

//                 case WM_SYSCOMMAND:
//                     if (wParam.ToInt64() == SC_MINIMIZE)
//                         MinimizeToTray.RemoveFromSystemAltTab(this);
//                     break;
            }

            return IntPtr.Zero;
        }

        private void prepareModalPanel(Window panel, ObservableBase panelViewModel)
        {
            if (panel == null || panelViewModel == null)
                return;

            panel.Owner = this; // prevent tossing this window to background by Windows
            panel.DataContext = panelViewModel;
            panel.IsVisibleChanged += new DependencyPropertyChangedEventHandler(onAnyPanel_IsVisibleChanged);

            // Make sure the HWND is created for SettingsPanel, so that we'll be able to position it before showing it
            var hwndHelper = new WindowInteropHelper(panel);
            hwndHelper.EnsureHandle();
        }

        private void onNotesClicked(SingleDayViewModel currentDay)
        {
            m_AnyModalPanel = new TimeMerge.View.NotesPanel();
            prepareModalPanel(m_AnyModalPanel, new NotesPanelViewModel(_mainWindowVM.MonthViewModel, currentDay.Day));

            // Show the SettingsPanel
            m_AnyModalPanel.Show();
        }

    }

    class InterruptTypeConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var interruptionType = (TimeMerge.Model.WorkInterruption.WorkInterruptionType)value;
            var referenceInterruptionType = (TimeMerge.Model.WorkInterruption.WorkInterruptionType)parameter;
            return interruptionType == referenceInterruptionType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool newIsCheckedValue = (bool)value;
            var referenceInterruptionType = (TimeMerge.Model.WorkInterruption.WorkInterruptionType)parameter;
            if (newIsCheckedValue)
                return referenceInterruptionType;
            else
                return Binding.DoNothing;
        }

        #endregion
    }

    class TimeSpanSignConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan span = (TimeSpan)value;
            if (span.TotalHours < 0)
                return "-";
            else
                return "+";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    class CorrectionsFontStyleConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool hasCorrections = (bool)value;
            if (hasCorrections)
                return FontStyles.Italic;
            else
                return FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    class TimeSpanToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan) value;
            return timeSpan.Ticks;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

namespace TimeMerge.ViewModel
{
    class DesignMainViewModel : MainViewModel
    {
        public DesignMainViewModel()
        {
            var now = TimeMerge.Utils.Calculations.NowTime;

            var mainViewModel = this;
            mainViewModel.Year = now.Year;
            mainViewModel.Month = now.Month;
            mainViewModel.SettingsPanelVM.UserID = "D0C22AC4-B7BE-4B5E-B0CC-B99419E822B4";
            mainViewModel.UserName = "John Connor";

            MonthData = new TimeMerge.Model.SingleMonthData();
            MonthData.YearMonth = now;
            var webDavConnection = new WebDavConnection();
            webDavConnection.Init(SettingsPanelVM.UserID, "dummyLogin", "001122", "dummyWebDavUrl");
            mainViewModel.MonthViewModel = new SingleMonthViewModel(mainViewModel, new NoActionOnWebRequest(), webDavConnection);
            mainViewModel.MonthViewModel.SetMonthData(MonthData);

            DayData = new TimeMerge.Model.SingleDayData();
            DayData.Day = 1;
            DayData.IsNoWorkDay = false;
            DayData.WorkInterruptions[0].StartTime = now - TimeSpan.FromHours(1);
            DayData.WorkInterruptions[0].EndTime = now + TimeSpan.FromHours(7.5);
            DayData.WorkInterruptions[0].CorrectionEndTime = now + TimeSpan.FromHours(8);
            DayData.WorkInterruptions[0].Type = TimeMerge.Model.WorkInterruption.WorkInterruptionType.OBED;
            SingleDayViewModel dayVM = new SingleDayViewModel();
            dayVM.ReInit(DayData, mainViewModel.MonthViewModel, MonthData.Days);
            mainViewModel.MonthViewModel.Days.Add(dayVM);

            var holidayData = new TimeMerge.Model.SingleDayData();
            holidayData.Day = 2;
            holidayData.IsNoWorkDay = true;
            SingleDayViewModel dayVM2 = new SingleDayViewModel();
            dayVM2.ReInit(holidayData, mainViewModel.MonthViewModel, MonthData.Days);
            mainViewModel.MonthViewModel.Days.Add(dayVM2);
        }

        public TimeMerge.Model.SingleMonthData MonthData { get; private set; }
        public TimeMerge.Model.SingleDayData DayData { get; private set; }
//         public SingleMonthViewModel MonthViewModel { get; private set; }
    }

    class DesignCellControlViewModel : SingleDayViewModel
    {
        private static Model.SingleMonthData gSingleMonthData = new Model.SingleMonthData() { YearMonth = TimeMerge.Utils.Calculations.NowTime };
        private static DesignMainViewModel gMainViewModel = new DesignMainViewModel();

        public DesignCellControlViewModel()
            : base()
        {
            base.ReInit(DesignCellControlViewModel.gMainViewModel.DayData, gMainViewModel.MonthViewModel, gSingleMonthData.Days);

            this.Interrupt0Start = "07:02";
            this.Interrupt0End = "15:35";
            this.Interrupt0Type = TimeMerge.Model.WorkInterruption.WorkInterruptionType.OBED;
        }
    }
}
