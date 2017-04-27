using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeMerge.ViewModel;

namespace TimeMerge.TimeBalanceServers
{
    class TimeBalanceServer
    {
        public class TimeBalanceChangedEventArgs : EventArgs
        {
            public string BalanceAsHumanString;
        }
        public event EventHandler<TimeBalanceChangedEventArgs> TimeBalanceChanged;

        private System.Windows.Threading.DispatcherTimer mRefreshTimer;

        private MainViewModel mMainViewModel;
        private SingleMonthViewModel mMonthViewModel;
        public TimeBalanceServer(MainViewModel mainViewModel)
        {
            mMainViewModel = mainViewModel;
            mMainViewModel.PropertyChanged += mMainViewModel_PropertyChanged;

            mMonthViewModel = mainViewModel.MonthViewModel;
            mMonthViewModel.PropertyChanged += MonthViewModel_PropertyChanged;

            mRefreshTimer = new System.Windows.Threading.DispatcherTimer();
            mRefreshTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            mRefreshTimer.Interval = TimeSpan.FromSeconds(29);
            mRefreshTimer.Start();
        }

        void mMainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is MainViewModel && e.PropertyName == "MonthViewModel")
            {
                if (mMonthViewModel != null)
                    mMonthViewModel.PropertyChanged -= MonthViewModel_PropertyChanged;

                mMonthViewModel = mMainViewModel.MonthViewModel;
                if (mMainViewModel.MonthViewModel != null)
                    mMainViewModel.MonthViewModel.PropertyChanged += MonthViewModel_PropertyChanged;
            }
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (mMainViewModel.JumpToYearMonthCommand.CanExecute(MainViewModel.RefreshCommandOrigin.AutomaticRefresh))
                mMainViewModel.JumpToYearMonthCommand.Execute(MainViewModel.RefreshCommandOrigin.AutomaticRefresh);
        }

        void MonthViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BalanceWholeMonth")
            {
                string balanceAsHumanString = TimeMerge.Utils.Calculations.MonthBalanceAsHumanString(mMainViewModel.MonthViewModel.BalanceWholeMonth);
                if (TimeBalanceChanged != null)
                    TimeBalanceChanged(this, new TimeBalanceChangedEventArgs() { BalanceAsHumanString = balanceAsHumanString });

                // Title Text updated, restart the timer now
                mRefreshTimer.Stop();
                mRefreshTimer.Start();
            }
        }
    }
}
