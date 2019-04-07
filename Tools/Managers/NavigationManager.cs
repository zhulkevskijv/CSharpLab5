using System;
using Lab5.Tools.Navigation;

namespace Lab5.Tools.Managers
{
    class NavigationManager
    {

        private static readonly object Locker = new object();
        private static NavigationManager _instance;
        
        internal static NavigationManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                lock (Locker)
                {
                    return _instance ?? (_instance = new NavigationManager());
                }
            }
        }

        private INavigationModel _navigationModel;

        private NavigationManager()
        {

        }

        internal void Initialize(InitializationNavigationModel navigationModel)
        {
            _navigationModel = navigationModel;
        }

        internal void Navigate(ViewType viewType)
        {
            _navigationModel.Navigate(viewType);
            NavigationPerformed?.Invoke(this, null);
        }

        public event EventHandler NavigationPerformed;


    }
}
