using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Text.RegularExpressions;
using TimeMerge.Model;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Input;
using System.Windows.Threading;
using TimeMerge.Utils;

namespace TimeMerge.ViewModel
{
    public class SingleMonthViewModel : ObservableBase
    {
        private TimeMerge.Model.SingleMonthData _monthData;
        private MainViewModel _mainViewModel;
        public SingleMonthViewModel(MainViewModel mainViewModel, IWebRequestObserver webRequestObserver, IWebDavConnection webDavConnection)
        {
            _mainViewModel = mainViewModel;
            _webRequestObserver = webRequestObserver;
            _webDavConnection = webDavConnection;

            _days = new ObservableCollection<SingleDayViewModel>();
            _days.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_days_CollectionChanged);

            this.CorrectionsRequestCommand = new RelayCommand((obj) => onCorrectionsRequest());
        }

        internal void HomeOfficeActivityOfDayDetected(HomeOfficeDetector.HomeOfficeActivityEventArgs args)
        {
            if (!Properties.Settings.Default.IsHomeOfficeDetectionOn)
                return;

            var now = Calculations.NowTime;
            if (now.Year == YearMonth.Year && now.Month == YearMonth.Month)
            {
                int dayIndex = now.Day - 1;
                if (Days.Count > dayIndex)
                {
                    var autoStartHomeOfficeDayCommand = Days[dayIndex].StartHomeOfficeDayCommand;
                    if (autoStartHomeOfficeDayCommand.CanExecute(args))
                    {
                        autoStartHomeOfficeDayCommand.Execute(args);
                        string ballonText = string.Format("Začiatok práce z domu: {0}", args.EventTime.ToString("HH:mm"));
                        _mainViewModel.HomeOfficeDayNotificationCommand?.Execute(ballonText);
                    }
                }
            }
        }

        public override void TurnOnNotifications()
        {
            base.TurnOnNotifications();

            this.notifyAllPropertiesChanged();
            foreach (SingleDayViewModel oneDayVM in _days)
            {
                oneDayVM.TurnOnNotifications();
            }
        }

        public override void TurnOffNotifications()
        {
            base.TurnOffNotifications();

            foreach (SingleDayViewModel oneDayVM in _days)
            {
                oneDayVM.TurnOffNotifications();
            }
        }

        private void notifyAllPropertiesChanged()
        {
            NotifyPropertyChangedDeferred("BalanceWholeMonth");
            NotifyPropertyChanged("CorrectionsRequestCommand");
//             NotifyPropertyChanged("Days");
            NotifyPropertyChanged("HasCorrections");
            NotifyPropertyChanged("IsBusy");
            NotifyPropertyChanged("TransferFromPrevMonth");
            NotifyPropertyChanged("YearMonth");
            NotifyPropertyChanged("TimeWhenZeroIsHit");
        }

        public void SetMonthData(TimeMerge.Model.SingleMonthData monthData)
        {
            _monthData = monthData;

            bool loadFromFileWentOK = this.LoadData_BkgndWithModalLoop();
            if (!loadFromFileWentOK)
                PopulateDataFromWebOnBackground();
        }

        public void PopulateDataFromWebOnBackground()
        {
            if ((Application.Current as App) == null)
                return; // do _not_ deal with DesignTime

            if (string.IsNullOrEmpty(_mainViewModel.UserID))
                return;

            if (_webRequestBackgroundWorker != null)
                return; // another '_webRequestBackgroundWorker_DoWork' is still running, so do _not_ start another one: this will prevent generating duplicate day numbers!

            _webRequestBackgroundWorker = new BackgroundWorker();
            _webRequestBackgroundWorker.DoWork += new DoWorkEventHandler(_webRequestBackgroundWorker_DoWork);
            _webRequestBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_webRequestBackgroundWorker_RunWorkerCompleted);
            _webRequestBackgroundWorker.WorkerSupportsCancellation = true;
            _webRequestBackgroundWorker.RunWorkerAsync();
        }

        void _days_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SingleDayViewModel dayVM in e.OldItems)
                {
                    dayVM.PropertyChanged -= dayViewModel_PropertyChanged;
                    this._monthData.Days.Remove(dayVM.GetDayData());
                }
            }

            if (e.NewItems != null)
            {
                // We need to track changes in days' ViewModels
                foreach (SingleDayViewModel dayVM in e.NewItems)
                {
                    dayVM.PropertyChanged += dayViewModel_PropertyChanged;
                }
                
                // Adding new single day ViewModels will be solved inside SingleDayViewModel.ReInit()
            }

            NotifyPropertyChangedDeferred("BalanceWholeMonth");
        }

        void dayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyPropertyChangedDeferred("BalanceWholeMonth");

            if (e.PropertyName.Contains("HasCorrections"))
                NotifyPropertyChanged("HasCorrections");
        }

        public delegate void GuiMessageRequestHandler(object sender, string message);
        public event GuiMessageRequestHandler GuiMessageRequest;

        private IWebRequestObserver _webRequestObserver;
        private BackgroundWorker _webRequestBackgroundWorker;
        private void getDataFromWeb()
        {
            var connectionString = string.Format("http://time:8071/idware/idware.dll/os_dl2?hash={0}&r={1}&m={2}", _mainViewModel.UserID
                                                                                                                 , _monthData.YearMonth.Year
                                                                                                                 , _monthData.YearMonth.Month);
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(connectionString);
            //This time, our method is GET.
            WebReq.Method = "GET";
            //From here on, it's all the same as above.
            HttpWebResponse webResp = null;
            try
            {
                webResp = (HttpWebResponse)WebReq.GetResponse();
            }
            catch (WebException ex)
            {
                var message = String.Format("{0}\n\nPlease double-check your User ID of: \"{1}\"", ex.Message, _mainViewModel.UserID);
                if (GuiMessageRequest != null)
                    GuiMessageRequest(this, message);
                return;
            }
            //Let's show some information about the response
            Console.WriteLine(webResp.StatusCode);
            Console.WriteLine(webResp.Server);

            //Now, we read the response (the string), and output it.
            Stream answerStream = webResp.GetResponseStream();
            StreamReader answerStreamReader = new StreamReader(answerStream, Encoding.GetEncoding(1250));
            var response = answerStreamReader.ReadToEnd();
            processWebResponseString(response);

            // Cleanup
            webResp.Close();
        }

        private void processWebResponseString(string webResponseString)
        {
            string webResponseStringRaw = webResponseString.Replace(Environment.NewLine, "");
            Regex ex = new Regex("<table class=dl.*?>(.*?)</table>", RegexOptions.Multiline);

            var matchDataTable = ex.Matches(webResponseStringRaw);
            int matchIndex = -1;
            foreach (Match oneMatch in matchDataTable)
            {
                matchIndex++;
                if (oneMatch.Groups.Count < 2)
                    continue;

                CaptureCollection captures = oneMatch.Groups[1].Captures;
                if (captures.Count > 0)
                {
                    string tableContent = captures[0].Value;
                    if (matchIndex == 0)
                    {
                        extractUserName(tableContent);
                    }
                    else if (matchIndex == 1)
                    {
                        extractTimesTable(tableContent);
                    }
                    else if (matchIndex == 2)
                    {
                        extractSummaries(tableContent);
                    }
                }
            }
        }

        private void extractSummaries(string tableContent)
        {
            Regex exSummariesTimes = new Regex(@"-?\d+:\d+"); // matches strings like "176:00" or "-0:27"
            var matches = exSummariesTimes.Matches(tableContent);

            // Sample summary looks like this:
            // Mesačný FPD: 176:00; Odpracované: 24:14; Aktuálne saldo: -0:07; Prevod z min.mesiaca: -0:27; Saldo prevod: -151:46
            // Extract 4th timespan value now
            if (matches.Count >= 4)
            {
                string transferredTimeSpanRawString = matches[3].Value;
                this.TransferFromPrevMonth = Utils.Calculations.ParseHHMMToTimeSpan(transferredTimeSpanRawString);
            }
        }

        private void extractTimesTable(string tableContent)
        {
            int year = _monthData.YearMonth.Year;
            int month = _monthData.YearMonth.Month;

            var daysCreatedList = new List<SingleDayViewModel>();
            int lastDayNumberNeeded = 1;

            Regex exTableRows = new Regex("<tr>.*?<td class=\"r\">(.*?)</td>(.*?)</tr>");
            var matches = exTableRows.Matches(tableContent);
            foreach(Match oneRowMatch in matches)
            {
                int dayNumber = 0;
                bool isNoWorkDay = false;
                if (oneRowMatch.Groups.Count > 1 && oneRowMatch.Groups[1].Captures.Count > 0)
                {
                    var dayNumberString = oneRowMatch.Groups[1].Captures[0].Value;
                    if (dayNumberString.Contains("&gt;") && dayNumberString.Contains("&lt;"))
                    {
                        isNoWorkDay = true;
                        dayNumberString = dayNumberString.Replace("&gt;", "");
                        dayNumberString = dayNumberString.Replace("&lt;", "");
                    }
                    else if (dayNumberString.StartsWith("=") && dayNumberString.EndsWith("="))
                    {
                        isNoWorkDay = true;
                        dayNumberString = dayNumberString.Replace("=", "");
                    }
                    if (!Int32.TryParse(dayNumberString, out dayNumber))
                        continue; // cannot parse day number!
                    lastDayNumberNeeded = dayNumber;
                }
                if (oneRowMatch.Groups.Count > 2 && oneRowMatch.Groups[2].Captures.Count > 0)
                {
                    string oneDayData = oneRowMatch.Groups[2].Captures[0].Value;
                    oneDayData = oneDayData.Replace("&nbsp;", " ");

                    Regex exFourthToNinethCell = new Regex("(<td.*?>(.*?)</td>.*?){47}");
                    var tdMatches = exFourthToNinethCell.Matches(oneDayData);
                    foreach (Match oneTdMatch in tdMatches)
                    {
                        if (oneTdMatch.Groups.Count > 1 && oneTdMatch.Groups[2].Captures.Count > 46)
                        {
                            bool isNewDayVMToBeAdded = false;
                            IEnumerable<SingleDayViewModel> dayVMsMatchingThisDay = from oneDayVM in _days
                                                                                    where oneDayVM.Day == dayNumber
                                                                                    select oneDayVM;
                            var dayVM = dayVMsMatchingThisDay.FirstOrDefault();
                            if (dayVM == null)
                            {
                                isNewDayVMToBeAdded = true;

                                dayVM = new SingleDayViewModel();
                            }
                            TimeMerge.Model.SingleDayData dayData = getOrCreateSingleDayData(dayNumber);
                            dayVM.ReInit(dayData, this, this._monthData.Days);

                            dayVM.IsNoWorkDay = isNoWorkDay;

                            dayVM.IsChangeByUserAllowed = false;

                            dayVM.WorkSpan0Start = oneTdMatch.Groups[2].Captures[3].Value;
                            dayVM.WorkSpan0End   = oneTdMatch.Groups[2].Captures[4].Value;
                            dayVM.WorkSpan1Start = oneTdMatch.Groups[2].Captures[5].Value;
                            dayVM.WorkSpan1End   = oneTdMatch.Groups[2].Captures[6].Value;
                            dayVM.WorkSpan2Start = oneTdMatch.Groups[2].Captures[7].Value;
                            dayVM.WorkSpan2End   = oneTdMatch.Groups[2].Captures[8].Value;

                            dayVM.WorkSpan3Start = oneTdMatch.Groups[2].Captures[9].Value;
                            dayVM.WorkSpan3End   = oneTdMatch.Groups[2].Captures[10].Value;
                            dayVM.WorkSpan4Start = oneTdMatch.Groups[2].Captures[11].Value;
                            dayVM.WorkSpan4End   = oneTdMatch.Groups[2].Captures[12].Value;
                            dayVM.WorkSpan5Start = oneTdMatch.Groups[2].Captures[13].Value;
                            dayVM.WorkSpan5End   = oneTdMatch.Groups[2].Captures[14].Value;
                            dayVM.WorkSpan6Start = oneTdMatch.Groups[2].Captures[15].Value;
                            dayVM.WorkSpan6End   = oneTdMatch.Groups[2].Captures[16].Value;
                            dayVM.WorkSpan7Start = oneTdMatch.Groups[2].Captures[17].Value;
                            dayVM.WorkSpan7End   = oneTdMatch.Groups[2].Captures[18].Value;
                            dayVM.WorkSpan8Start = oneTdMatch.Groups[2].Captures[19].Value;
                            dayVM.WorkSpan8End   = oneTdMatch.Groups[2].Captures[20].Value;
                            dayVM.WorkSpan9Start = oneTdMatch.Groups[2].Captures[21].Value;
                            dayVM.WorkSpan9End   = oneTdMatch.Groups[2].Captures[22].Value;

                            dayVM.Interrupt0Start = oneTdMatch.Groups[2].Captures[23].Value;
                            dayVM.Interrupt0End   = oneTdMatch.Groups[2].Captures[24].Value;
                            dayVM.Interrupt0Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[25].Value);
                            dayVM.Interrupt1Start = oneTdMatch.Groups[2].Captures[26].Value;
                            dayVM.Interrupt1End   = oneTdMatch.Groups[2].Captures[27].Value;
                            dayVM.Interrupt1Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[28].Value);
                            dayVM.Interrupt2Start = oneTdMatch.Groups[2].Captures[29].Value;
                            dayVM.Interrupt2End   = oneTdMatch.Groups[2].Captures[30].Value;
                            dayVM.Interrupt2Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[31].Value);
                            dayVM.Interrupt3Start = oneTdMatch.Groups[2].Captures[32].Value;
                            dayVM.Interrupt3End   = oneTdMatch.Groups[2].Captures[33].Value;
                            dayVM.Interrupt3Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[34].Value);
                            dayVM.Interrupt4Start = oneTdMatch.Groups[2].Captures[35].Value;
                            dayVM.Interrupt4End   = oneTdMatch.Groups[2].Captures[36].Value;
                            dayVM.Interrupt4Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[37].Value);
                            dayVM.Interrupt5Start = oneTdMatch.Groups[2].Captures[38].Value;
                            dayVM.Interrupt5End   = oneTdMatch.Groups[2].Captures[39].Value;
                            dayVM.Interrupt5Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[40].Value);
                            dayVM.Interrupt6Start = oneTdMatch.Groups[2].Captures[41].Value;
                            dayVM.Interrupt6End   = oneTdMatch.Groups[2].Captures[42].Value;
                            dayVM.Interrupt6Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[43].Value);
                            dayVM.Interrupt7Start = oneTdMatch.Groups[2].Captures[44].Value;
                            dayVM.Interrupt7End   = oneTdMatch.Groups[2].Captures[45].Value;
                            dayVM.Interrupt7Type  = WorkInterruption.ParseInterruptionType(oneTdMatch.Groups[2].Captures[46].Value);

                            dayVM.IsChangeByUserAllowed = true;

                            if (isNewDayVMToBeAdded)
                                daysCreatedList.Add(dayVM);
                        }
                    }
                }
            }

            IEnumerable<SingleDayViewModel> daysDeleted = from oneDayVM in _days
                                                          where oneDayVM.Day > lastDayNumberNeeded
                                                          select oneDayVM;
            var daysDeletedList = daysDeleted.ToList(); // make the enumeration of IEnumerable<> happen right now, so that '_days' collection changes won't affect the 'Remove()' calls below

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (SingleDayViewModel singleDayVM in daysCreatedList)
                    _days.Add(singleDayVM);

                foreach (SingleDayViewModel singleDayVM in daysDeletedList)
                    _days.Remove(singleDayVM);
            }));
        }

        private void extractUserName(string tableContent)
        {
            Regex exUserName = new Regex("<td.*?>(.*?)</td>");
            var userNameMatch = exUserName.Match(tableContent);
            if (userNameMatch.Groups.Count > 1 && userNameMatch.Groups[1].Captures.Count > 0)
            {
                _mainViewModel.UserName = userNameMatch.Groups[1].Captures[0].Value;
            }
        }

        private TimeMerge.Model.SingleDayData getOrCreateSingleDayData(int dayNumber)
        {
            var matchingDays = from TimeMerge.Model.SingleDayData dayData in this._monthData.Days
                               where dayData.Day == dayNumber
                               select dayData;

            TimeMerge.Model.SingleDayData matchingDayData = matchingDays.SingleOrDefault();
            if (matchingDayData == null)
                matchingDayData = new TimeMerge.Model.SingleDayData();
            
            matchingDayData.Day = dayNumber;
            return matchingDayData;
        }

        void _webRequestBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            (Application.Current as App).EnterBusyState("Pripája sa...", _mainViewModel.MessageBarVM);
            getDataFromWeb();
        }

        void _webRequestBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _webRequestBackgroundWorker.DoWork -= new DoWorkEventHandler(_webRequestBackgroundWorker_DoWork);
            _webRequestBackgroundWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(_webRequestBackgroundWorker_RunWorkerCompleted);
            _webRequestBackgroundWorker = null;

            _webRequestObserver.OnWebRequestCompleted();

            (Application.Current as App).ExitBusyState();
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyPropertyChanged("IsBusy");
            }
        }

        virtual public DateTime YearMonth
        {
            get { return _monthData.YearMonth; }
        }

        protected ObservableCollection<SingleDayViewModel> _days;
        public ObservableCollection<SingleDayViewModel> Days
        {
            get { return _days; }
        }

        public TimeSpan TransferFromPrevMonth
        {
            get { return _monthData.TransferFromPrevMonth; }
            set
            {
                _monthData.TransferFromPrevMonth = value;
                NotifyPropertyChanged("TransferFromPrevMonth");
                NotifyPropertyChangedDeferred("BalanceWholeMonth");
            }
        }

        virtual public TimeSpan BalanceWholeMonth
        {
            get
            {
                if (!this.IsNotificationTurnedOn())
                    return SingleDayViewModel.NoDuration;

                var now = TimeMerge.Utils.Calculations.NowTime;
                bool isLiveMonth = this.YearMonth.Year == now.Year && this.YearMonth.Month == now.Month;
                int currentDayInMonth = now.Day;

                int dailyWorkHours = Properties.Settings.Default.DailyWorkHours;
                int workDaysCount = 0;
                foreach (SingleDayViewModel day in Days)
                {
                    if (isLiveMonth)
                    {
                        if (day.Day > currentDayInMonth)
                            continue;
                        if (day.Day == currentDayInMonth)
                        {
                            if (isLiveMonth && !day.IsNoWorkDay && day.Duration.TotalMinutes > 0)
                                workDaysCount++;
                            continue;
                        }
                    }
                    if (!day.IsNoWorkDay)
                        workDaysCount++;
                }
                TimeSpan workToBeDone = TimeSpan.FromHours(workDaysCount * dailyWorkHours);

                TimeSpan workAmountDone = TimeSpan.FromSeconds(0);
                if (Days != null)
                {
                    foreach (SingleDayViewModel day in Days)
                    {
                        workAmountDone += day.Duration;
                    }
                }

                TimeSpan balance = workAmountDone - workToBeDone;
                if (IsBalanceASummaryThroughAllMonths)
                    balance += this.TransferFromPrevMonth;
                return balance;
            }
        }

        private bool _isBalanceASummaryThroughAllMonths = true;
        /// <summary>
        /// True when "Balance" means a summary through all months; false when "Balance" means balance of this single month only
        /// </summary>
        public bool IsBalanceASummaryThroughAllMonths
        {
            set
            {
                if (_isBalanceASummaryThroughAllMonths == value)
                    return;
                _isBalanceASummaryThroughAllMonths = value;
                NotifyPropertyChanged("IsBalanceASummaryThroughAllMonths");
                NotifyPropertyChanged("BalanceWholeMonth");
            }
            get { return _isBalanceASummaryThroughAllMonths; }
        }

        private class LoadDataResult
        {
            public bool Result { get; set; }
        }

        private BackgroundWorker _loadDataBackgroundWorker;
        public bool LoadData_BkgndWithModalLoop()
        {
            LoadDataResult resultFromBackgroundWorker = new LoadDataResult() { Result = false };

            DispatcherFrame modalLoopFrame = new DispatcherFrame();

            _loadDataBackgroundWorker = new BackgroundWorker();
            _loadDataBackgroundWorker.DoWork += new DoWorkEventHandler(_loadDataBackgroundWorker_DoWork);
            _loadDataBackgroundWorker.RunWorkerCompleted += (sender, e) => modalLoopFrame.Continue = false;
            _loadDataBackgroundWorker.WorkerSupportsCancellation = true;
            _loadDataBackgroundWorker.RunWorkerAsync(resultFromBackgroundWorker);

            Dispatcher.PushFrame(modalLoopFrame);

            return resultFromBackgroundWorker.Result;
        }

        void _loadDataBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadDataResult resultWrapper = e.Argument as LoadDataResult;
            if (resultWrapper != null)
            {
                resultWrapper.Result = this.LoadData();
            }
        }

        public bool LoadData()
        {
            // Load settings
            this.IsBalanceASummaryThroughAllMonths = Properties.Settings.Default.IsBalanceASummaryThroughAllMonths;

            // Load data
            bool loadFromFileWentOK = false;

            object monthDataRaw = null;
            XmlSerializer serializer = new XmlSerializer(typeof(SingleMonthData), new Type[] { typeof(SingleMonthData) });
            using (XmlTextReader xmlReader = new XmlTextReader(getThisMonthLocalDataFilename()))
            {
                try
                {
                    monthDataRaw = serializer.Deserialize(xmlReader);
                }
                catch (InvalidOperationException)
                {
                    // OK, this month's local cache doesn't need to exist yet
                }
                finally
                {
                    xmlReader.Close();
                }
            }

            if (monthDataRaw is SingleMonthData)
            {
                SingleMonthData monthData = monthDataRaw as SingleMonthData;

                // Copy over the loaded SingleMonthData instance to our own, local instance of SingleMonthData.
                // Try recycling each ViewModel if possible, it will improve performance.
                foreach (SingleDayData dayData in monthData.Days)
                {
                    var vmToUpdate = from SingleDayViewModel selectedDayVM in this.Days
                                     where selectedDayVM.Day == dayData.Day
                                     select selectedDayVM;
                    SingleDayViewModel dayVM = vmToUpdate.SingleOrDefault();
                    if (dayVM != null)
                    {
                        // can recycle
                        dayVM.ReInit(dayData, this, this._monthData.Days);
                    }
                    else
                    {
                        // can't recycle
                        dayVM = new SingleDayViewModel();
                        dayVM.ReInit(dayData, this, this._monthData.Days);
                        addSingleDayVM_onGuiThread(dayVM); // always access 'this.Days' collection on the Dispatcher thread
                    }
                }

                // Get rid of any superfluous single day ViewModels (e.g. if previously shown month had 31 days,
                // but the month we're going to shown now has only 30 or even less days.
                if (monthData.Days.Count > 0)
                {
                    List<SingleDayViewModel> viewmodelsToDelete = new List<SingleDayViewModel>();

                    for (int dayToDelete = monthData.Days.Count + 1; dayToDelete <= this.Days.Count; dayToDelete++)
                    {
                        var toDelete = from SingleDayViewModel selectedDayVM in this.Days
                                       where selectedDayVM.Day == dayToDelete
                                       select selectedDayVM;
                        SingleDayViewModel vmToDelete = toDelete.SingleOrDefault();
                        if (vmToDelete != null)
                            viewmodelsToDelete.Add(vmToDelete);
                    }

                    // Delete all collected day VMs at once, since this deletion directly alters elements of 'this.Days.Count'
                    foreach (var vmToDelete in viewmodelsToDelete)
                        removeSingleDayVM_onGuiThread(vmToDelete); // always access 'this.Days' collection on the Dispatcher thread
                }

                this.TransferFromPrevMonth = monthData.TransferFromPrevMonth;

                loadFromFileWentOK = true;
            }

            return loadFromFileWentOK;
        }

        private void addSingleDayVM_onGuiThread(SingleDayViewModel dayVM)
        {
            App.Current.Dispatcher.Invoke(new Action(() => this.Days.Add(dayVM)));
        }

        private void removeSingleDayVM_onGuiThread(SingleDayViewModel dayVM)
        {
            App.Current.Dispatcher.Invoke(new Action(() => this.Days.Remove(dayVM)));
        }

        IWebDavConnection _webDavConnection;
        public void SaveData(TimeSpan wholeMonthBalanceToSave)
        {
            // Simply serialize the whole SingleMonthData instance of ours
            XmlSerializer serializer = new XmlSerializer(typeof(SingleMonthData), new Type[] { typeof(SingleMonthData) });
            using (XmlTextWriter xmlWriter = new XmlTextWriter(getThisMonthLocalDataFilename(), Encoding.Unicode))
            {
                xmlWriter.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"TimeMerge_Data.xsl\"");

                this._monthData.CachedBalanceOfMonth = TimeMerge.Utils.Calculations.MonthBalanceAsHumanString(wholeMonthBalanceToSave);

                serializer.Serialize(xmlWriter, this._monthData);
                xmlWriter.Close();
            }

            if (Properties.Settings.Default.IsWebAccessOn)
                _webDavConnection?.Upload(getThisMonthLocalDataFilename());

            // Saving settings
            Properties.Settings.Default.IsBalanceASummaryThroughAllMonths = this.IsBalanceASummaryThroughAllMonths;
        }

        private string getThisMonthLocalDataFilename()
        {
            return string.Format("TimeMerge_Data_{0:D4}{1:D2}.xml", this.YearMonth.Year, this.YearMonth.Month);
        }

        public bool HasCorrections
        {
            get
            {
                var correctedDayVMs = from SingleDayViewModel dayVM in this.Days
                                      where (dayVM.Interrupt0HasCorrections || dayVM.Interrupt1HasCorrections || dayVM.Interrupt2HasCorrections || dayVM.Interrupt3HasCorrections || dayVM.Interrupt4HasCorrections || dayVM.Interrupt5HasCorrections || dayVM.Interrupt6HasCorrections || dayVM.Interrupt7HasCorrections ||
                                             dayVM.WorkSpan0HasCorrections || dayVM.WorkSpan1HasCorrections || dayVM.WorkSpan2HasCorrections || dayVM.WorkSpan3HasCorrections || dayVM.WorkSpan4HasCorrections || dayVM.WorkSpan5HasCorrections || dayVM.WorkSpan6HasCorrections || dayVM.WorkSpan7HasCorrections || dayVM.WorkSpan8HasCorrections || dayVM.WorkSpan9HasCorrections)
                                      select dayVM;
                bool anyDaysWithCorrections = correctedDayVMs.Count() > 0;
                return anyDaysWithCorrections;
            }
        }
        public ICommand CorrectionsRequestCommand
        {
            get;
            private set;
        }
        private void onCorrectionsRequest()
        {
            string monthCorrections = this.YearMonth.ToString("Y") + ":"; // e.g. "June, 2009:" for (en-US)
            foreach (SingleDayViewModel dayVM in this.Days)
            {
                string dayCorrections = string.Empty;
                bool hasWorkSpanCorrections = false;
                bool hasInterruptCorrections = false;

                //////////////////////////////////////////////////////////////////////////
                // Normal Work Corrections
                if (dayVM.WorkSpan0HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan0Start, dayVM.WorkSpan0End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan1HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan1Start, dayVM.WorkSpan1End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan2HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan2Start, dayVM.WorkSpan2End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan3HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan3Start, dayVM.WorkSpan3End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan4HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan4Start, dayVM.WorkSpan4End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan5HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan5Start, dayVM.WorkSpan5End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan6HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan6Start, dayVM.WorkSpan6End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan7HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan7Start, dayVM.WorkSpan7End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan8HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan8Start, dayVM.WorkSpan8End);
                    hasWorkSpanCorrections = true;
                }
                if (dayVM.WorkSpan9HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0} - {1}\n", dayVM.WorkSpan9Start, dayVM.WorkSpan9End);
                    hasWorkSpanCorrections = true;
                }

                //////////////////////////////////////////////////////////////////////////
                // Interruptions Corrections
                if (dayVM.Interrupt0HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt0Start, dayVM.Interrupt0End, dayVM.Interrupt0Type));
                    hasInterruptCorrections = true;
                }
                if (dayVM.Interrupt1HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt1Start, dayVM.Interrupt1End, dayVM.Interrupt1Type));
                    hasInterruptCorrections = true;
                }
                if (dayVM.Interrupt2HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt2Start, dayVM.Interrupt2End, dayVM.Interrupt2Type));
                    hasInterruptCorrections = true;
                }
                if (dayVM.Interrupt3HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt3Start, dayVM.Interrupt3End, dayVM.Interrupt3Type));
                    hasInterruptCorrections = true;
                }
                if (dayVM.Interrupt4HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt4Start, dayVM.Interrupt4End, dayVM.Interrupt4Type));
                    hasInterruptCorrections = true;
                }
                if (dayVM.Interrupt5HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt5Start, dayVM.Interrupt5End, dayVM.Interrupt5Type));
                    hasInterruptCorrections = true;
                }
                if (dayVM.Interrupt6HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt6Start, dayVM.Interrupt6End, dayVM.Interrupt6Type));
                    hasInterruptCorrections = true;
                }
                if (dayVM.Interrupt7HasCorrections)
                {
                    dayCorrections += string.Format("\t\t{0}\n", formatInterruptionCorrection(dayVM.Interrupt7Start, dayVM.Interrupt7End, dayVM.Interrupt7Type));
                    hasInterruptCorrections = true;
                }

                if (!string.IsNullOrWhiteSpace(dayVM.NotesContent))
                {
                    dayCorrections += Utils.NotesContentFormatter.NotesWithIndent(2, dayVM.NotesContent);
                }

                //////////////////////////////////////////////////////////////////////////
                // Summary
                if (hasWorkSpanCorrections || hasInterruptCorrections)
                {
                    dayCorrections = string.Format("\t{0}.: \n{1}", dayVM.Day, dayCorrections);
                    monthCorrections += string.Format("\n{0}", dayCorrections);
                }
            }

            // Render our E-mail request so that the default Mail Client will be able to send it
            // (syntax is e.g. "mailto:abc@abc.com?cc=def@def.com&subject=this is my subject&body=this is my body")
            var mailMessage = new StringBuilder();
            mailMessage.Append("mailto:" + Properties.Settings.Default.MailToRecipients);
            mailMessage.Append("?cc=" + Properties.Settings.Default.MailCcRecipients);
            mailMessage.Append("&subject=Korekcia");
            mailMessage.Append("&body=" + monthCorrections);

            TimeMerge.Utils.SendMail.Send(mailMessage.ToString());
        }

        private string formatInterruptionCorrection(string start, string end, TimeMerge.Model.WorkInterruption.WorkInterruptionType type)
        {
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
                return string.Format("{0}:00: {1}", Properties.Settings.Default.DailyWorkHours, type);
            else
                return string.Format("{0} - {1} [{2}]", start, end, type);
        }

        Dictionary<int, string> _spanHintsForZeroTimeHit = new Dictionary<int, string>();
        public void SetTimeSpanHintForZeroHit(int spanGlobalId, string hintForZeroTimeHit)
        {
            if (string.IsNullOrEmpty(hintForZeroTimeHit))
                _spanHintsForZeroTimeHit.Remove(spanGlobalId);
            else
                _spanHintsForZeroTimeHit[spanGlobalId] = hintForZeroTimeHit;

            NotifyPropertyChanged("TimeWhenZeroIsHit");
        }

        public string TimeWhenZeroIsHit
        {
            get
            {
                var spanIDAndHintPairFound = _spanHintsForZeroTimeHit.FirstOrDefault((spanIDAndHintPair) => !string.IsNullOrEmpty(spanIDAndHintPair.Value));
                return spanIDAndHintPairFound.Value;
            }
        }
    }
}
