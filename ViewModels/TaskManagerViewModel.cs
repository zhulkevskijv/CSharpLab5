using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Lab5.Models;
using Lab5.Tools;
using Lab5.Tools.Managers;
using Lab5.Tools.Navigation;


namespace Lab5.ViewModels
{
    class TaskManagerViewModel : BaseViewModel
    {
        #region Fields
        private ObservableCollection<MyProcess> _myProcesses;
        private MyProcess _selectedProcess;
        private int _indexFilter;
        private RelayCommand<object> _terminateCommand;
        private RelayCommand<object> _sortAscCommand;
        private RelayCommand<object> _sortDescCommand;
        private RelayCommand<object> _showModulesThreadsCommand;
        private RelayCommand<object> _openDirCommand;
        #endregion 

        #region Constructor
        internal TaskManagerViewModel()
        {
            _myProcesses = new ObservableCollection<MyProcess>();
            foreach (var process in Process.GetProcesses())
            {
                _myProcesses.Add(new MyProcess(process));
            }
        }
        #endregion

        #region Properties

        public int IndexFilter
        {
            get { return _indexFilter; }
            set
            {
                _indexFilter = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MyProcess> MyProcesses
        {
            get => _myProcesses;
            private set
            {
                _myProcesses = value;
                OnPropertyChanged();
            }
        }

        public MyProcess SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
                OnPropertyChanged();
                _selectedProcess = value;
            }
        }
        #endregion

        #region Command Properties

        public RelayCommand<object> TerminateCommand
        {
            get
            {
                return _terminateCommand ??
                       (_terminateCommand = new RelayCommand<object>(TerminateImplementation, o => CanExecuteProcessCommand()));
            }
        }
        public RelayCommand<object> ShowModulesThreadsCommand
        {
            get
            {
                return _showModulesThreadsCommand ??
                       (_showModulesThreadsCommand = new RelayCommand<object>(ShowModulesThreadsImplementation, o => CanExecuteProcessCommand()));
            }
        }

        public RelayCommand<object> OpenDirCommand
        {
            get
            {
                return _openDirCommand ??
                       (_openDirCommand = new RelayCommand<object>(OpenDirImplementation, o => CanExecuteProcessCommand()));
            }
        }

        public RelayCommand<object> SortAscCommand
        {
            get
            {
                return _sortAscCommand ??
                       (_sortAscCommand = new RelayCommand<object>(SortAscImplementation));
            }
        }

        public RelayCommand<object> SortDescCommand
        {
            get
            {
                return _sortDescCommand ??
                       (_sortDescCommand = new RelayCommand<object>(SortDescImplementation));
            }
        }

        #endregion

        #region CommandImplementation

        public bool CanExecuteProcessCommand()
        {
            return _selectedProcess != null;
        }


        private void TerminateImplementation(object obj)
        {
            _selectedProcess.Terminate();
            _myProcesses.Remove(_selectedProcess);
        }

        private void ShowModulesThreadsImplementation(object obj)
        {
            StationManager.CurrentProcess = _selectedProcess;
            NavigationManager.Instance.Navigate(ViewType.ThreadsModules);
        }

        private void OpenDirImplementation(object obj)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo($"explorer.exe", Path.GetDirectoryName(_selectedProcess.FilePath));
                Process.Start(startInfo);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show($"{Path.GetDirectoryName(_selectedProcess.FilePath)} directory does not exist!");
            }
        }

        private void SortAscImplementation(object obj)
        {
            switch (IndexFilter)
            {
                case (0):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.Name));
                    break;
                case (1):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.Id));
                    break;
                case (2):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.Cpu));
                    break;
                case (3):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.RamPercent));
                    break;
                case (4):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.Ram));
                    break;
                case (5):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.ThreadNumber));
                    break;
                case (6):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.FilePath));
                    break;
                case (7):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderBy(i => i.StartTime));
                    break;
            }
            OnPropertyChanged($"MyProcesses");
        }

        private void SortDescImplementation(object obj)
        {
            switch (IndexFilter)
            {
                case (0):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.Name));
                    break;
                case (1):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.Id));
                    break;
                case (2):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.Cpu));
                    break;
                case (3):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.RamPercent));
                    break;
                case (4):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.Ram));
                    break;
                case (5):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.ThreadNumber));
                    break;
                case (6):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.FilePath));
                    break;
                case (7):
                    _myProcesses = new ObservableCollection<MyProcess>(_myProcesses.OrderByDescending(i => i.StartTime));
                    break;
            }
            OnPropertyChanged($"MyProcesses");
        }

        #endregion
    }
}
