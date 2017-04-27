using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeMerge.Model
{
    public class SingleDayData
    {
        private static int _workSpansCount = 10;
        private static int _interruptionsCount = 8;
        public SingleDayData()
        {
            WorkSpans = new WorkSpan[SingleDayData._workSpansCount];
            for (int i = 0; i < SingleDayData._workSpansCount; i++)
                WorkSpans[i] = new WorkSpan();

            WorkInterruptions = new WorkInterruption[SingleDayData._interruptionsCount];
            for (int i = 0; i < SingleDayData._interruptionsCount; i++)
            {
                WorkInterruptions[i] = new WorkInterruption();
            }
        }

        public static readonly int LunchTimeMinimalTime = 20; // minutes
        public static readonly TimeSpan MaxWorkDayDurationWithMandatoryLunchTime = TimeSpan.FromMinutes(6 * 60 + 25);

        public int Day { get; set; }
        public WorkSpan[] WorkSpans;
        public WorkInterruption[] WorkInterruptions;

        private TimeSpan calculateSumOfWorkSpans()
        {
            TimeSpan sumOfWorkSpans = TimeSpan.FromSeconds(0);

            foreach (WorkSpan span in WorkSpans)
            {
                if (span != null)
                    sumOfWorkSpans += span.Duration;
            }

            foreach (WorkInterruption interrupt in WorkInterruptions)
            {
                if (interrupt.IsWorkAsWell)
                    sumOfWorkSpans += interrupt.Duration;
            }

            return sumOfWorkSpans;
        }

        public TimeSpan Duration
        {
            get
            {
                TimeSpan dayDuration = calculateSumOfWorkSpans();
                if (this.HasAutoDecrementedLunchTime)
                    dayDuration -= TimeSpan.FromMinutes(SingleDayData.LunchTimeMinimalTime);

                return dayDuration;
            }
        }

        /// <summary>
        /// Just force XML Serializer to save a "whole-day-balance" as well (so that XSLT does not need to sum up all workspans/interruptions of the day)
        /// </summary>
        public string CachedBalanceOfDay
        {
            get { return Duration.ToString("hh\\:mm"); }
            set
            {
                // on reading from XML, just ignore this value
            }
        }

        public bool HasAutoDecrementedLunchTime
        {
            get
            {
                bool hasAutoDecrementedLunchTime = false;

                TimeSpan sumOfWorkSpans = calculateSumOfWorkSpans();

                int nonEmptyWorkSpansCount = 0;
                foreach (WorkSpan span in WorkSpans)
                {
                    if (span.Duration.Ticks > 0)
                        nonEmptyWorkSpansCount++;
                }

                int lunchInterruptionsCount = 0;
                int workFromHomeInterruptionsCount = 0;
                bool dayWillBeExpandedToFullWorkingHours = false;
                foreach (WorkInterruption interrupt in WorkInterruptions)
                {
                    if (interrupt.GetTypeIncludingCorrections() == WorkInterruption.WorkInterruptionType.OBED)
                        lunchInterruptionsCount++;
                    else if (interrupt.GetTypeIncludingCorrections() == WorkInterruption.WorkInterruptionType.PDOMA)
                        workFromHomeInterruptionsCount++;
                    else if (interrupt.GetTypeIncludingCorrections() == WorkInterruption.WorkInterruptionType.SLUZ)
                        dayWillBeExpandedToFullWorkingHours = true; // even a half-day business trip will upscale your working hours to full 8 hours
                    else if (interrupt.GetTypeIncludingCorrections() == WorkInterruption.WorkInterruptionType.DOV)
                        dayWillBeExpandedToFullWorkingHours = true;
                }

                if (!dayWillBeExpandedToFullWorkingHours)
                {
                    if ((nonEmptyWorkSpansCount == 1 && lunchInterruptionsCount == 0)
                        || (nonEmptyWorkSpansCount == 0 && workFromHomeInterruptionsCount == 1))
                    {
                        if (TimeMerge.Utils.Calculations.NowTime.Date != GetDate()
                            && sumOfWorkSpans > SingleDayData.MaxWorkDayDurationWithMandatoryLunchTime)
                        {
                            hasAutoDecrementedLunchTime = true;
                        }
                    }
                }

                return hasAutoDecrementedLunchTime;
            }
        }

        /// <summary>
        /// Returns the date this SingleDayData belongs to
        /// </summary>
        /// <returns></returns>
        public DateTime GetDate()
        {
            if (WorkSpans[0].StartTime.Ticks > 0)
                return WorkSpans[0].GetStartTimeIncludingCorrections().Date;
            else
                return WorkInterruptions[0].GetStartTimeIncludingCorrections().Date;
        }

        private bool _isNoWorkDay;
        public bool IsNoWorkDay
        {
            get { return _isNoWorkDay; }
            set
            {
                _isNoWorkDay = value;
                foreach (var interruption in WorkInterruptions)
                    interruption.ParentIsNoWorkDay = value;
            }
        }

        public bool SplitWorkSpanWithInterruption(int workSpanIndexToSplit, TimeSpan hhmmInsertedInterruptionStart, TimeSpan hhmmInsertedInterruptionEnd, WorkInterruption.WorkInterruptionType insertedInterruptionType)
        {
            // We shall turn intervals like this...
            //  7:00 - 15:00	17:17 - 18:18
            //
            // ...into intervals like this:
            //  7:00 - 12:00	12:20 - 15:00	17:17 - 18:18

            bool success = false;

            DateTime intervalToSplitStart = this.WorkSpans[workSpanIndexToSplit].GetStartTimeIncludingCorrections();
            DateTime intervalToSplitEnd = this.WorkSpans[workSpanIndexToSplit].GetEndTimeIncludingCorrections();

            // Can't add an interruption of e.g. 12:00 - 12:20 when the interval actually ends before 12:20 or starts after 12:00 or it's exactly 12:00 - 12:20 !
            if (intervalToSplitStart.TimeOfDay > hhmmInsertedInterruptionStart ||
                intervalToSplitEnd.TimeOfDay < hhmmInsertedInterruptionEnd ||
                (intervalToSplitStart.TimeOfDay == hhmmInsertedInterruptionStart && intervalToSplitEnd.TimeOfDay == hhmmInsertedInterruptionEnd))
            {
                return success;
            }

            // Shift interruptions to the right, by one
            for (int workSpanIndexToMoveOnto = this.WorkSpans.Length - 1; workSpanIndexToMoveOnto > workSpanIndexToSplit; workSpanIndexToMoveOnto--)
            {
                var copiedStartTime = this.WorkSpans[workSpanIndexToMoveOnto - 1].GetStartTimeIncludingCorrections();
                var copiedEndTime = this.WorkSpans[workSpanIndexToMoveOnto - 1].GetEndTimeIncludingCorrections();

                // If Source and Destination of the shift have the same data, then nothing to do for this one
                if (copiedStartTime == this.WorkSpans[workSpanIndexToMoveOnto].GetStartTimeIncludingCorrections() &&
                    copiedEndTime == this.WorkSpans[workSpanIndexToMoveOnto].GetEndTimeIncludingCorrections())
                {
                    continue;
                }

                // Quasi-Clone, but do _not_ use Clone() for this; we want to set only corrections, nothing else, so that "Clear Correction" will really clear that cell out completely
                this.WorkSpans[workSpanIndexToMoveOnto] = new WorkSpan()
                {
                    CorrectionStartTime = copiedStartTime,
                    CorrectionEndTime = copiedEndTime
                };
            }

            var dateBase = new DateTime(this.WorkSpans[workSpanIndexToSplit].GetEndTimeIncludingCorrections().Year,
                                        this.WorkSpans[workSpanIndexToSplit].GetEndTimeIncludingCorrections().Month,
                                        this.WorkSpans[workSpanIndexToSplit].GetEndTimeIncludingCorrections().Day);

            // Adjust the WorkSpan being split
            this.WorkSpans[workSpanIndexToSplit].CorrectionEndTime = dateBase.AddMinutes(hhmmInsertedInterruptionStart.TotalMinutes);

            // End the WorkSpan being split as a new WorkSpan
            this.WorkSpans[workSpanIndexToSplit + 1].CorrectionStartTime = dateBase.AddMinutes(hhmmInsertedInterruptionEnd.TotalMinutes);
            this.WorkSpans[workSpanIndexToSplit + 1].CorrectionEndTime = dateBase.AddMinutes(intervalToSplitEnd.Hour * 60 + intervalToSplitEnd.Minute);

            // Find out which interruptions to shift to right by one
            int interruptIndexToInsert = 0;
            foreach (WorkInterruption interrupt in this.WorkInterruptions)
            {
                if (interrupt.GetStartTimeIncludingCorrections().TimeOfDay.TotalMinutes == 0 &&
                    interrupt.GetEndTimeIncludingCorrections().TimeOfDay.TotalMinutes == 0)
                {
                    break;
                }
                if (interrupt.GetStartTimeIncludingCorrections().TimeOfDay.TotalMinutes >= hhmmInsertedInterruptionStart.TotalMinutes)
                {
                    break;
                }

                interruptIndexToInsert++;
            }
            // Intervals that need to be shifted to the right shall be shifted now
            for (int interruptIndexToMoveOnto = this.WorkInterruptions.Length - 1; interruptIndexToMoveOnto > workSpanIndexToSplit; interruptIndexToMoveOnto--)
            {
                var copiedStartTime = this.WorkInterruptions[interruptIndexToMoveOnto - 1].GetStartTimeIncludingCorrections();
                var copiedEndTime = this.WorkInterruptions[interruptIndexToMoveOnto - 1].GetEndTimeIncludingCorrections();
                var copiedType = this.WorkInterruptions[interruptIndexToMoveOnto - 1].GetTypeIncludingCorrections();

                // If Source and Destination of the shift have the same data, then nothing to do for this one
                if (copiedStartTime == this.WorkInterruptions[interruptIndexToMoveOnto].GetStartTimeIncludingCorrections() &&
                    copiedEndTime == this.WorkInterruptions[interruptIndexToMoveOnto].GetEndTimeIncludingCorrections() &&
                    copiedType == this.WorkInterruptions[interruptIndexToMoveOnto].GetTypeIncludingCorrections())
                {
                    continue;
                }

                // Quasi-Clone, but do _not_ use Clone() for this; we want to set only corrections, nothing else, so that "Clear Correction" will really clear that cell out completely
                this.WorkInterruptions[interruptIndexToMoveOnto] = new WorkInterruption()
                {
                    CorrectionStartTime = copiedStartTime,
                    CorrectionEndTime = copiedEndTime,
                    CorrectedType = copiedType
                };
            }

            // Create an interruption that actually splits our desired WorkSpan
            this.WorkInterruptions[interruptIndexToInsert].CorrectionStartTime = dateBase.AddMinutes(hhmmInsertedInterruptionStart.TotalMinutes);
            this.WorkInterruptions[interruptIndexToInsert].CorrectionEndTime = dateBase.AddMinutes(hhmmInsertedInterruptionEnd.TotalMinutes);
            this.WorkInterruptions[interruptIndexToInsert].CorrectedType = insertedInterruptionType;

            success = true;
            return success;
        }

        private bool isTimeSpanSet(TimeSpan timespan)
        {
            return (timespan.Ticks > 0);
        }

        public bool SplitWorkInterruptionWithInterruption(int workInterruptionIndexToSplit, TimeSpan hhmmInsertedInterruptionStart, TimeSpan hhmmInsertedInterruptionEnd, WorkInterruption.WorkInterruptionType insertedInterruptionType)
        {
            // We shall turn interruptions like this...
            //  7:00 - 15:00 (PDOMA)	17:17 - 18:18 (SLUZ)
            //
            // ...into intervals like this:
            //  7:00 - 12:00 (PDOMA)	12:00 - 12:20 (OBED)    12:20 - 15:00 (PDOMA)	17:17 - 18:18 (SLUZ)

            bool success = false;

            DateTime intervalToSplitStart = this.WorkInterruptions[workInterruptionIndexToSplit].GetStartTimeIncludingCorrections();
            DateTime intervalToSplitEnd = this.WorkInterruptions[workInterruptionIndexToSplit].GetEndTimeIncludingCorrections();
            WorkInterruption.WorkInterruptionType intervalToSplitType = this.WorkInterruptions[workInterruptionIndexToSplit].GetTypeIncludingCorrections();

            // Can't add an interruption of e.g. 12:00 - 12:20 when the interval actually ends before 12:20 or starts after 12:00 or it's exactly 12:00 - 12:20 !
            if (intervalToSplitStart.TimeOfDay > hhmmInsertedInterruptionStart ||
                (intervalToSplitEnd.TimeOfDay < hhmmInsertedInterruptionEnd && isTimeSpanSet(intervalToSplitEnd.TimeOfDay)) ||
                (intervalToSplitStart.TimeOfDay == hhmmInsertedInterruptionStart && intervalToSplitEnd.TimeOfDay == hhmmInsertedInterruptionEnd))
            {
                return success;
            }

            if (   !isTimeSpanSet(intervalToSplitStart.TimeOfDay)
                && !isTimeSpanSet(intervalToSplitEnd.TimeOfDay))
            {
                return success;
            }

            // Shift interruptions to the right, by 2
            for (int interruptIndexToMoveOnto = this.WorkInterruptions.Length - 1; interruptIndexToMoveOnto > workInterruptionIndexToSplit + 1; interruptIndexToMoveOnto--)
            {
                var copiedStartTime = this.WorkInterruptions[interruptIndexToMoveOnto - 2].GetStartTimeIncludingCorrections();
                var copiedEndTime = this.WorkInterruptions[interruptIndexToMoveOnto - 2].GetEndTimeIncludingCorrections();
                var copiedType = this.WorkInterruptions[interruptIndexToMoveOnto - 2].GetTypeIncludingCorrections();

                // If Source and Destination of the shift have the same data, then nothing to do for this one
                if (copiedStartTime == this.WorkInterruptions[interruptIndexToMoveOnto].GetStartTimeIncludingCorrections() &&
                    copiedEndTime == this.WorkInterruptions[interruptIndexToMoveOnto].GetEndTimeIncludingCorrections() &&
                    copiedType == this.WorkInterruptions[interruptIndexToMoveOnto].GetTypeIncludingCorrections())
                {
                    continue;
                }

                // Quasi-Clone, but do _not_ use Clone() for this; we want to set only corrections, nothing else, so that "Clear Correction" will really clear that cell out completely
                this.WorkInterruptions[interruptIndexToMoveOnto] = new WorkInterruption()
                {
                    CorrectionStartTime = copiedStartTime,
                    CorrectionEndTime = copiedEndTime,
                    CorrectedType = copiedType
                };
            }

            var dateBase = new DateTime(this.WorkInterruptions[workInterruptionIndexToSplit].GetStartTimeIncludingCorrections().Year,
                                        this.WorkInterruptions[workInterruptionIndexToSplit].GetStartTimeIncludingCorrections().Month,
                                        this.WorkInterruptions[workInterruptionIndexToSplit].GetStartTimeIncludingCorrections().Day);

            // Adjust the interruption being split
            this.WorkInterruptions[workInterruptionIndexToSplit].CorrectionEndTime = dateBase.AddMinutes(hhmmInsertedInterruptionStart.TotalMinutes);

            // Create an interruption into the middle of the split one
            this.WorkInterruptions[workInterruptionIndexToSplit + 1].CorrectionStartTime = dateBase.AddMinutes(hhmmInsertedInterruptionStart.TotalMinutes);
            this.WorkInterruptions[workInterruptionIndexToSplit + 1].CorrectionEndTime = dateBase.AddMinutes(hhmmInsertedInterruptionEnd.TotalMinutes);
            this.WorkInterruptions[workInterruptionIndexToSplit + 1].CorrectedType = insertedInterruptionType;

            // End the interruption being split as a new interval
            this.WorkInterruptions[workInterruptionIndexToSplit + 2].CorrectionStartTime = dateBase.AddMinutes(hhmmInsertedInterruptionEnd.TotalMinutes);
            this.WorkInterruptions[workInterruptionIndexToSplit + 2].CorrectionEndTime = intervalToSplitEnd;
            this.WorkInterruptions[workInterruptionIndexToSplit + 2].CorrectedType = intervalToSplitType;

            success = true;
            return success;
        }

        public void StartHomeOfficeDay(Utils.HomeOfficeDetector.HomeOfficeActivityEventArgs args)
        {
            var homeOfficeDayStart = new DateTime(args.EventTime.Year, args.EventTime.Month, args.EventTime.Day, args.EventTime.Hour, args.EventTime.Minute, 0);
            this.WorkInterruptions[0].CorrectionStartTime = homeOfficeDayStart;
            this.WorkInterruptions[0].CorrectedType = WorkInterruption.WorkInterruptionType.PDOMA;
        }

        public string NotesContent { get; set; }
    }

    public class WorkSpan : ICloneable
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public virtual TimeSpan Duration
        {
            get
            {
                var now = TimeMerge.Utils.Calculations.NowTime;

                TimeSpan retDuration = TimeSpan.FromMinutes(0);

                DateTime start = GetStartTimeIncludingCorrections();
                DateTime end = GetEndTimeIncludingCorrections();

                // For the last, yet unfinished WorkSpan today, consider current time
                // (let current time be the supposed end time of today's unfinished WorkSpan)
                if (start.Date == now.Date && /*end.Date == now.Date &&*/
                    start.TimeOfDay.TotalSeconds > 0 && end.TimeOfDay.TotalSeconds == 0)
                {
                    // ignore the seconds part so that the "end - start" operation will give whole minutes only (this is to prevent rounding errors)
                    end = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                }

                // Do not consider timespans which are not terminated yet
                if (end > start)
                {
                    retDuration = end - start;
                }
                
                return retDuration;
            }
        }

        public virtual bool HasCorrectionData
        {
            get
            {
                return (CorrectionStartTime.HasValue && CorrectionStartTime.GetValueOrDefault() != StartTime) ||
                       (CorrectionEndTime.HasValue && CorrectionEndTime.GetValueOrDefault() != EndTime);
            }
        }

        public virtual void ClearAllCorrections()
        {
            CorrectionStartTime = new Nullable<DateTime>();
            CorrectionEndTime = new Nullable<DateTime>();
        }

        public Nullable<DateTime> CorrectionStartTime { get; set; }
        public Nullable<DateTime> CorrectionEndTime   { get; set; }

        public DateTime GetStartTimeIncludingCorrections()
        {
            return (CorrectionStartTime.HasValue) ? CorrectionStartTime.GetValueOrDefault() : StartTime;
        }
        public DateTime GetEndTimeIncludingCorrections()
        {
            return (CorrectionEndTime.HasValue) ? CorrectionEndTime.GetValueOrDefault() : EndTime;
        }

        public virtual object Clone()
        {
            return new WorkSpan() { StartTime = this.StartTime, EndTime = this.EndTime, CorrectionStartTime = this.CorrectionStartTime, CorrectionEndTime = this.CorrectionEndTime };
        }
    }

    public class WorkInterruption : WorkSpan
    {
        public enum WorkInterruptionType
        {
            OTHER = 0,
            OBED  = 1,
            PDOMA = 2,
            DOV   = 3,
            LEK   = 4,
            SLUZ  = 5,
            ZP    = 6,   // "Zakonnik prace"
            OCR   = 7,
            PN    = 8
        }

        public override object Clone()
        {
            return new WorkInterruption()
            {
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                CorrectionStartTime = this.CorrectionStartTime,
                CorrectionEndTime = this.CorrectionEndTime,
                Type = this.Type,
                CorrectedType = this.CorrectedType
            };
        }

        public override TimeSpan Duration
        {
            get
            {
                TimeSpan retDuration = base.Duration;

                // Full-day holidays and Full-day business travels: count them as e.g. 8 hours
                WorkInterruptionType realType = GetTypeIncludingCorrections();
                if (retDuration == TimeSpan.FromMinutes(0) && (realType == WorkInterruptionType.DOV
                                                            || realType == WorkInterruptionType.SLUZ
                                                            || realType == WorkInterruptionType.LEK
                                                            || realType == WorkInterruptionType.ZP
                                                            || realType == WorkInterruptionType.OCR
                                                            || realType == WorkInterruptionType.PN))
                    retDuration = TimeSpan.FromHours(Properties.Settings.Default.DailyWorkHours);
                
                return retDuration;
            }
        }

        public override bool HasCorrectionData
        {
            get
            {
                return base.HasCorrectionData || (CorrectedType.HasValue && CorrectedType.GetValueOrDefault() != Type);
            }
        }

        public override void ClearAllCorrections()
        {
            base.ClearAllCorrections();
            CorrectedType = new Nullable<WorkInterruptionType>();
        }

        public WorkInterruptionType Type { get; set; }
        public Nullable<WorkInterruptionType> CorrectedType { get; set; }

        public WorkInterruptionType GetTypeIncludingCorrections()
        {
            WorkInterruptionType realType = this.Type;
            if (this.CorrectedType.HasValue)
                realType = this.CorrectedType.GetValueOrDefault();

            return realType;
        }

        public bool ParentIsNoWorkDay
        {
            get;
            set;
        }

        public bool IsWorkAsWell
        {
            get
            {
                bool isWorkAsWell = false;

                // "SLUZ" always counts as work, no matter if it's during a weekend or not
                WorkInterruptionType realType = GetTypeIncludingCorrections();
                if (realType == WorkInterruptionType.PDOMA || realType == WorkInterruptionType.SLUZ || realType == WorkInterruptionType.ZP)
                {
                    isWorkAsWell = true;
                }

                // "PN" during weekends does not count as work
                if (ParentIsNoWorkDay == false && (   realType == WorkInterruptionType.DOV
                                                   || realType == WorkInterruptionType.LEK
                                                   || realType == WorkInterruptionType.OCR
                                                   || realType == WorkInterruptionType.PN))
                {
                    isWorkAsWell = true;
                }

                return isWorkAsWell;
            }
        }

        public static WorkInterruptionType ParseInterruptionType(string interruptionTypeString)
        {
            WorkInterruptionType retType = WorkInterruptionType.OTHER;
            try
            {
                Enum.TryParse<WorkInterruptionType>(interruptionTypeString, out retType);
            }
            catch (System.ArgumentException)
            {
                // 'WorkInterruptionType' _is_ an enum type, so 'ArgumentException' should never occur
            }
            return retType;
        }
    }
}
