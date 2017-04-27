using System;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

namespace TimeMerge.Utils
{
    /// <summary>
    /// Class implementing support for "minimize to tray" functionality.
    /// 
    /// http://blogs.msdn.com/b/delay/archive/2009/08/31/get-out-of-the-way-with-the-tray-minimize-to-tray-sample-implementation-for-wpf.aspx
    /// </summary>
    public static class MinimizeToTray
    {
        /// <summary>
        /// Enables "minimize to tray" behavior for the specified Window.
        /// </summary>
        /// <param name="window">Window to enable the behavior for.</param>
        public static void Enable(Window window)
        {
            // No need to track this instance; its event handlers will keep it alive
            new MinimizeToTrayInstance(window);
        }

        public static void MinimizeWindow(Window window)
        {
            window.Hide();
        }

        public static void RestoreWindow(Window window)
        {
            if (window == null)
                return;

            if (window.IsLoaded)
            {
                window.Show();
                window.Activate();
            }
        }

//         static readonly int GWL_EXSTYLE = -20;
//         static readonly int WS_EX_TOOLWINDOW = 0x00000080;
// 
//         [DllImport("user32.dll")]
//         public static extern int SetWindowLong(IntPtr window, int index, int value);
// 
//         [DllImport("user32.dll")]
//         public static extern int GetWindowLong(IntPtr window, int index);
//         public static void RemoveFromSystemAltTab(Window window)
//         {
//             //Make it gone from the ALT+TAB
//             var helper = new WindowInteropHelper(this);
//             int windowStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
//             SetWindowLong(helper.Handle, GWL_EXSTYLE, windowStyle | WS_EX_TOOLWINDOW);
//         }

        /// <summary>
        /// Class implementing "minimize to tray" functionality for a Window instance.
        /// </summary>
        private class MinimizeToTrayInstance
        {
            private Window _window;
            private NotifyIcon _notifyIcon;

            private NotifyIcon NotifyIcon
            {
                get
                {
                    if (_notifyIcon == null)
                    {
                        // Initialize "on demand"
                        _notifyIcon = new NotifyIcon();
                        _notifyIcon.Icon = Properties.Resources.TimeMerge;
                        _notifyIcon.BalloonTipText = "TimeMerge";
                        _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
                        _notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
                    }
                    return _notifyIcon;
                }
            }

            /// <summary>
            /// Initializes a new instance of the MinimizeToTrayInstance class.
            /// </summary>
            /// <param name="window">Window instance to attach to.</param>
            public MinimizeToTrayInstance(Window window)
            {
                Debug.Assert(window != null, "window parameter is null.");
                _window = window;
                _window.IsVisibleChanged += HandleStateChanged;
                //                 _window.StateChanged += new EventHandler(HandleStateChanged);

                (_window as MainWindow).RequestHomeOfficeStartedTrayBalloonNotification += MinimizeToTrayInstance_RequestHomeOfficeStartedTrayBalloonNotification;

                // http://stackoverflow.com/questions/1067844/issue-with-notifyicon-not-dissappearing-on-winforms-app/3835780#3835780
                _window.Closed += (s, e) =>
                {
                    NotifyIcon.Visible = false;
                    NotifyIcon.Dispose();
                    _notifyIcon = null;
                };
            }

            private void MinimizeToTrayInstance_RequestHomeOfficeStartedTrayBalloonNotification(object sender, string balloonText)
            {
                NotifyIcon.BalloonTipText = balloonText;
                NotifyIcon.Visible = true;
                NotifyIcon.ShowBalloonTip(1000);
            }

            /// <summary>
            /// Handles the Window's StateChanged event.
            /// </summary>
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleStateChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                // Show/hide Window and NotifyIcon
                var minimized = !_window.IsVisible; // TimeMerge window minimized?
                _window.ShowInTaskbar = !minimized;
                if (minimized)
                {
                    // Show the user what happened every time when user minimizes app to the tray
                    NotifyIcon.BalloonTipText = _window.Title; // Update copy of Window Title in case it has changed
                    NotifyIcon.Visible = true;
                    NotifyIcon.ShowBalloonTip(1000);
                }
            }

            /// <summary>
            /// Handles a click on the notify icon or its balloon.
            /// </summary>
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
            {
                // Restore the Window
                MinimizeToTray.RestoreWindow(_window);
            }

        }
    }
}
