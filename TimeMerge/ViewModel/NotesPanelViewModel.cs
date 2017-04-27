using System;
using System.Linq;
using System.Windows.Input;
using TimeMerge.Model;

namespace TimeMerge.ViewModel
{
    public class NotesPanelViewModel : ObservableBase
    {
        SingleMonthViewModel _monthViewModel;
        int _dayNumber = 1;
        public NotesPanelViewModel(SingleMonthViewModel monthViewModel, int dayNumber)
        {
            _monthViewModel = monthViewModel;
            _dayNumber = dayNumber;
        }

        public string NotesTitleTextPrefix
        {
            get { return "Poznámka na deň: "; }
        }

        public string NotesTitleText
        {
            get
            {
                DateTime currentDate = new DateTime(_monthViewModel.YearMonth.Year, _monthViewModel.YearMonth.Month, _dayNumber);
                string titleText = string.Format("{0}{1}", NotesTitleTextPrefix, currentDate.ToLongDateString());
                return titleText;
            }
        }

        private SingleDayData getDayModel()
        {
            var dayModel = (from dayVM in _monthViewModel.Days.AsQueryable()
                            where dayVM.GetDayData().Day == _dayNumber
                            select dayVM.GetDayData()).FirstOrDefault();
            return dayModel;
        }

        public string NotesContent
        {
            get
            {
                return getDayModel().NotesContent;
            }
            set
            {
                getDayModel().NotesContent = value;
                NotifyPropertyChanged(nameof(NotesContent));
            }
        }

        public ICommand CloseCommand
        {
            get; set;
        }
    }
}
