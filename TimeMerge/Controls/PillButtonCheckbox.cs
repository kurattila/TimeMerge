using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TimeMerge.Controls
{
    internal class PillButtonCheckbox : CheckBox
    {
        static PillButtonCheckbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PillButtonCheckbox), new FrameworkPropertyMetadata(typeof(PillButtonCheckbox)));
        }

        public PillButtonCheckbox()
        {
            this.Loaded += (sender, args) => onLoadedHandler();
        }

        void setOnOffButtonTexts()
        {
            string onOffString = getContent();
            if (string.IsNullOrEmpty(onOffString))
                throw new ArgumentException("Content shall be a pipe-separated string, e.g. 'OnValue|OffValue'", "Content");

            string[] onOffValues = onOffString.Split('|');
            if (onOffValues.Length < 2)
                throw new ArgumentException("Content shall be a pipe-separated string, e.g. 'OnValue|OffValue'", "Content");

            setContent(_onButton, onOffValues[0]);
            setContent(_offButton, onOffValues[1]);
        }

        ToggleButton _onButton;
        ToggleButton _offButton;
        public override void OnApplyTemplate()
        {
            _onButton = findTemplateObject("PART_OnButton") as ToggleButton;
            _offButton = findTemplateObject("PART_OffButton") as ToggleButton;

            base.OnApplyTemplate();
        }

        protected virtual void onLoadedHandler()
        {
            setOnOffButtonTexts();
        }

        protected virtual object findTemplateObject(string templateObjectName)
        {
            return Template.FindName(templateObjectName, this);
        }

        protected virtual string getContent()
        {
            return this.Content as string;
        }

        protected virtual void setContent(ContentControl control, string newContentString)
        {
            control.Content = newContentString;
        }
    }
}
