
namespace TimeMerge.Utils
{
    public interface IWebRequestObserver
    {
        void OnWebRequestCompleted();
    }

    // NULL object pattern
    class NoActionOnWebRequest : IWebRequestObserver
    {
        public void OnWebRequestCompleted()
        { }
    }

    // Shuts application down
    class AppShutdownOnWebRequest : IWebRequestObserver
    {
        private IAppRestarter m_AppRestarter;

        public AppShutdownOnWebRequest(IAppRestarter appRestarter)
        {
            m_AppRestarter = appRestarter;
        }

        public void OnWebRequestCompleted()
        {
            m_AppRestarter.ShutDown();
        }
    }
}
