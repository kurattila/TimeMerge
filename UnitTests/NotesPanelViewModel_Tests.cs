using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.ViewModel;
using TimeMerge.Utils;
using System.Threading;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class NotesPanelViewModel_Tests : IDisposable
    {
        System.Globalization.CultureInfo _prevCulture;
        System.Globalization.CultureInfo _prevUICulture;

        public NotesPanelViewModel_Tests()
        {
            _prevCulture = Thread.CurrentThread.CurrentCulture;
            _prevUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("sk");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("sk");
        }

        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = _prevCulture;
            Thread.CurrentThread.CurrentUICulture = _prevUICulture;
        }

        [TestMethod]
        public void NotesTitleText_AccordingToDayInMonth()
        {
            Calculations.NowTime = new DateTime(2017, 1, 27);
            var fakeMonthVM = new FakeMonthViewModel() { ForcedYearMonth = Calculations.NowTime };
            var notesPanelVM = new NotesPanelViewModel(fakeMonthVM, Calculations.NowTime.Day);

            string notesTitle = notesPanelVM.NotesTitleText;

            Assert.AreEqual(notesPanelVM.NotesTitleTextPrefix + "piatok, 27. januára 2017", notesTitle);
        }

        [TestMethod]
        public void NotesContent_ReadsDayModel()
        {
            Calculations.NowTime = new DateTime(2017, 1, 27);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = Calculations.NowTime.Day, IsNoWorkDay = false };
            var dayViewModel = new SingleDayViewModel();
            dayViewModel.ReInit(dayData, null, null);
            var fakeMonthVM = new FakeMonthViewModel() { ForcedYearMonth = Calculations.NowTime };
            fakeMonthVM.AddDayVM(dayViewModel);
            var dayModel = (from dayVM in fakeMonthVM.Days.AsQueryable()
                            where dayVM.GetDayData().Day == Calculations.NowTime.Day
                            select dayVM.GetDayData()).FirstOrDefault();
            var notesPanelVM = new NotesPanelViewModel(fakeMonthVM, Calculations.NowTime.Day);

            dayModel.NotesContent = "abc\ndef";

            Assert.AreEqual(notesPanelVM.NotesContent, dayModel.NotesContent);
        }

        [TestMethod]
        public void NotesContent_WritesDayModel()
        {
            Calculations.NowTime = new DateTime(2017, 1, 27);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = Calculations.NowTime.Day, IsNoWorkDay = false };
            var dayViewModel = new SingleDayViewModel();
            dayViewModel.ReInit(dayData, null, null);
            var fakeMonthVM = new FakeMonthViewModel() { ForcedYearMonth = Calculations.NowTime };
            fakeMonthVM.AddDayVM(dayViewModel);
            var dayModel = (from dayVM in fakeMonthVM.Days.AsQueryable()
                            where dayVM.GetDayData().Day == Calculations.NowTime.Day
                            select dayVM.GetDayData()).FirstOrDefault();
            var notesPanelVM = new NotesPanelViewModel(fakeMonthVM, Calculations.NowTime.Day);

            notesPanelVM.NotesContent = "abc\ndef";

            Assert.AreEqual(notesPanelVM.NotesContent, dayModel.NotesContent);
        }
    }
}
