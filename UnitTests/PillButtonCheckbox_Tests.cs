using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls.Primitives;
using TimeMerge.Controls;

namespace UnitTests
{
    class FakePillButtonCheckbox : PillButtonCheckbox
    {
        public ToggleButton OnButton;
        public ToggleButton OffButton;
        public string ForcedContent;

        protected override object findTemplateObject(string templateObjectName)
        {
            if (templateObjectName == "PART_OnButton")
            {
                OnButton = new ToggleButton();
                return OnButton;
            }
            else
            {
                OffButton = new ToggleButton();
                return OffButton;
            }
        }

        protected override string getContent()
        {
            return ForcedContent;
        }

        public void FireLoadedEvent()
        {
            onLoadedHandler();
        }
    }

    [TestClass]
    public class PillButtonCheckbox_Tests
    {
        [TestMethod]
        public void OnApplyTemplate_WillAssignOnAndOffButtons_Always()
        {
            var pillButton = new FakePillButtonCheckbox();

            pillButton.OnApplyTemplate();

            Assert.IsTrue(pillButton.OnButton != null);
            Assert.IsTrue(pillButton.OffButton != null);
        }

        [TestMethod]
        public void OnLoaded_WillAssignOnButtonText_Always()
        {
            var pillButton = new FakePillButtonCheckbox();
            pillButton.ForcedContent = "On Text|Off Text";
            pillButton.OnApplyTemplate();

            pillButton.FireLoadedEvent();

            Assert.AreEqual("On Text", pillButton.OnButton.Content as string);
        }

        [TestMethod]
        public void OnLoaded_WillAssignOffButtonText_Always()
        {
            var pillButton = new FakePillButtonCheckbox();
            pillButton.ForcedContent = "On Text|Off Text";
            pillButton.OnApplyTemplate();

            pillButton.FireLoadedEvent();

            Assert.AreEqual("Off Text", pillButton.OffButton.Content as string);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void OnLoaded_WillThrow_WhenOnOffTextMissing()
        {
            var pillButton = new FakePillButtonCheckbox();
            pillButton.ForcedContent = null;
            pillButton.OnApplyTemplate();

            pillButton.FireLoadedEvent();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void OnLoaded_WillThrow_WhenOnOrOffTextMissing()
        {
            var pillButton = new FakePillButtonCheckbox();
            pillButton.ForcedContent = "Off Text";
            pillButton.OnApplyTemplate();

            pillButton.FireLoadedEvent();
        }
    }
}
