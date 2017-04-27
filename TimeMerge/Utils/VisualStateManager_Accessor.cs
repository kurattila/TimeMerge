using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TimeMerge.Utils
{
    /// <summary>
    /// Adapted, modified from http://tdanemar.wordpress.com/2009/11/15/using-the-visualstatemanager-with-the-model-view-viewmodel-pattern-in-wpf-or-silverlight/
    /// </summary>
    internal class VisualStateManager_Accessor : DependencyObject
    {
        public static string GetVisualStateProperty(DependencyObject obj)
        {
            return (string)obj.GetValue(VisualStatePropertyProperty);
        }

        public static void SetVisualStateProperty(DependencyObject obj, string value)
        {
            obj.SetValue(VisualStatePropertyProperty, value);
        }

        public static readonly DependencyProperty VisualStatePropertyProperty =
            DependencyProperty.RegisterAttached(
            "VisualStateProperty",
            typeof(string),
            typeof(VisualStateManager_Accessor),
            new PropertyMetadata((s, e) =>
            {
                var propertyName = (string)e.NewValue;
                var fe = s as FrameworkElement;
                if (fe == null)
                    throw new InvalidOperationException("This attached property only supports types derived from ContFrameworkElementrol.");

                // Use 'GoToState()' for setting VSM states inside a ControlTemplate
                // System.Windows.VisualStateManager.GoToState(fe, (string)e.NewValue, true);
                System.Windows.VisualStateManager.GoToElementState(fe, (string)e.NewValue, true);
            }));
    }
}
