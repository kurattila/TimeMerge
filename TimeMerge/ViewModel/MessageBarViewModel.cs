using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeMerge.ViewModel
{
    public class MessageBarViewModel : ObservableBase
    {
        public static int TimeOutMillisecs = 3000;

        System.Windows.Threading.DispatcherTimer _displayTimeOutTimer;

        public void AddRow(string text)
        {
            if (string.IsNullOrEmpty(_multiRowText))
                MultiRowText = text;
            else
                MultiRowText = string.Format("{0}\n{1}", _multiRowText, text);

            NotifyPropertyChanged("IsShown");

            resetTimerSeam();
        }

        string _multiRowText;
        public string MultiRowText
        {
            get { return _multiRowText; }
            private set
            {
                if (_multiRowText != value)
                {
                    _multiRowText = value;
                    NotifyPropertyChanged("MultiRowText");
                }
            }
        }

        public bool IsShown
        {
            get
            {
                return !string.IsNullOrEmpty(_multiRowText);
            }
        }

        protected virtual void resetTimerSeam()
        {
            if (_displayTimeOutTimer == null)
                _displayTimeOutTimer = new System.Windows.Threading.DispatcherTimer();

            _displayTimeOutTimer.Interval = TimeSpan.FromMilliseconds(MessageBarViewModel.TimeOutMillisecs);
            _displayTimeOutTimer.Tick += (senderObject, args) => { timerTickSeam(); };
            _displayTimeOutTimer.Start();
        }

        protected virtual void timerTickSeam()
        {
            if (_displayTimeOutTimer != null)
                _displayTimeOutTimer.Stop();

            MultiRowText = null;
            NotifyPropertyChanged("IsShown");
        }
    }
}
