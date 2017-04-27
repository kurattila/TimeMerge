using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace TimeMerge.Utils
{
    public interface IAppRestarter
    {
        void Init(ISingleAppInstanceGuard singleAppInstanceGuard);
        void ShutDown();
        void ShutDownAndRestart(string startupArg);
    }

    class AppRestarter : IAppRestarter
    {
        static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(AppRestarter));

        ISingleAppInstanceGuard m_SingleAppInstanceGuard;
        public void Init(ISingleAppInstanceGuard singleAppInstanceGuard)
        {
            logger.Info("Init()");
            m_SingleAppInstanceGuard = singleAppInstanceGuard;
        }

        public void ShutDown()
        {
            logger.Info("ShutDown()");
            m_SingleAppInstanceGuard.OnBeforeAppShutDown();
            appShutdown();
        }

        public void ShutDownAndRestart(string startupArg)
        {
            logger.Info("ShutDownAndRestart()");
            ShutDown();
            appStart(startupArg + " " + App.NightlyRestart);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void appStart(string startParam)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, startParam);
        }

        [ExcludeFromCodeCoverage]
        protected virtual void appShutdown()
        {
            Application.Current.Shutdown();
        }
    }
}
