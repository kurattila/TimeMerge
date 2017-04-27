using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;

namespace TimeMerge.Controls
{
    public class ImageButton : Button
    {
        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
        }

        public ImageButton()
        {
            this.MouseEnter += new System.Windows.Input.MouseEventHandler(ImageButton_MouseEnter);
            this.MouseLeave += new System.Windows.Input.MouseEventHandler(ImageButton_MouseLeave);
        }

        private ContentPresenter _hoverContentPresenter;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._hoverContentPresenter = this.Template.FindName("PART_HoverContent", this) as ContentPresenter;
        }

        void ImageButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
            this._hoverContentPresenter.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        void ImageButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation(0.0, TimeSpan.FromMilliseconds(200));
            this._hoverContentPresenter.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        public object HoverContent
        {
            get { return (object)GetValue(HoverContentProperty); }
            set { SetValue(HoverContentProperty, value); }
        }
        public static readonly DependencyProperty HoverContentProperty =
            DependencyProperty.Register("HoverContent", typeof(object), typeof(ImageButton));
    }
}
