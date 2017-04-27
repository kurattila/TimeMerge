using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.Utils;

namespace UnitTests
{
    class Fake_InterruptTypeToImageBrushConverter : InterruptTypeToImageBrushConverter
    {
        public String RequestedCreateBitmapImage = null;
        protected override System.Windows.Media.Imaging.BitmapImage createBitmapImage(string imageUriString)
        {
            RequestedCreateBitmapImage = imageUriString;
            return null; // do _not_ actually create any bitmap, no _not_ actually access any resources
        }
    }

    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void InterruptTypeToImageBrushConverter_ReturnsMedicalImage_ForPN()
        {
            Fake_InterruptTypeToImageBrushConverter spyConverter = new Fake_InterruptTypeToImageBrushConverter();

            spyConverter.Convert(TimeMerge.Model.WorkInterruption.WorkInterruptionType.PN, null, null, null);

            Assert.AreEqual("pack://application:,,,/Images/Doctor.png", spyConverter.RequestedCreateBitmapImage);
        }
    }
}
