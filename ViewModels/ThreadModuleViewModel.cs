using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
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
            
            NavigationManager.Instance.NavigationPerformed += Instance_NavigationPerformed; ;
            
        }

        private void Instance_NavigationPerformed(object sender, EventArgs e)
        {
            _myProcess = StationManager.CurrentProcess;
            _threads = new ObservableCollection<ProcessThread>();
            _modules = new ObservableCollection<ProcessModule>();
            try
            {
                foreach (ProcessThread processOriginThread in _myProcess.ProcessOrigin.Threads)
                {
                    _threads.Add(processOriginThread);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Threads aren't available!");
            }

            try
            {
                foreach (ProcessModule processOriginModule in _myProcess.ProcessOrigin.Modules)
                {
                    _modules.Add(processOriginModule);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Modules aren't available!");
            }
            OnPropertyChanged($"Threads");
            OnPropertyChanged($"Modules");
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
            StationManager.CurrentProcess = _myProcess;
            NavigationManager.Instance.Navigate(ViewType.TaskManager);
        }
    }
}
