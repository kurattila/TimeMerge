using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeMerge.Model;
using System.Windows.Input;
using System.Globalization;

namespace TimeMerge.ViewModel
{
    public class SingleDayViewModel : ObservableBase
    {
        private SingleMonthViewModel _parentMonthVM;
        private TimeMerge.Model.SingleDayData _dayData;
        public SingleDayViewModel()
        {
            IsChangeByUserAllowed = true;
        }

        public void ReInit(TimeMerge.Model.SingleDayData dayData, SingleMonthViewModel parentMonthVM, List<TimeMerge.Model.SingleDayData> dayModelsList)
        {
            _dayData = dayData;
            _parentMonthVM = parentMonthVM;

            // Synchronize the collection of single day ViewModels with collection of single day Models
            if (dayModelsList != null && !dayModelsList.Contains(dayData))
            {
                dayModelsList.Add(dayData);
            }

            if (this.IsNotificationTurnedOn())
                notifyAllPropertiesChanged();
        }

        public override void TurnOnNotifications()
        {
            base.TurnOnNotifications();

            this.notifyAllPropertiesChanged();
        }

        private void notifyAllPropertiesChanged()
        {
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("HasAutoDecrementedLunchTime");
            NotifyPropertyChanged("IsNoWorkDay");

            NotifyPropertyChanged("WorkSpan0Start");
            NotifyPropertyChanged("WorkSpan0End");
            NotifyPropertyChanged("WorkSpan0HasCorrections");
            NotifyPropertyChanged("WorkSpan0ToolTip");
            NotifyPropertyChanged("WorkSpan0VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan1Start");
            NotifyPropertyChanged("WorkSpan1End");
            NotifyPropertyChanged("WorkSpan1HasCorrections");
            NotifyPropertyChanged("WorkSpan1ToolTip");
            NotifyPropertyChanged("WorkSpan1VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan2Start");
            NotifyPropertyChanged("WorkSpan2End");
            NotifyPropertyChanged("WorkSpan2HasCorrections");
            NotifyPropertyChanged("WorkSpan2ToolTip");
            NotifyPropertyChanged("WorkSpan2VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan3Start");
            NotifyPropertyChanged("WorkSpan3End");
            NotifyPropertyChanged("WorkSpan3HasCorrections");
            NotifyPropertyChanged("WorkSpan3ToolTip");
            NotifyPropertyChanged("WorkSpan3VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan4Start");
            NotifyPropertyChanged("WorkSpan4End");
            NotifyPropertyChanged("WorkSpan4HasCorrections");
            NotifyPropertyChanged("WorkSpan4ToolTip");
            NotifyPropertyChanged("WorkSpan4VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan5Start");
            NotifyPropertyChanged("WorkSpan5End");
            NotifyPropertyChanged("WorkSpan5HasCorrections");
            NotifyPropertyChanged("WorkSpan5ToolTip");
            NotifyPropertyChanged("WorkSpan5VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan6Start");
            NotifyPropertyChanged("WorkSpan6End");
            NotifyPropertyChanged("WorkSpan6HasCorrections");
            NotifyPropertyChanged("WorkSpan6ToolTip");
            NotifyPropertyChanged("WorkSpan6VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan7Start");
            NotifyPropertyChanged("WorkSpan7End");
            NotifyPropertyChanged("WorkSpan7HasCorrections");
            NotifyPropertyChanged("WorkSpan7ToolTip");
            NotifyPropertyChanged("WorkSpan7VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan8Start");
            NotifyPropertyChanged("WorkSpan8End");
            NotifyPropertyChanged("WorkSpan8HasCorrections");
            NotifyPropertyChanged("WorkSpan8ToolTip");
            NotifyPropertyChanged("WorkSpan8VirtualTimeEnding");

            NotifyPropertyChanged("WorkSpan9Start");
            NotifyPropertyChanged("WorkSpan9End");
            NotifyPropertyChanged("WorkSpan9HasCorrections");
            NotifyPropertyChanged("WorkSpan9ToolTip");
            NotifyPropertyChanged("WorkSpan9VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt0Start");
            NotifyPropertyChanged("Interrupt0End");
            NotifyPropertyChanged("Interrupt0Type");
            NotifyPropertyChanged("Interrupt0HasCorrections");
            NotifyPropertyChanged("Interrupt0ToolTip");
            NotifyPropertyChanged("Interrupt0VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt1Start");
            NotifyPropertyChanged("Interrupt1End");
            NotifyPropertyChanged("Interrupt1Type");
            NotifyPropertyChanged("Interrupt1HasCorrections");
            NotifyPropertyChanged("Interrupt1ToolTip");
            NotifyPropertyChanged("Interrupt1VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt2Start");
            NotifyPropertyChanged("Interrupt2End");
            NotifyPropertyChanged("Interrupt2Type");
            NotifyPropertyChanged("Interrupt2HasCorrections");
            NotifyPropertyChanged("Interrupt2ToolTip");
            NotifyPropertyChanged("Interrupt2VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt3Start");
            NotifyPropertyChanged("Interrupt3End");
            NotifyPropertyChanged("Interrupt3Type");
            NotifyPropertyChanged("Interrupt3HasCorrections");
            NotifyPropertyChanged("Interrupt3ToolTip");
            NotifyPropertyChanged("Interrupt3VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt4Start");
            NotifyPropertyChanged("Interrupt4End");
            NotifyPropertyChanged("Interrupt4Type");
            NotifyPropertyChanged("Interrupt4HasCorrections");
            NotifyPropertyChanged("Interrupt4ToolTip");
            NotifyPropertyChanged("Interrupt4VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt5Start");
            NotifyPropertyChanged("Interrupt5End");
            NotifyPropertyChanged("Interrupt5Type");
            NotifyPropertyChanged("Interrupt5HasCorrections");
            NotifyPropertyChanged("Interrupt5ToolTip");
            NotifyPropertyChanged("Interrupt5VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt6Start");
            NotifyPropertyChanged("Interrupt6End");
            NotifyPropertyChanged("Interrupt6Type");
            NotifyPropertyChanged("Interrupt6HasCorrections");
            NotifyPropertyChanged("Interrupt6ToolTip");
            NotifyPropertyChanged("Interrupt6VirtualTimeEnding");

            NotifyPropertyChanged("Interrupt7Start");
            NotifyPropertyChanged("Interrupt7End");
            NotifyPropertyChanged("Interrupt7Type");
            NotifyPropertyChanged("Interrupt7HasCorrections");
            NotifyPropertyChanged("Interrupt7ToolTip");
            NotifyPropertyChanged("Interrupt7VirtualTimeEnding");
        }
        virtual public int Day
        {
            get
            {
                return _dayData.Day;
            }
        }

        public static TimeSpan NoDuration = TimeSpan.FromTicks(0);
        virtual public TimeSpan Duration
        {
            get
            {
                if (this.IsNotificationTurnedOn())
                    return _dayData.Duration;
                else
                    return SingleDayViewModel.NoDuration;
            }
        }

        public bool HasAutoDecrementedLunchTime
        {
            get { return _dayData.HasAutoDecrementedLunchTime; }
        }

        private string getWorkSpanStart(int workSpanIndex, bool isInterruption = false)
        {
            if (!this.IsNotificationTurnedOn())
                return "";

            WorkSpan srcWorkSpan = isInterruption ? _dayData.WorkInterruptions[workSpanIndex] : _dayData.WorkSpans[workSpanIndex];
            DateTime startTimeWithCorrections = srcWorkSpan.GetStartTimeIncludingCorrections();
            if (startTimeWithCorrections.Hour == 0 && startTimeWithCorrections.Minute == 0)
                return "";
            else
                return string.Format("{0:t}", startTimeWithCorrections);
        }
        private string getWorkSpanEnd(int workSpanIndex, bool isInterruption = false)
        {
            if (!this.IsNotificationTurnedOn())
                return "";

            WorkSpan srcWorkSpan = isInterruption ? _dayData.WorkInterruptions[workSpanIndex] : _dayData.WorkSpans[workSpanIndex];
            DateTime endTimeWithCorrections = srcWorkSpan.GetEndTimeIncludingCorrections();
            if (endTimeWithCorrections.Hour == 0 && endTimeWithCorrections.Minute == 0)
                return "";
            else
                return string.Format("{0:t}", endTimeWithCorrections);
        }
        private void setWorkSpanStart(int workSpanIndex, string value, bool isInterruption = false)
        {
            WorkSpan destWorkSpan = isInterruption ? _dayData.WorkInterruptions[workSpanIndex] : _dayData.WorkSpans[workSpanIndex];
            if (this.IsChangeByUserAllowed)
            {
                destWorkSpan.CorrectionStartTime = TimeMerge.Utils.Calculations.StringToDateTime(value, _parentMonthVM.YearMonth.Year, _parentMonthVM.YearMonth.Month, Day);
            }
            else
            {
                destWorkSpan.StartTime = TimeMerge.Utils.Calculations.StringToDateTime(value, _parentMonthVM.YearMonth.Year, _parentMonthVM.YearMonth.Month, Day);
                // Setting start/end time from remote web server doesn't mean we'll lose our manual corrections
                // _dayData.WorkSpans[workSpanIndex].HasCorrectionData = false;
            }

            string changedPropertyBaseName = isInterruption ? "Interrupt" : "WorkSpan";

            NotifyPropertyChanged(string.Format("{0}{1}Start", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("HasAutoDecrementedLunchTime");
            NotifyPropertyChanged(string.Format("{0}{1}HasCorrections", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged(string.Format("{0}{1}ToolTip", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged(string.Format("{0}{1}VirtualTimeEnding", changedPropertyBaseName, workSpanIndex));
        }
        private void setWorkSpanEnd(int workSpanIndex, string value, bool isInterruption = false)
        {
            WorkSpan destWorkSpan = isInterruption ? _dayData.WorkInterruptions[workSpanIndex] : _dayData.WorkSpans[workSpanIndex];
            if (this.IsChangeByUserAllowed)
            {
                destWorkSpan.CorrectionEndTime = TimeMerge.Utils.Calculations.StringToDateTime(value, _parentMonthVM.YearMonth.Year, _parentMonthVM.YearMonth.Month, Day);
            }
            else
            {
                destWorkSpan.EndTime = TimeMerge.Utils.Calculations.StringToDateTime(value, _parentMonthVM.YearMonth.Year, _parentMonthVM.YearMonth.Month, Day);
                // Setting start/end time from remote web server doesn't mean we'll lose our manual corrections
                // _dayData.WorkSpans[workSpanIndex].HasCorrectionData = false;
            }

            string changedPropertyBaseName = isInterruption ? "Interrupt" : "WorkSpan";

            NotifyPropertyChanged(string.Format("{0}{1}End", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("HasAutoDecrementedLunchTime");
            NotifyPropertyChanged(string.Format("{0}{1}HasCorrections", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged(string.Format("{0}{1}ToolTip", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged(string.Format("{0}{1}VirtualTimeEnding", changedPropertyBaseName, workSpanIndex));
        }
        private string getWorkSpanToolTip(int workSpanIndex, bool isInterruption = false)
        {
            var now = TimeMerge.Utils.Calculations.NowTime;

            DateTime startWithCorrections, endWithCorrections;
            TimeSpan workSpanDuration;
            string hintZeroTimeHit = string.Empty;

            bool hasCorrections = false;
            if (isInterruption)
            {
                hasCorrections = _dayData.WorkInterruptions[workSpanIndex].HasCorrectionData;
                startWithCorrections = _dayData.WorkInterruptions[workSpanIndex].GetStartTimeIncludingCorrections();
                endWithCorrections = _dayData.WorkInterruptions[workSpanIndex].GetEndTimeIncludingCorrections();
                workSpanDuration = _dayData.WorkInterruptions[workSpanIndex].Duration;
            }
            else
            {
                hasCorrections = _dayData.WorkSpans[workSpanIndex].HasCorrectionData;
                startWithCorrections = _dayData.WorkSpans[workSpanIndex].GetStartTimeIncludingCorrections();
                endWithCorrections = _dayData.WorkSpans[workSpanIndex].GetEndTimeIncludingCorrections();
                workSpanDuration = _dayData.WorkSpans[workSpanIndex].Duration;
            }

            string virtualTimeEnding = getWorkSpanVirtualTimeEnding(workSpanIndex, isInterruption);
            string tooltip = null;
            if (hasCorrections)
            {
                DateTime start, end;
                if (isInterruption)
                {
                    start = _dayData.WorkInterruptions[workSpanIndex].StartTime;
                    end = _dayData.WorkInterruptions[workSpanIndex].EndTime;
                }
                else
                {
                    start = _dayData.WorkSpans[workSpanIndex].StartTime;
                    end = _dayData.WorkSpans[workSpanIndex].EndTime;
                }

                tooltip = string.Format(CultureInfo.InvariantCulture, "Korekcia z {0:t} - {1:t}", start, end);

                if (isInterruption && _dayData.WorkInterruptions[workSpanIndex].CorrectedType.HasValue)
                    tooltip += string.Format(", z typu {0}", _dayData.WorkInterruptions[workSpanIndex].CorrectedType.GetValueOrDefault());
            }

            // When "virtual time ending" is in effect, show some additional info about how long to be in work yet today
            if (!string.IsNullOrEmpty(virtualTimeEnding))
            {
                // Get current time, ignoring the seconds part
                var nowWithNoSeconds = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                DateTime calculationBaseTime;
                if (workSpanDuration.TotalMinutes > 0)
                    calculationBaseTime = nowWithNoSeconds; // current whole-month balance was calculated based on current time
                else
                    calculationBaseTime = startWithCorrections; // current whole-month balance was calculated based on WorkSpan start time

                // Calculate when the current minuses will be get rid of, at what time of today
                DateTime timeWhenZeroIsHit = calculationBaseTime.AddMinutes(-this._parentMonthVM.BalanceWholeMonth.TotalMinutes);

                TimeSpan lunchDuration = this.GetLunchDuration();

                // If the +0:00 zero balance cannot be achieved today (e.g. too much minuses), then show no info about it
                if (timeWhenZeroIsHit.DayOfYear == calculationBaseTime.DayOfYear)
                {
                    // The +0:00 zero balance can be achieved today, when user will be at work until 'timeWhenZeroIsHit'
                    if (!string.IsNullOrEmpty(tooltip))
                        tooltip += System.Environment.NewLine;
                    if (this._parentMonthVM.BalanceWholeMonth.TotalMinutes < 0)
                    {
                        hintZeroTimeHit += String.Format("+0:00 sa dosiahne o {1:t}", System.Environment.NewLine, timeWhenZeroIsHit);
                        if (lunchDuration.Ticks == 0)
                        {
                            DateTime timeWhenZeroIsHitWithLunch = timeWhenZeroIsHit + TimeSpan.FromMinutes(SingleDayData.LunchTimeMinimalTime);
                            if (timeWhenZeroIsHitWithLunch.DayOfYear == calculationBaseTime.DayOfYear)
                                hintZeroTimeHit += string.Format(" (s obedom o {0:t})", timeWhenZeroIsHitWithLunch);
                        }
                    }
                    else
                    {
                        hintZeroTimeHit += String.Format("+0:00 bol dosiahnutý o {1:t}", System.Environment.NewLine, timeWhenZeroIsHit);
                        if (lunchDuration.Ticks == 0)
                        {
                            DateTime timeWhenZeroIsHitWithLunch = timeWhenZeroIsHit + TimeSpan.FromMinutes(SingleDayData.LunchTimeMinimalTime);
                            if (timeWhenZeroIsHitWithLunch.DayOfYear == calculationBaseTime.DayOfYear)
                                hintZeroTimeHit += string.Format(" (s obedom o {0:t})", timeWhenZeroIsHitWithLunch);
                        }
                    }

                    tooltip += hintZeroTimeHit;
                }
            }

            // Notes for the day
            string notes = Utils.NotesContentFormatter.NotesWithIndent(0, NotesContent);
            if (!string.IsNullOrWhiteSpace(notes))
            {
                if (!string.IsNullOrWhiteSpace(tooltip))
                    tooltip += "\r\n";
                tooltip += notes;
            }

            _parentMonthVM.SetTimeSpanHintForZeroHit(SingleDayViewModel.GetSpanGlobalID(this.Day, workSpanIndex, isInterruption), hintZeroTimeHit);

            return tooltip;
        }

        public static int GetSpanGlobalID(int dayNumber, int workSpanIndex, bool isInterruption = false)
        {
            return dayNumber * 100 + workSpanIndex * 10 + (isInterruption ? 1 : 0);
        }

        public TimeSpan GetLunchDuration()
        {
            DateTime lunchStart = new DateTime(), lunchEnd = new DateTime();
            bool currentlyInLunchInterval = false;

            // Collect adjacent work interruptions of [OBED]...
            foreach (WorkInterruption interrupt in this._dayData.WorkInterruptions)
            {
                if (interrupt.GetTypeIncludingCorrections() == WorkInterruption.WorkInterruptionType.OBED)
                {
                    if (!currentlyInLunchInterval)
                    {
                        lunchStart = interrupt.GetStartTimeIncludingCorrections();
                        currentlyInLunchInterval = true;
                    }
                    lunchEnd = interrupt.GetEndTimeIncludingCorrections();
                }
                else if (currentlyInLunchInterval)
                {
                    // Once a lunch interval ended, we're not interested in further work interruptions
                    break;
                }
            }

            // ...and check if such an accumulated work interruption also corresponds with a "space" in work spans
            bool lunchInterruptionHasItsWorkSpanSpaceOK = false;
            bool currentlyInsideWorkInterruption = false;
            foreach (WorkSpan workSpan in this._dayData.WorkSpans)
            {
                if (!currentlyInsideWorkInterruption && workSpan.GetEndTimeIncludingCorrections() == lunchStart)
                {
                    currentlyInsideWorkInterruption = true;
                    continue;
                }

                if (currentlyInsideWorkInterruption && workSpan.GetStartTimeIncludingCorrections() == lunchEnd)
                {
                    lunchInterruptionHasItsWorkSpanSpaceOK = true;
                    break;
                }
            }
            // ...or check if such an accumulated work interruption also corresponds with a "space" in work interruptions
            if (!lunchInterruptionHasItsWorkSpanSpaceOK)
            {
                currentlyInsideWorkInterruption = false;
                foreach (WorkInterruption workInterrupt in this._dayData.WorkInterruptions)
                {
                    if (!currentlyInsideWorkInterruption && workInterrupt.GetEndTimeIncludingCorrections() == lunchStart && workInterrupt.GetTypeIncludingCorrections() != WorkInterruption.WorkInterruptionType.OBED)
                    {
                        currentlyInsideWorkInterruption = true;
                        continue;
                    }

                    if (currentlyInsideWorkInterruption && workInterrupt.GetStartTimeIncludingCorrections() == lunchEnd && workInterrupt.GetTypeIncludingCorrections() != WorkInterruption.WorkInterruptionType.OBED)
                    {
                        lunchInterruptionHasItsWorkSpanSpaceOK = true;
                        break;
                    }
                }
            }

            if (lunchInterruptionHasItsWorkSpanSpaceOK)
                return lunchEnd - lunchStart;
            else
                return TimeSpan.FromMinutes(0);
        }

        private string getWorkSpanVirtualTimeEnding(int workSpanIndex, bool isInterruption = false)
        {
            var now = TimeMerge.Utils.Calculations.NowTime;

            var span = isInterruption ? _dayData.WorkInterruptions[workSpanIndex] : _dayData.WorkSpans[workSpanIndex];
            if (span.GetStartTimeIncludingCorrections().Date != now.Date) // show virtual ending for today's work spans only
                return string.Empty;

            if (span.GetStartTimeIncludingCorrections().TimeOfDay.TotalMinutes > 0 &&
                   span.GetEndTimeIncludingCorrections().TimeOfDay.TotalMinutes == 0)
            {
                // virtual time ending is needed only for _unfinished_ work spans
                if (span.GetStartTimeIncludingCorrections() > now)
                    return "---";
                else
                    return string.Format("{0:t}", now); 
            }
            else
            {
                return string.Empty;
            }
        }

        public class AddLunchTimeInterruptionArgs : System.Windows.RoutedEventArgs
        {
            public int WorkSpanIndex { get; set; }
            public bool IsInterruption { get; set; }
            public Action<string> FailAction { get; set; }
        }
        private void addLunchTimeInterruption(AddLunchTimeInterruptionArgs args)
        {
            if (args == null)
            {
                System.Diagnostics.Debug.Fail("AddLunchTimeInterruptionArgs not specified in AddLunchTimeInterruptionCommand");
                return;
            }

            int spanIndex = args.WorkSpanIndex;
            bool isInterruption = args.IsInterruption;

            bool isSplitSuccessful = false;
            if (!isInterruption)
                isSplitSuccessful = _dayData.SplitWorkSpanWithInterruption(spanIndex, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            else
                isSplitSuccessful = _dayData.SplitWorkInterruptionWithInterruption(spanIndex, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);

            if (!isSplitSuccessful)
            {
                if (args.FailAction != null)
                    args.FailAction.Invoke("Nie je možné pridať prerušenie 12:00 - 12:20");
                return;
            }

            notifyPropertiesAfterAddingLunchTimeInterruption();
        }

        protected void notifyPropertiesAfterAddingLunchTimeInterruption()
        {
            for (int workSpanIndex = 0; workSpanIndex < _dayData.WorkSpans.Length; workSpanIndex++)
            {
                NotifyPropertyChanged(string.Format("WorkSpan{0}Start", workSpanIndex));
                NotifyPropertyChanged(string.Format("WorkSpan{0}End", workSpanIndex));
                NotifyPropertyChanged(string.Format("WorkSpan{0}HasCorrections", workSpanIndex));
                NotifyPropertyChanged(string.Format("WorkSpan{0}ToolTip", workSpanIndex));
            }
            for (int interruptIndex = 0; interruptIndex < _dayData.WorkInterruptions.Length; interruptIndex++)
            {
                NotifyPropertyChanged(string.Format("Interrupt{0}Start", interruptIndex));
                NotifyPropertyChanged(string.Format("Interrupt{0}End", interruptIndex));
                NotifyPropertyChanged(string.Format("Interrupt{0}HasCorrections", interruptIndex));
                NotifyPropertyChanged(string.Format("Interrupt{0}ToolTip", interruptIndex));
                NotifyPropertyChanged(string.Format("Interrupt{0}Type", interruptIndex));
                NotifyPropertyChanged(string.Format("Interrupt{0}VirtualTimeEnding", interruptIndex));
            }
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("HasAutoDecrementedLunchTime");
        }

        private RelayCommand _addLunchTimeInterruptionCommand;
        public ICommand AddLunchTimeInterruptionCommand
        {
            get
            {
                if (_addLunchTimeInterruptionCommand == null)
                    _addLunchTimeInterruptionCommand = new RelayCommand((args) => this.addLunchTimeInterruption(args as AddLunchTimeInterruptionArgs));
                return _addLunchTimeInterruptionCommand;
            }
        }

        private RelayCommand _startHomeOfficeDayCommand;
        public ICommand StartHomeOfficeDayCommand
        {
            get
            {
                if (_startHomeOfficeDayCommand == null)
                    _startHomeOfficeDayCommand = new RelayCommand((args) => this.startHomeOfficeDay(args as Utils.HomeOfficeDetector.HomeOfficeActivityEventArgs), (args) => this.canStartHomeOfficeDay());
                return _startHomeOfficeDayCommand;
            }
        }

        private bool canStartHomeOfficeDay()
        {
            // simply "checking the mails" in the weekend does not count as "home office"
            if (IsNoWorkDay)
                return false;
            // whole day must be completely empty, in order to start a "home office" day
            else if (_dayData.Duration.TotalMilliseconds > 0)
                return false;
            else
                return true;
        }

        private void startHomeOfficeDay(Utils.HomeOfficeDetector.HomeOfficeActivityEventArgs args)
        {
            _dayData.StartHomeOfficeDay(args);

            NotifyPropertyChanged("Interrupt0Start");
            NotifyPropertyChanged("Interrupt0End");
            NotifyPropertyChanged("Interrupt0HasCorrections");
            NotifyPropertyChanged("Interrupt0ToolTip");
            NotifyPropertyChanged("Interrupt0Type");
            NotifyPropertyChanged("Interrupt0VirtualTimeEnding");
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("HasAutoDecrementedLunchTime");
        }

        public class ClearWorkSpanCorrectionArgs : System.Windows.RoutedEventArgs
        {
            public int WorkSpanIndex { get; set; }
            public bool IsInterruption { get; set; }
        }
        private void clearWorkSpanCorrection(ClearWorkSpanCorrectionArgs args)
        {
            if (args == null)
            {
                System.Diagnostics.Debug.Fail("ClearWorkSpanCorrectionArgs not specified in ClearWorkSpanCommand");
                return;
            }

            int workSpanIndex = args.WorkSpanIndex;
            bool isInterruption = args.IsInterruption;

            if (!isInterruption)
                _dayData.WorkSpans[workSpanIndex].ClearAllCorrections();
            else
                _dayData.WorkInterruptions[workSpanIndex].ClearAllCorrections();

            string changedPropertyBaseName = isInterruption ? "Interrupt" : "WorkSpan";

            NotifyPropertyChanged(string.Format("{0}{1}Start", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged(string.Format("{0}{1}End", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("HasAutoDecrementedLunchTime");
            NotifyPropertyChanged(string.Format("{0}{1}HasCorrections", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged(string.Format("{0}{1}ToolTip", changedPropertyBaseName, workSpanIndex));
            NotifyPropertyChanged(string.Format("{0}{1}VirtualTimeEnding", changedPropertyBaseName, workSpanIndex));

            if (isInterruption)
                NotifyPropertyChanged(string.Format("Interrupt{0}Type", workSpanIndex));
        }

        private RelayCommand _clearWorkSpanCommand;
        public ICommand ClearWorkSpanCommand
        {
            get
            {
                if (_clearWorkSpanCommand == null)
                    _clearWorkSpanCommand = new RelayCommand((args) => this.clearWorkSpanCorrection(args as ClearWorkSpanCorrectionArgs));
                return _clearWorkSpanCommand;
            }
        }

        public string WorkSpan0Start
        {
            get { return getWorkSpanStart(0); }
            set { setWorkSpanStart(0, value); }
        }
        public string WorkSpan0End
        {
            get { return getWorkSpanEnd(0); }
            set { setWorkSpanEnd(0, value); }
        }
        public bool WorkSpan0HasCorrections
        {
            get { return _dayData.WorkSpans[0].HasCorrectionData; }
        }
        public string WorkSpan0ToolTip
        {
            get { return getWorkSpanToolTip(0); }
        }
        public string WorkSpan0VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(0); }
        }

        public string WorkSpan1Start
        {
            get { return getWorkSpanStart(1); }
            set { setWorkSpanStart(1, value); }
        }
        public string WorkSpan1End
        {
            get { return getWorkSpanEnd(1); }
            set { setWorkSpanEnd(1, value); }
        }
        public bool WorkSpan1HasCorrections
        {
            get { return _dayData.WorkSpans[1].HasCorrectionData; }
        }
        public string WorkSpan1ToolTip
        {
            get { return getWorkSpanToolTip(1); }
        }
        public string WorkSpan1VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(1); }
        }

        public string WorkSpan2Start
        {
            get { return getWorkSpanStart(2); }
            set { setWorkSpanStart(2, value); }
        }
        public string WorkSpan2End
        {
            get { return getWorkSpanEnd(2); }
            set { setWorkSpanEnd(2, value); }
        }
        public bool WorkSpan2HasCorrections
        {
            get { return _dayData.WorkSpans[2].HasCorrectionData; }
        }
        public string WorkSpan2ToolTip
        {
            get { return getWorkSpanToolTip(2); }
        }
        public string WorkSpan2VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(2); }
        }

        public string WorkSpan3Start
        {
            get { return getWorkSpanStart(3); }
            set { setWorkSpanStart(3, value); }
        }
        public string WorkSpan3End
        {
            get { return getWorkSpanEnd(3); }
            set { setWorkSpanEnd(3, value); }
        }
        public bool WorkSpan3HasCorrections
        {
            get { return _dayData.WorkSpans[3].HasCorrectionData; }
        }
        public string WorkSpan3ToolTip
        {
            get { return getWorkSpanToolTip(3); }
        }
        public string WorkSpan3VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(3); }
        }

        public string WorkSpan4Start
        {
            get { return getWorkSpanStart(4); }
            set { setWorkSpanStart(4, value); }
        }
        public string WorkSpan4End
        {
            get { return getWorkSpanEnd(4); }
            set { setWorkSpanEnd(4, value); }
        }
        public bool WorkSpan4HasCorrections
        {
            get { return _dayData.WorkSpans[4].HasCorrectionData; }
        }
        public string WorkSpan4ToolTip
        {
            get { return getWorkSpanToolTip(4); }
        }
        public string WorkSpan4VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(4); }
        }

        public string WorkSpan5Start
        {
            get { return getWorkSpanStart(5); }
            set { setWorkSpanStart(5, value); }
        }
        public string WorkSpan5End
        {
            get { return getWorkSpanEnd(5); }
            set { setWorkSpanEnd(5, value); }
        }
        public bool WorkSpan5HasCorrections
        {
            get { return _dayData.WorkSpans[5].HasCorrectionData; }
        }
        public string WorkSpan5ToolTip
        {
            get { return getWorkSpanToolTip(5); }
        }
        public string WorkSpan5VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(5); }
        }

        public string WorkSpan6Start
        {
            get { return getWorkSpanStart(6); }
            set { setWorkSpanStart(6, value); }
        }
        public string WorkSpan6End
        {
            get { return getWorkSpanEnd(6); }
            set { setWorkSpanEnd(6, value); }
        }
        public bool WorkSpan6HasCorrections
        {
            get { return _dayData.WorkSpans[6].HasCorrectionData; }
        }
        public string WorkSpan6ToolTip
        {
            get { return getWorkSpanToolTip(6); }
        }
        public string WorkSpan6VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(6); }
        }

        public string WorkSpan7Start
        {
            get { return getWorkSpanStart(7); }
            set { setWorkSpanStart(7, value); }
        }
        public string WorkSpan7End
        {
            get { return getWorkSpanEnd(7); }
            set { setWorkSpanEnd(7, value); }
        }
        public bool WorkSpan7HasCorrections
        {
            get { return _dayData.WorkSpans[7].HasCorrectionData; }
        }
        public string WorkSpan7ToolTip
        {
            get { return getWorkSpanToolTip(7); }
        }
        public string WorkSpan7VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(7); }
        }

        public string WorkSpan8Start
        {
            get { return getWorkSpanStart(8); }
            set { setWorkSpanStart(8, value); }
        }
        public string WorkSpan8End
        {
            get { return getWorkSpanEnd(8); }
            set { setWorkSpanEnd(8, value); }
        }
        public bool WorkSpan8HasCorrections
        {
            get { return _dayData.WorkSpans[8].HasCorrectionData; }
        }
        public string WorkSpan8ToolTip
        {
            get { return getWorkSpanToolTip(8); }
        }
        public string WorkSpan8VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(8); }
        }

        public string WorkSpan9Start
        {
            get { return getWorkSpanStart(9); }
            set { setWorkSpanStart(9, value); }
        }
        public string WorkSpan9End
        {
            get { return getWorkSpanEnd(9); }
            set { setWorkSpanEnd(9, value); }
        }
        public bool WorkSpan9HasCorrections
        {
            get { return _dayData.WorkSpans[9].HasCorrectionData; }
        }
        public string WorkSpan9ToolTip
        {
            get { return getWorkSpanToolTip(9); }
        }
        public string WorkSpan9VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(9); }
        }



        private WorkInterruption.WorkInterruptionType getInterruptionType(int interruptionIndex)
        {
            if (_dayData.WorkInterruptions[interruptionIndex].CorrectedType.HasValue)
                return _dayData.WorkInterruptions[interruptionIndex].CorrectedType.GetValueOrDefault();
            else
                return _dayData.WorkInterruptions[interruptionIndex].Type;
        }
        private void setInterruptionType(int interruptionIndex, WorkInterruption.WorkInterruptionType newType)
        {
            if (this.IsChangeByUserAllowed)
                _dayData.WorkInterruptions[interruptionIndex].CorrectedType = newType;
            else
                _dayData.WorkInterruptions[interruptionIndex].Type = newType;

            NotifyPropertyChanged(string.Format("Interrupt{0}Type", interruptionIndex));
            NotifyPropertyChanged("Duration");
            NotifyPropertyChanged("HasAutoDecrementedLunchTime");
            NotifyPropertyChanged(string.Format("Interrupt{0}HasCorrections", interruptionIndex));
            NotifyPropertyChanged(string.Format("Interrupt{0}ToolTip", interruptionIndex));
        }



        public string Interrupt0Start
        {
            get { return getWorkSpanStart(0, true); }
            set { setWorkSpanStart(0, value, true); }
        }
        public string Interrupt0End
        {
            get { return getWorkSpanEnd(0, true); }
            set { setWorkSpanEnd(0, value, true); }
        }
        public bool Interrupt0HasCorrections
        {
            get { return _dayData.WorkInterruptions[0].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt0Type
        {
            get { return getInterruptionType(0); }
            set { setInterruptionType(0, value); }
        }
        public string Interrupt0ToolTip
        {
            get { return getWorkSpanToolTip(0, true); }
        }
        public string Interrupt0VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(0, true); }
        }

        public string Interrupt1Start
        {
            get { return getWorkSpanStart(1, true); }
            set { setWorkSpanStart(1, value, true); }
        }
        public string Interrupt1End
        {
            get { return getWorkSpanEnd(1, true); }
            set { setWorkSpanEnd(1, value, true); }
        }
        public bool Interrupt1HasCorrections
        {
            get { return _dayData.WorkInterruptions[1].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt1Type
        {
            get { return getInterruptionType(1); }
            set { setInterruptionType(1, value); }
        }
        public string Interrupt1ToolTip
        {
            get { return getWorkSpanToolTip(1, true); }
        }
        public string Interrupt1VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(1, true); }
        }

        public string Interrupt2Start
        {
            get { return getWorkSpanStart(2, true); }
            set { setWorkSpanStart(2, value, true); }
        }
        public string Interrupt2End
        {
            get { return getWorkSpanEnd(2, true); }
            set { setWorkSpanEnd(2, value, true); }
        }
        public bool Interrupt2HasCorrections
        {
            get { return _dayData.WorkInterruptions[2].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt2Type
        {
            get { return getInterruptionType(2); }
            set { setInterruptionType(2, value); }
        }
        public string Interrupt2ToolTip
        {
            get { return getWorkSpanToolTip(2, true); }
        }
        public string Interrupt2VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(2, true); }
        }

        public string Interrupt3Start
        {
            get { return getWorkSpanStart(3, true); }
            set { setWorkSpanStart(3, value, true); }
        }
        public string Interrupt3End
        {
            get { return getWorkSpanEnd(3, true); }
            set { setWorkSpanEnd(3, value, true); }
        }
        public bool Interrupt3HasCorrections
        {
            get { return _dayData.WorkInterruptions[3].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt3Type
        {
            get { return getInterruptionType(3); }
            set { setInterruptionType(3, value); }
        }
        public string Interrupt3ToolTip
        {
            get { return getWorkSpanToolTip(3, true); }
        }
        public string Interrupt3VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(3, true); }
        }

        public string Interrupt4Start
        {
            get { return getWorkSpanStart(4, true); }
            set { setWorkSpanStart(4, value, true); }
        }
        public string Interrupt4End
        {
            get { return getWorkSpanEnd(4, true); }
            set { setWorkSpanEnd(4, value, true); }
        }
        public bool Interrupt4HasCorrections
        {
            get { return _dayData.WorkInterruptions[4].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt4Type
        {
            get { return getInterruptionType(4); }
            set { setInterruptionType(4, value); }
        }
        public string Interrupt4ToolTip
        {
            get { return getWorkSpanToolTip(4, true); }
        }
        public string Interrupt4VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(4, true); }
        }

        public string Interrupt5Start
        {
            get { return getWorkSpanStart(5, true); }
            set { setWorkSpanStart(5, value, true); }
        }
        public string Interrupt5End
        {
            get { return getWorkSpanEnd(5, true); }
            set { setWorkSpanEnd(5, value, true); }
        }
        public bool Interrupt5HasCorrections
        {
            get { return _dayData.WorkInterruptions[5].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt5Type
        {
            get { return getInterruptionType(5); }
            set { setInterruptionType(5, value); }
        }
        public string Interrupt5ToolTip
        {
            get { return getWorkSpanToolTip(5, true); }
        }
        public string Interrupt5VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(5, true); }
        }

        public string Interrupt6Start
        {
            get { return getWorkSpanStart(6, true); }
            set { setWorkSpanStart(6, value, true); }
        }
        public string Interrupt6End
        {
            get { return getWorkSpanEnd(6, true); }
            set { setWorkSpanEnd(6, value, true); }
        }
        public bool Interrupt6HasCorrections
        {
            get { return _dayData.WorkInterruptions[6].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt6Type
        {
            get { return getInterruptionType(6); }
            set { setInterruptionType(6, value); }
        }
        public string Interrupt6ToolTip
        {
            get { return getWorkSpanToolTip(6, true); }
        }
        public string Interrupt6VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(6, true); }
        }

        public string Interrupt7Start
        {
            get { return getWorkSpanStart(7, true); }
            set { setWorkSpanStart(7, value, true); }
        }
        public string Interrupt7End
        {
            get { return getWorkSpanEnd(7, true); }
            set { setWorkSpanEnd(7, value, true); }
        }
        public bool Interrupt7HasCorrections
        {
            get { return _dayData.WorkInterruptions[7].HasCorrectionData; }
        }
        public WorkInterruption.WorkInterruptionType Interrupt7Type
        {
            get { return getInterruptionType(7); }
            set { setInterruptionType(7, value); }
        }
        public string Interrupt7ToolTip
        {
            get { return getWorkSpanToolTip(7, true); }
        }
        public string Interrupt7VirtualTimeEnding
        {
            get { return getWorkSpanVirtualTimeEnding(7, true); }
        }

        
        public bool IsNoWorkDay
        {
            get { return _dayData.IsNoWorkDay; }
            set
            {
                _dayData.IsNoWorkDay = value;
                NotifyPropertyChanged("IsNoWorkDay");
            }
        }

        private bool _isChangeByUserAllowed;
        public bool IsChangeByUserAllowed
        {
            get { return _isChangeByUserAllowed; }
            set
            {
                _isChangeByUserAllowed = value;
                NotifyPropertyChanged("IsChangeByUserAllowed");
            }
        }

        public SingleDayData GetDayData()
        {
            return _dayData;
        }

        public string NotesContent
        {
            get { return _dayData.NotesContent; }
            set
            {
                if (_dayData.NotesContent != value)
                {
                    _dayData.NotesContent = value;
                    NotifyPropertyChanged(nameof(NotesContent));
                }
            }
        }
    }
}
