using System;
using System.Windows.Input;
using log4net;
using TimeMerge.Utils;
using System.IO;

namespace TimeMerge.ViewModel
{
    public class MainViewModel : ObservableBase
    {
        public MainViewModel()
        { }

        public void Init()
        {
            var now = TimeMerge.Utils.Calculations.NowTime;
            this.Year = now.Year;
            this.Month = now.Month;

            this.SettingsPanelVM.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SettingsPanelVM_PropertyChanged);
            this.SettingsPanelVM.NotifyPropertyChanged("UserID");
            this.SettingsPanelVM.NotifyPropertyChanged("IsUserIDSetAlready");
            this.SettingsPanelVM.NotifyPropertyChanged("RefreshButtonTooltip");

            m_HomeOfficeDetector = createHomeOfficeDetector();

            executeJumpToYearMonth(RefreshCommandOrigin.User);
        }

        private HomeOfficeDetector m_HomeOfficeDetector;
        protected virtual HomeOfficeDetector createHomeOfficeDetector()
        {
            var detector = new HomeOfficeDetector();
            detector.HomeOfficeActivityOfDayDetected += Detector_HomeOfficeActivityOfDayDetected1;
            return detector;
        }

        private void Detector_HomeOfficeActivityOfDayDetected1(object sender, HomeOfficeDetector.HomeOfficeActivityEventArgs e)
        {
            this.MonthViewModel.HomeOfficeActivityOfDayDetected(e);
        }

        void SettingsPanelVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is SettingsPanelViewModel && e.PropertyName == "UserID")
            {
                this.NotifyPropertyChanged("UserID");
                this.NotifyPropertyChanged("IsUserIDSetAlready");
                this.NotifyPropertyChanged("RefreshButtonTooltip");
            }
        }

        SettingsPanelViewModel _settingsPanelVM;
        public SettingsPanelViewModel SettingsPanelVM
        {
            get
            {
                if (_settingsPanelVM == null)
                    _settingsPanelVM = new TimeMerge.ViewModel.SettingsPanelViewModel();
                return _settingsPanelVM;
            }
            set
            {
                _settingsPanelVM = value;
                NotifyPropertyChanged("SettingsPanelVM");
            }
        }

        public string UserID
        {
            get { return _settingsPanelVM.UserID; }
        }

        public bool IsUserIDSetAlready
        {
            get { return !string.IsNullOrEmpty(UserID); }
        }

        public string UserName
        {
            get { return Properties.Settings.Default.UserName; }
            set
            {
                Properties.Settings.Default.UserName = value;
                NotifyPropertyChanged("UserName");
            }
        }

        private SingleMonthViewModel _monthVM;
        public SingleMonthViewModel MonthViewModel
        {
            get { return _monthVM; }
            set
            {
                _monthVM = value;
                NotifyPropertyChanged("MonthViewModel");
            }
        }

        private int _year;
        public int Year
        {
            get { return _year; }
            set
            {
                _year = value;
                NotifyPropertyChanged("Year");
                NotifyPropertyChanged("YearMonthString");
            }
        }

        private int _month;
        public int Month
        {
            get { return _month; }
            set
            {
                _month = value;
                NotifyPropertyChanged("Month");
                NotifyPropertyChanged("YearMonthString");
            }
        }

        public string YearMonthString
        {
            get
            {
                DateTime yearMonth = new DateTime(this.Year, this.Month, 1);
                return yearMonth.ToString("y");
            }
        }

        public enum RefreshCommandOrigin
        {
            User,
            AutomaticRefresh
        }

        private RelayCommand _jumpToYearMonthCommand;
        public ICommand JumpToYearMonthCommand
        {
            get
            {
                if (_jumpToYearMonthCommand == null)
                    _jumpToYearMonthCommand = new RelayCommand(
                            param => executeJumpToYearMonth(param),
                            param => canExecuteJumpToYearMonth(param) );
                return _jumpToYearMonthCommand;
            }
        }

        private bool canExecuteJumpToYearMonth(object param)
        {
            if (!IsUserIDSetAlready)
                return false;

            bool isUserOriginatedCommand = param != null && param is RefreshCommandOrigin && (RefreshCommandOrigin)param == RefreshCommandOrigin.User;
            if (isUserOriginatedCommand)
            {
                return true; // user-initiated refresh always allowed
            }
            else
            {
                // timer-based refresh allowed only while not editing by user and settings panel not shown
                if (IsCellEditingNow || IsAnyPanelShownNow)
                    return false;
                else
                    return true;
            }
        }

        public ICommand HomeOfficeDayNotificationCommand;

        static readonly ILog logger = LogManager.GetLogger(typeof(MainViewModel));

        private DateTime _mostRecentJumpToYearMonthExecution = TimeMerge.Utils.Calculations.NowTime;
        private void executeJumpToYearMonth(object param)
        {
            logger.Info("executeJumpToYearMonth()");

            App nonDesignModeApp = System.Windows.Application.Current as App;
            if (nonDesignModeApp != null)
                nonDesignModeApp.EnterBusyState("Načítavanie údajov...", this.MessageBarVM);

            int perfStart = System.Environment.TickCount;

            var nowTime = TimeMerge.Utils.Calculations.NowTime;

            bool needNewMonthViewModel = false;
            if (this.MonthViewModel != null)
            {
                TimeSpan wholeMonthBalance = MonthViewModel.BalanceWholeMonth;
                this.MonthViewModel.TurnOffNotifications();
                this.MonthViewModel.SaveData(wholeMonthBalance);

                if (nowTime.Month != _mostRecentJumpToYearMonthExecution.Month)
                {
                    // automatic transition to a new month when running TimeMerge for several days in a row (e.g. date changes from "Jan" to "Feb" overnight)
                    this.Year = nowTime.Year;
                    this.Month = nowTime.Month;
                }

                if (this.Year != this.MonthViewModel.YearMonth.Year ||
                    this.Month != this.MonthViewModel.YearMonth.Month)
                {
                    needNewMonthViewModel = true;
                }
                else
                {
                    needNewMonthViewModel = false;
                    this.MonthViewModel.PopulateDataFromWebOnBackground();
                }
            }
            else
            {
                needNewMonthViewModel = true;
            }

            if (needNewMonthViewModel)
            {
                var monthData = new TimeMerge.Model.SingleMonthData() { YearMonth = new DateTime(this.Year, this.Month, 1) };

                // Reuse existing Month VM if possible. When also reusing ViewModels of individual days inside the Month VM, it will improve performance
                // since the DataGrid controls won't need to be recreated. They'll be just updated instead.
                if (this.MonthViewModel == null)
                {
                    IWebRequestObserver webRequestObserver = App.Current == null ? null : (App.Current as App).Context.WebRequestObserver;
                    this.MonthViewModel = new TimeMerge.ViewModel.SingleMonthViewModel(this, webRequestObserver, (App.Current as App)?.Context.WebDavConnection);
                }

                this.MonthViewModel.SetMonthData(monthData);
            }

            this.MonthViewModel.TurnOnNotifications();

            m_HomeOfficeDetector?.DetectNow();

            if (nonDesignModeApp != null)
                nonDesignModeApp.ExitBusyState();

            int perfStop = System.Environment.TickCount;
//             System.Windows.MessageBox.Show(string.Format("Jump took {0} msecs", perfStop - perfStart), "TimeMerge", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            _mostRecentJumpToYearMonthExecution = nowTime;
        }

        private string _titleText = "TimeMerge";
        public string TitleText
        {
            get { return _titleText; }
            set
            {
                _titleText = value;
                NotifyPropertyChanged("TitleText");
            }
        }

        private bool _isCellEditingNow;
        public bool IsCellEditingNow
        {
            get { return _isCellEditingNow; }
            set
            {
                _isCellEditingNow = value;
                NotifyPropertyChanged("IsCellEditingNow");
            }
        }

        private bool _isAnyPanelShownNow;
        public bool IsAnyPanelShownNow
        {
            get { return _isAnyPanelShownNow; }
            set
            {
                if (_isAnyPanelShownNow == value)
                    return;
                _isAnyPanelShownNow = value;
                NotifyPropertyChanged(nameof(IsAnyPanelShownNow));
            }
        }

        public string RefreshButtonTooltip
        {
            get
            {
                if (IsUserIDSetAlready)
                    return "Obnoviť";
                else
                    return "Prístupné až po zadaní UserID";
            }
        }

        public string Copyright
        {
            get
            {
                System.Reflection.Assembly app = System.Reflection.Assembly.GetExecutingAssembly();
                System.Reflection.AssemblyCopyrightAttribute copyright = (System.Reflection.AssemblyCopyrightAttribute)app.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false)[0];
                return string.Format("TimeMerge {0} {1}", TimeMerge.VersionInfo.AssemblyVersion, copyright.Copyright);
            }
        }

        MessageBarViewModel _messageBarVM;
        public MessageBarViewModel MessageBarVM
        {
            get
            {
                if (_messageBarVM == null)
                    _messageBarVM = new MessageBarViewModel();

                return _messageBarVM;
            }
        }
    }
}
