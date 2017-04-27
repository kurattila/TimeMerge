using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeMerge.Utils
{
    interface ISingleAppInstancePolicy
    {
        bool IsAppStartAllowed(SingleAppInstanceGuard singleAppInstanceGuard);
    }

    class DefaultSingleAppInstancePolicy : ISingleAppInstancePolicy
    {
        static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(DefaultSingleAppInstancePolicy));

        public bool IsAppStartAllowed(SingleAppInstanceGuard singleAppInstanceGuard)
        {
            // Deny startup of this app instance if there's another TimeMerge running somewhere.
            bool isAppStartAllowed = singleAppInstanceGuard.IsTheOnlyRunningInstance();
            logger.Info(string.Format("IsAppStartAllowed = {0}", isAppStartAllowed));
            return isAppStartAllowed;
        }
    }

    class RestartedAppSingleInstancePolicy : ISingleAppInstancePolicy
    {
        static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(RestartedAppSingleInstancePolicy));

        public bool IsAppStartAllowed(SingleAppInstanceGuard singleAppInstanceGuard)
        {
            // On restart, there's no need to check if we're the only app running.
            // Skipping the check actually fixes problems with not properly releasing
            // the EventWaitHandle of SingleAppInstanceGuard by Windows (and finding TimeMerge
            // not running in the morning).
            logger.Info("IsAppStartAllowed = true");
            return true;
        }
    }
}
