using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.ViewModel;

namespace UnitTests
{
    /// <summary>
    /// Summary description for MessageBarViewModel_Tests
    /// </summary>
    [TestClass]
    public class MessageBarViewModel_Tests
    {
        public MessageBarViewModel_Tests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void MessageBarVM_Texts_AreInitiallyEmpty()
        {
            MessageBarViewModel vm = new MessageBarViewModel();

            Assert.IsTrue(string.IsNullOrEmpty(vm.MultiRowText));
        }

        [TestMethod]
        public void MessabeBarVM_AddRow_WillAddText_IfPreviouslyEmpty()
        {
            MessageBarViewModel vm = new MessageBarViewModel();

            string message = "Any new message.";
            vm.AddRow(message);

            Assert.IsTrue(vm.MultiRowText == message);
        }

        [TestMethod]
        public void MessabeBarVM_AddRow_WillAddText_IfPreviouslyNotEmpty()
        {
            MessageBarViewModel vm = new MessageBarViewModel();
            string prevMessage = "Previous message.";
            vm.AddRow(prevMessage);

            string message = "Any new message.";
            vm.AddRow(message);

            Assert.IsTrue(vm.MultiRowText == string.Format("{0}\n{1}", prevMessage, message));
        }

        [TestMethod]
        public void MessabeBarVM_IsNotShown_Initially()
        {
            MessageBarViewModel vm = new MessageBarViewModel();

            Assert.IsFalse(vm.IsShown);
        }

        [TestMethod]
        public void MessabeBarVM_IsShown_AfterAddingAnyRow()
        {
            MessageBarViewModel vm = new MessageBarViewModel();

            vm.AddRow("dummy message");

            Assert.IsTrue(vm.IsShown);
        }

        [TestMethod]
        public void MessabeBarVM_AddRow_WillNotifyAboutTextChange()
        {
            FakeMessageBarViewModel fakeVm = new FakeMessageBarViewModel();

            fakeVm.AddRow("dummy message");

            Assert.IsTrue(fakeVm.PropertiesChanged.Contains("MultiRowText"));
        }

        [TestMethod]
        public void MessabeBarVM_IsNotShown_AfterTimerTick()
        {
            FakeMessageBarViewModel fakeVm = new FakeMessageBarViewModel();
            fakeVm.AddRow("dummy message");

            fakeVm.Forward_TimerTickSeam();

            Assert.IsFalse(fakeVm.IsShown);
        }

        [TestMethod]
        public void MessabeBarVM_HidingWillNotifyAboutIsShownChange()
        {
            FakeMessageBarViewModel fakeVm = new FakeMessageBarViewModel();
            fakeVm.AddRow("dummy message");

            fakeVm.PropertiesChanged.Clear();
            fakeVm.Forward_TimerTickSeam();

            Assert.IsTrue(fakeVm.PropertiesChanged.Contains("IsShown"));
        }

        [TestMethod]
        public void MessabeBarVM_RunningTimerIsReset_AfterAddingAnyNewRow()
        {
            FakeMessageBarViewModel fakeVm = new FakeMessageBarViewModel();

            fakeVm.AddRow("dummy message");

            Assert.IsTrue(fakeVm.TimerWasReset);
        }

        [TestMethod]
        public void MessabeBarVM_AddRow_WillNotifyAboutTextAndIsShownChange()
        {
            FakeMessageBarViewModel fakeVm = new FakeMessageBarViewModel();

            fakeVm.AddRow("dummy message");

            Assert.IsTrue(fakeVm.PropertiesChanged.Contains("MultiRowText"));
            Assert.IsTrue(fakeVm.PropertiesChanged.Contains("IsShown"));
        }
    }

    internal class FakeMessageBarViewModel : MessageBarViewModel
    {
        public FakeMessageBarViewModel()
        {
            this.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(FakeMessageBarViewModel_PropertyChanged);
        }

        public List<string> PropertiesChanged = new List<string>();
        void FakeMessageBarViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PropertiesChanged.Add(e.PropertyName);
        }

        public bool TimerWasReset;
        protected override void resetTimerSeam()
        {
            TimerWasReset = true;
        }

        public void Forward_TimerTickSeam()
        {
            base.timerTickSeam();
        }
    }
}
