using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TimeMerge.Controls
{
    internal class TimeMergeGauge : RangeBase
    {
        static TimeMergeGauge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeMergeGauge), new FrameworkPropertyMetadata(typeof(TimeMergeGauge)));
        }

        public TimeMergeGauge()
        {
            this.Minimum = new TimeSpan(-30*8, 0, 0).Ticks;
            this.Maximum = new TimeSpan(30*8, 0, 0).Ticks;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._valueTextBlock = this.Template.FindName("valueTextBlock", this) as TextBlock;
            this._zeroBalanceReachedTextBlock = this.Template.FindName("zeroBalanceReachedTextBlock", this) as TextBlock;
            this._valuePointerRotateTransform = this.Template.FindName("valuePointerRotateTransform", this) as RotateTransform;

            this.OnValueChanged(0, 0); // init display text to "+0:00"
        }



        public string ZeroBalanceReachedTime
        {
            get { return (string)GetValue(ZeroBalanceReachedTimeProperty); }
            set { SetValue(ZeroBalanceReachedTimeProperty, value); }
        }
        public static readonly DependencyProperty ZeroBalanceReachedTimeProperty =
            DependencyProperty.Register("ZeroBalanceReachedTime", typeof(string), typeof(TimeMergeGauge), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnZeroBalanceReachedTimeChanged)));




        public string ZeroBalanceReachedTooltip
        {
            get { return (string)GetValue(ZeroBalanceReachedTooltipProperty); }
            set { SetValue(ZeroBalanceReachedTooltipProperty, value); }
        }
        public static readonly DependencyProperty ZeroBalanceReachedTooltipProperty =
            DependencyProperty.Register("ZeroBalanceReachedTooltip", typeof(string), typeof(TimeMergeGauge), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnZeroBalanceReachedTooltipChanged)));


        private static void OnZeroBalanceReachedTimeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TimeMergeGauge thisObject = sender as TimeMergeGauge;
            if (thisObject != null)
            {
                var newZeroBalanceText = args.NewValue as string;
                thisObject._zeroBalanceReachedTextBlock.Visibility = string.IsNullOrEmpty(newZeroBalanceText) ? Visibility.Hidden : Visibility.Visible;
                thisObject._zeroBalanceReachedTextBlock.Text = newZeroBalanceText;
            }
        }

        private static void OnZeroBalanceReachedTooltipChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TimeMergeGauge thisObject = sender as TimeMergeGauge;
            if (thisObject != null)
            {
                thisObject._zeroBalanceReachedTextBlock.ToolTip = args.NewValue as string;
            }
        }
        

        private static TimeSpan PointerMaxTimeSpan = new TimeSpan(5, 0, 0);
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            if (this._valueTextBlock != null)
            {
                TimeSpan timespan = new TimeSpan((long)this.Value);
                this._valueTextBlock.Text = TimeMerge.Utils.Calculations.MonthBalanceAsHumanString(timespan);
                this._valueTextBlock.Foreground = this.Value < 0 ? Brushes.DarkRed : Brushes.DarkGreen;
            }

            if (this._valuePointerRotateTransform != null)
            {
                TimeSpan timespan = new TimeSpan((long)this.Value);
                double limitedAbsTimeSpan = Math.Min(Math.Abs(timespan.Ticks), PointerMaxTimeSpan.Ticks);
                double normalizedTimeSpan = 0; // value between -1.0 and +1.0
                if (timespan.Ticks >= 0)
                {
                    normalizedTimeSpan = limitedAbsTimeSpan / PointerMaxTimeSpan.Ticks;
                }
                else
                {
                    normalizedTimeSpan = -1 * limitedAbsTimeSpan / PointerMaxTimeSpan.Ticks;
                }
                double pointerRotation = normalizedTimeSpan * 125;

                // Animate pointer to the new Angle value
                DoubleAnimation anim = new DoubleAnimation(pointerRotation, TimeSpan.FromMilliseconds(1000));
                anim.EasingFunction = new ElasticEase() { EasingMode = EasingMode.EaseInOut, Oscillations = 1 };
                this._valuePointerRotateTransform.BeginAnimation(RotateTransform.AngleProperty, anim, HandoffBehavior.SnapshotAndReplace);
            }
        }

        private TextBlock _valueTextBlock;
        private TextBlock _zeroBalanceReachedTextBlock;
        private RotateTransform _valuePointerRotateTransform;
    }
}
