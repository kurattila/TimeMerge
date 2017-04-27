using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimeMerge.View
{
    /// <summary>
    /// Interaction logic for WaitWindow.xaml
    /// </summary>
    public partial class WaitWindow : Window
    {
        public WaitWindow(string messageText)
        {
            IsAllowedToClose = false;
            InitializeComponent();

            this.messageTextBlock.Text = messageText;

            this.Closing += new System.ComponentModel.CancelEventHandler(WaitWindow_Closing);
        }

        void WaitWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsAllowedToClose)
                e.Cancel = true;
        }

        public bool IsAllowedToClose;
    }
}
