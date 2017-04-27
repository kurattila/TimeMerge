using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.ViewModel;

namespace UnitTests
{
    [TestClass]
    public class MainViewModel_Tests
    {
        [TestMethod]
        public void JumpToYearMonthCommand_CanBeExecuted_WhenUserMadeRefresh()
        {
            MainViewModel mainVm = new FakeMainViewModel();
            mainVm.SettingsPanelVM.UserID = "dummyID";

            bool canExecute = mainVm.JumpToYearMonthCommand.CanExecute(MainViewModel.RefreshCommandOrigin.User);

            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void JumpToYearMonthCommand_CanBeExecuted_WhenAutomaticRefreshAndCellNotEditing()
        {
            MainViewModel mainVm = new FakeMainViewModel();
            mainVm.SettingsPanelVM.UserID = "dummyID";

            bool canExecute = mainVm.JumpToYearMonthCommand.CanExecute(MainViewModel.RefreshCommandOrigin.AutomaticRefresh);

            Assert.IsTrue(canExecute);
        }

        [TestMethod]
        public void JumpToYearMonthCommand_CannotBeExecuted_WhenAutomaticRefreshAndCellIsEditing()
        {
            MainViewModel mainVm = new FakeMainViewModel();
            mainVm.SettingsPanelVM.UserID = "dummyID";
            mainVm.IsCellEditingNow = true;

            bool canExecute = mainVm.JumpToYearMonthCommand.CanExecute(MainViewModel.RefreshCommandOrigin.AutomaticRefresh);

            Assert.IsFalse(canExecute);
        }

        [TestMethod]
        public void JumpToYearMonthCommand_CannotBeExecuted_WhenAutomaticRefreshAndAnyPanelShown()
        {
            MainViewModel mainVm = new FakeMainViewModel();
            mainVm.SettingsPanelVM.UserID = "dummyID";
            mainVm.IsAnyPanelShownNow = true;

            bool canExecute = mainVm.JumpToYearMonthCommand.CanExecute(MainViewModel.RefreshCommandOrigin.AutomaticRefresh);

            Assert.IsFalse(canExecute);
        }
    }
}
