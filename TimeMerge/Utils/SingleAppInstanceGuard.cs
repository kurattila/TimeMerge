using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeMerge.Utils
{
    public interface ISingleAppInstanceGuard
    {
        void OnAppStartup();
        bool IsTheOnlyRunningInstance();
        void OnBeforeAppShutDown();
        void WaitForShowRunningInstanceRequest();
    }

    class SingleAppInstanceGuard : ISingleAppInstanceGuard
                                 , IDisposable
    {
        static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SingleAppInstanceGuard));

        EventWaitHandle m_ShowRunningInstanceEvent = null;
        bool m_EventWaitHandleCreatedNew; // MSDN: passed uninitialized

        public void OnAppStartup()
        {
            logger.Info("OnAppStartup()");
            m_ShowRunningInstanceEvent = new EventWaitHandle(false, EventResetMode.AutoReset, NameOfSystemWideSyncEvent, out m_EventWaitHandleCreatedNew);
            m_ShowRunningInstanceEvent.Set(); // the other instance is listening/waiting for this event => ask that instance to pop up

            if (m_EventWaitHandleCreatedNew)
                logger.Info("   EventWaitHandle created new");
            else
                logger.Info("   EventWaitHandle already existed");
        }

        public bool IsTheOnlyRunningInstance()
        {
            return m_EventWaitHandleCreatedNew;
        }

        public void OnBeforeAppShutDown()
        {
            logger.Info("OnBeforeAppShutDown()");
            if (m_EventWaitHandleCreatedNew)
            {
                m_ShowRunningInstanceEvent.Close();
                m_ShowRunningInstanceEvent.Dispose();
            }
        }

        public void WaitForShowRunningInstanceRequest()
        {
            m_ShowRunningInstanceEvent.WaitOne();
        }

        [ExcludeFromCodeCoverage]
        protected virtual string NameOfSystemWideSyncEvent
        {
            get { return "TimeMerge-ShowRunningInstanceEvent-{71455225-8162-46C8-80BE-C195A1E818D3}"; }
        }

        public void Dispose()
        {
            logger.Info("Dispose()");
            OnBeforeAppShutDown();
            m_ShowRunningInstanceEvent.Dispose();
        }
    }
}
