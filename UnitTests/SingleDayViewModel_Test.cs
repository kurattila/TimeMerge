using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TimeMerge.Utils;

namespace UnitTests
{
    [TestClass]
    public class SingleDayViewModel_Test
    {
        class FakeSingleDayViewModel : TimeMerge.ViewModel.SingleDayViewModel
        {
            public void ForceUpdateAfterLunchTimeAddition()
            {
                notifyPropertiesAfterAddingLunchTimeInterruption();
            }
        }

        [TestMethod]
        public void notifyPropertiesAfterAddingLunchTimeInterruption_WillUpdateVirtualTimeEnding_Always()
        {
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            var dayVM = new FakeSingleDayViewModel();
            dayVM.ReInit(dayData, null, null);
            var propertiesUpdated = new List<string>();
            dayVM.PropertyChanged += (sender, e) => propertiesUpdated.Add(e.PropertyName);

            dayVM.ForceUpdateAfterLunchTimeAddition();

            Assert.IsTrue(propertiesUpdated.Contains("Interrupt0VirtualTimeEnding"));
            Assert.IsTrue(propertiesUpdated.Contains("Interrupt1VirtualTimeEnding"));
            Assert.IsTrue(propertiesUpdated.Contains("Interrupt2VirtualTimeEnding"));
            Assert.IsTrue(propertiesUpdated.Contains("Interrupt3VirtualTimeEnding"));
            Assert.IsTrue(propertiesUpdated.Contains("Interrupt4VirtualTimeEnding"));
            Assert.IsTrue(propertiesUpdated.Contains("Interrupt5VirtualTimeEnding"));
            Assert.IsTrue(propertiesUpdated.Contains("Interrupt6VirtualTimeEnding"));
            Assert.IsTrue(propertiesUpdated.Contains("Interrupt7VirtualTimeEnding"));
        }

        [TestMethod]
        public void HomeOfficeDay_CannotBeAutoStarted_WhenNoWorkDay()
        {
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = true };
            var dayVM = new FakeSingleDayViewModel();
            dayVM.ReInit(dayData, null, null);

            bool canExecute = dayVM.StartHomeOfficeDayCommand.CanExecute(null);

            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        public void HomeOfficeDay_CannotBeAutoStarted_WhenDayNotCompletelyEmpty()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 21, 11, 59);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            var dayVM = new FakeSingleDayViewModel();
            dayVM.ReInit(dayData, null, null);

            bool canExecute = dayVM.StartHomeOfficeDayCommand.CanExecute(null);

            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        public void HomeOfficeDay_CanBeStarted_WhenWorkDayCompletelyEmpty()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 21, 11, 59);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            var dayVM = new FakeSingleDayViewModel();
            dayVM.ReInit(dayData, null, null);

            bool canExecute = dayVM.StartHomeOfficeDayCommand.CanExecute(null);

            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void SingleDayViewModel_EmptyNotes_AreNotInTooltip_WhenNoCorrections()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 21, 11, 59);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            var dayVM = new FakeSingleDayViewModel();
            dayVM.ReInit(dayData, new FakeMonthViewModel(), null);

            string tooltip = dayVM.WorkSpan0ToolTip;

            Assert.AreEqual(null, tooltip);
        }

        [TestMethod]
        public void SingleDayViewModel_Notes_ShownInTooltip_WhenNoCorrections()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 21, 11, 59);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.NotesContent = "some dummy notes";
            var dayVM = new FakeSingleDayViewModel();
            dayVM.ReInit(dayData, new FakeMonthViewModel(), null);

            string tooltip = dayVM.WorkSpan0ToolTip;

            Assert.AreEqual(NotesContentFormatter.Prefix + "some dummy notes", tooltip);
        }

        [TestMethod]
        public void SingleDayViewModel_Notes_ShownInTooltip_WhenCorrectionsExist()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 21, 11, 59);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.NotesContent = "some dummy notes";
            dayData.WorkSpans[0].CorrectionStartTime = new DateTime(2012, 10, 26, 7, 7, 7);
            var dayVM = new FakeSingleDayViewModel();
            dayVM.ReInit(dayData, new FakeMonthViewModel(), null);

            string tooltip = dayVM.WorkSpan0ToolTip;

            string expected = "Korekcia z 00:00 - 00:00\r\n+0:00 bol dosiahnutý o 21:11 (s obedom o 21:31)\r\n" + NotesContentFormatter.Prefix + "some dummy notes";
            Assert.AreEqual(expected, tooltip);
        }
    }
}
