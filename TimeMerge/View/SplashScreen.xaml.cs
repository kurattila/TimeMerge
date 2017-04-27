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
using System.Windows.Media.Animation;

namespace TimeMerge.View
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        public void AnimatedClose(Action onCompletedAction)
        {
            var closeStoryboard = this.Resources["animatedCloseStoryboard"] as Storyboard;
            Storyboard.SetTarget(closeStoryboard, this);

            if (onCompletedAction != null)
                closeStoryboard.Completed += (sender, args) => closeAndFireOnCompletedAction(onCompletedAction);

            closeStoryboard.Begin();
        }

        private void closeAndFireOnCompletedAction(Action onCompletedAction)
        {
            this.Close();
            onCompletedAction();
        }
    }
}
