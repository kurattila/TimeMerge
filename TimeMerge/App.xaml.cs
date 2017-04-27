using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using TimeMerge.Utils;

namespace TimeMerge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        public static void Main()
        {
            App.SplashScreenCreatedEvent = new ManualResetEvent(false);

            // Show the splash screen on a background thread, with a separate WPF dispatcher running on it.
            // This way, that splash screen can be even animated, since it runs on a separate GUI thread.
            Thread splashScreenThread = new Thread(SplashScreenThreadStartingPoint);
            splashScreenThread.SetApartmentState(ApartmentState.STA);
            splashScreenThread.IsBackground = true;
            splashScreenThread.Start();

            // The app must not try to close the splash screen until it is fully created by the background thread
            App.SplashScreenCreatedEvent.WaitOne();

            log4net.Config.XmlConfigurator.Configure();

            // Classic way of starting up our application:
            TimeMerge.App app = new TimeMerge.App();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.DispatcherUnhandledException += app_DispatcherUnhandledException;
            app.InitializeComponent();
            app.Run();
        }

        static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(App));
        static void app_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Fatal(e.Exception.Message);
            logger.Fatal(e.Exception.StackTrace);
        }

        private static ManualResetEvent SplashScreenCreatedEvent;
        private static TimeMerge.View.SplashScreen AppSplashScreen;
        private static void SplashScreenThreadStartingPoint()
        {
            App.AppSplashScreen = new TimeMerge.View.SplashScreen();
            App.AppSplashScreen.Show();
            App.SplashScreenCreatedEvent.Set();

            Dispatcher.Run();
        }

        public void CloseSplashScreen()
        {
            if (App.AppSplashScreen != null)
            {
                Action shutdownDispatcherAction = new Action(() =>
                    {
                        App.AppSplashScreen.Dispatcher.InvokeShutdown();
                        App.AppSplashScreen = null;
                    });

                // Access the splash screen on the thread which has created it.
                // When the closing animation completes, also shutdown that thread's Dispatcher.
                App.AppSplashScreen.Dispatcher.Invoke(new Action(() => App.AppSplashScreen.AnimatedClose(shutdownDispatcherAction)));
            }
        }

        public void EnterBusyState(string waitReasonText, TimeMerge.ViewModel.MessageBarViewModel messageBarVM)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new Action<string, TimeMerge.ViewModel.MessageBarViewModel>((text, viewModel) => { this.EnterBusyState(text, viewModel); }), waitReasonText, messageBarVM);
                return;
            }

            ExitBusyState(); // prevent any overlapping (such states would be inconsistent)

            if (this.MainWindow != null) // prevent crashes during app restarts
            {
                if (this.MainWindow.IsLoaded)
                {
                    messageBarVM.AddRow(waitReasonText);
                }
                this.MainWindow.IsEnabled = false;
            }
        }

        public void ExitBusyState()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new Action(() => { this.ExitBusyState(); }));
                return;
            }

            if (this.MainWindow != null) // prevent crashes during app restarts
            {
                this.MainWindow.IsEnabled = true;
                var timeMergeMainWindow = this.MainWindow as TimeMerge.MainWindow;
                if (timeMergeMainWindow != null)
                    timeMergeMainWindow.FocusToDataGrid();
            }
        }

        public static readonly string MinimizedStartupArgString = "/minimized";
        public static readonly string ShutdownAfterFirstWebRequestArgString = "/once";
        public static readonly string NightlyRestart = "/restarted";

        internal StartupContext Context = new StartupContext();
        internal class StartupContext
        {
            public StartupContext()
            {
                MinimizedStartupRequested = false;
                WebRequestObserver = new NoActionOnWebRequest();
                SingleAppInstancePolicy = new DefaultSingleAppInstancePolicy();
                WebDavConnection = new WebDavConnection();
            }

            public bool MinimizedStartupRequested { get; set; }
            public IWebRequestObserver WebRequestObserver { get; set; }
            public ISingleAppInstancePolicy SingleAppInstancePolicy { get; set; }
            public IWebDavConnection WebDavConnection { get; set; }
        }

        private Utils.SingleAppInstanceGuard m_SingleAppInstanceGuard = new Utils.SingleAppInstanceGuard();
        private Utils.AppRestarter m_AppRestarter = new Utils.AppRestarter();

        private void monitorShowRunningInstanceRequests()
        {
            while (true)
            {
                m_SingleAppInstanceGuard.WaitForShowRunningInstanceRequest();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var timeMergeMainWindow = this.MainWindow as TimeMerge.MainWindow;
                    if (timeMergeMainWindow != null)
                        timeMergeMainWindow.RequestRestoreWindow();
                });
            }
        }

        public Utils.IAppRestarter GetAppRestarter()
        {
            return m_AppRestarter;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            logger.Info("Startup");

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("sk");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("sk");

            m_AppRestarter.Init(m_SingleAppInstanceGuard);

            if (e.Args.Length > 0)
            {
                foreach (var arg in e.Args)
                {
                    if (arg.ToLower() == TimeMerge.App.MinimizedStartupArgString)
                        Context.MinimizedStartupRequested = true;
                    else if (arg.ToLower() == TimeMerge.App.ShutdownAfterFirstWebRequestArgString)
                        Context.WebRequestObserver = new AppShutdownOnWebRequest(m_AppRestarter);
                    else if (arg.ToLower() == TimeMerge.App.NightlyRestart)
                        Context.SingleAppInstancePolicy = new RestartedAppSingleInstancePolicy();
                }
            }

            Context.WebDavConnection.Init(
                  TimeMerge.Properties.Settings.Default.UserID
                , TimeMerge.Properties.Settings.Default.WebDavGenericLogin
                , readFileContents(TimeMerge.Properties.Settings.Default.WebDavGenericHashPath)
                , TimeMerge.Properties.Settings.Default.UrlWebDav);

            m_SingleAppInstanceGuard.OnAppStartup();
            if (Context.SingleAppInstancePolicy.IsAppStartAllowed(m_SingleAppInstanceGuard) == false)
            {
                m_AppRestarter.ShutDown();
                return;
            }

            Task.Run(new Action(() => { monitorShowRunningInstanceRequests(); }));

            // Ensure that settings inside 'user.config' are not lost just because a new version of TimeMerge has been deployed
            if (TimeMerge.Properties.Settings.Default.CallUpgrade)
            {
                TimeMerge.Properties.Settings.Default.Upgrade();
                TimeMerge.Properties.Settings.Default.CallUpgrade = false;
            }
        }

        private string readFileContents(string webDavGenericHashPath)
        {
            string fileContents = "";
            try
            {
                fileContents = System.IO.File.ReadAllText(webDavGenericHashPath);
            }
            catch(System.IO.IOException)
            {
                // Nothing critical if e.g. file does not exist or \\slovensko2 is down at the moment,
                // it's just that WebDav's XML file won't get uploaded
            }
            return fileContents;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            logger.Info(string.Format("Memory usage: {0} MB", process.PrivateMemorySize64 / (1024 * 1024)));
            logger.Info("Exit");
        }
    }
}
