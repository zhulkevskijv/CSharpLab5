using System.Collections.ObjectModel;
using System.Diagnostics;
using Lab5.Models;
using Lab5.Tools;
using Lab5.Tools.Managers;
using Lab5.Tools.Navigation;

namespace Lab5.ViewModels
{
    class ThreadModuleViewModel: BaseViewModel
    {
        private MyProcess _myProcess;
        private ObservableCollection<ProcessThread> _threads;
        private ObservableCollection<ProcessModule> _modules;
        private RelayCommand<object> _backCommand;

        internal ThreadModuleViewModel()
        {
            _myProcess = StationManager.CurrentProcess;
            _threads = new ObservableCollection<ProcessThread>();
            _modules = new ObservableCollection<ProcessModule>();
            foreach (ProcessThread processOriginThread in _myProcess.ProcessOrigin.Threads)
            {
                _threads.Add(processOriginThread);
            }

            foreach (ProcessModule processOriginModule in _myProcess.ProcessOrigin.Modules)
            {
                _modules.Add(processOriginModule);
            }
        }

        public RelayCommand<object> BackCommand
        {
            get {
                return _backCommand ??(_backCommand = new RelayCommand<object>(BackImplementation));
            }
        }
        public ObservableCollection<ProcessThread> Threads
        {
            get { return _threads; }
        }

        public ObservableCollection<ProcessModule> Modules
        {
            get { return _modules; }
        }

        private void BackImplementation(object obj)
        {
            NavigationManager.Instance.Navigate(ViewType.TaskManager);
        }
    }
}
