using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TimeMerge.Utils
{
    public class DebugConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        #endregion
    }


    public class VisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolean = (bool)value;
            string paramString = (string) parameter;
            if (!string.IsNullOrEmpty(paramString) && paramString.ToLower() == "not")
                boolean = !boolean;
            return boolean ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class VisibilityMultiConverter : IMultiValueConverter
    {
        public enum CombiningLogic
        {
            AnyOfBindingsIsTrue,
            AllTrue
        }

        public CombiningLogic BindingsCombiningLogic { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool resultingBoolean = false;
            bool runningAllTrueBoolean = true;
            foreach(object oneValue in values)
            {
                if (oneValue == DependencyProperty.UnsetValue)
                {
                    if (this.BindingsCombiningLogic == CombiningLogic.AllTrue)
                        runningAllTrueBoolean = false;
                    break;
                }

                bool oneCondition = (bool)oneValue;

                if (this.BindingsCombiningLogic == CombiningLogic.AnyOfBindingsIsTrue && oneCondition == true)
                {
                    resultingBoolean = true;
                    break;
                }

                runningAllTrueBoolean = runningAllTrueBoolean && oneCondition;
                if (this.BindingsCombiningLogic == CombiningLogic.AllTrue && runningAllTrueBoolean == false)
                    break;
            }

            if (this.BindingsCombiningLogic == CombiningLogic.AllTrue)
            {
                resultingBoolean = runningAllTrueBoolean;
            }

            return resultingBoolean ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InterruptTypeToImageBrushConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var interruptionType = (TimeMerge.Model.WorkInterruption.WorkInterruptionType)value;
            string imageUri = "";
            switch (interruptionType)
            {
                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.OBED:
                    imageUri = "pack://application:,,,/Images/Meal.png";
                    break;
                
                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.PDOMA:
                    imageUri = "pack://application:,,,/Images/Pickaxe.png";
                    break;

                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.DOV:
                    imageUri = "pack://application:,,,/Images/Palm.png";
                    break;

                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.LEK:
                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.OCR:
                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.PN:
                    imageUri = "pack://application:,,,/Images/Doctor.png";
                    break;

                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.SLUZ:
                    imageUri = "pack://application:,,,/Images/SuitCase.png";
                    break;

                case TimeMerge.Model.WorkInterruption.WorkInterruptionType.ZP:
                    imageUri = "pack://application:,,,/Images/Paragraph.png";
                    break;
            }
            if (string.IsNullOrEmpty(imageUri))
                return new SolidColorBrush(Colors.White);
            else
            {
                // var imageBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri(imageUri, UriKind.RelativeOrAbsolute)) };
                // imageBrush.Stretch = Stretch.Uniform;
                // imageBrush.Freeze();
                // return imageBrush;
                return createBitmapImage(imageUri);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        virtual protected BitmapImage createBitmapImage(string imageUriString)
        {
            return new BitmapImage(new Uri(imageUriString, UriKind.RelativeOrAbsolute));
        }

        #endregion
    }

    public class IsEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string textToTest = value as string;
            bool result = string.IsNullOrEmpty(textToTest);
            if (parameter != null && parameter is string && (parameter as string).ToLower() == "not")
                result = !result;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NegationConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool inputValue = (bool)value;
            return !inputValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool inputValue = (bool)value;
            return !inputValue;
        }

        #endregion
    }
}
