using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
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
        private Thread _workingThreadList;
        private Thread _workingThreadMeta;
        private readonly CancellationToken _token;
        private readonly CancellationTokenSource _tokenSource;
        private readonly object _locker;
        #endregion 

        #region Constructor
        internal TaskManagerViewModel()
        {
            _locker = new object();
            _myProcesses = new ObservableCollection<MyProcess>();
            foreach (var process in Process.GetProcesses())
            {
                _myProcesses.Add(new MyProcess(process));
            }
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            StartWorkingThreads();
            StationManager.StopThreads += StopWorkingThreads;
            BindingOperations.EnableCollectionSynchronization(_myProcesses, _locker);
        }
        #endregion

        #region Properties

        public int IndexFilter
        {
            get => _indexFilter;
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
                OnPropertyChanged();
                _myProcesses = value;
            }
        }

        public MyProcess SelectedProcess
        {
            get => _selectedProcess;
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

        public RelayCommand<object> SortAscCommand =>
            _sortAscCommand ??
            (_sortAscCommand = new RelayCommand<object>(SortAscImplementation));

        public RelayCommand<object> SortDescCommand =>
            _sortDescCommand ??
            (_sortDescCommand = new RelayCommand<object>(SortDescImplementation));

        #endregion

        #region CommandImplementation

        public bool CanExecuteProcessCommand()
        {
            return _selectedProcess != null;
        }


        private void TerminateImplementation(object obj)
        {
            lock (_locker)
            {
                _selectedProcess.Terminate();
                _myProcesses.Remove(_selectedProcess);
                OnPropertyChanged($@"MyProcesses");
            }
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
                ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", Path.GetDirectoryName(_selectedProcess.FilePath));
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

        #region Update
        private void StartWorkingThreads()
        {
            _workingThreadList = new Thread(WorkingThreadListProcess);
            _workingThreadMeta = new Thread(WorkingThreadMetaProcess);
            _workingThreadMeta.Start();
            _workingThreadList.Start();

        }

        private void WorkingThreadMetaProcess()
        {
            Stopwatch stopwatch = new Stopwatch();
            while (!_token.IsCancellationRequested)
            {
                stopwatch.Restart();
                lock (_locker)
                {
                    foreach (var myProcess in _myProcesses)
                    {
                        myProcess.UpdateMeta();
                        if (_token.IsCancellationRequested)
                            break;
                    }
                    OnPropertyChanged($"MyProcesses");
                }

                if (stopwatch.ElapsedMilliseconds < 2000)
                {
                    Thread.Sleep(2000 - (int) stopwatch.ElapsedMilliseconds);
                    Console.WriteLine("THIS IS " + stopwatch.ElapsedMilliseconds);
                }

                if (_token.IsCancellationRequested)
                    break;
            }
        }
        private void WorkingThreadListProcess()
        {
            Stopwatch stopwatch = new Stopwatch();
            while (!_token.IsCancellationRequested)
            {
                stopwatch.Restart();
                //var tempListProcesses = (from pr in Process.GetProcesses() select new MyProcess(pr)).ToArray();
                //tempListProcesses = tempListProcesses.Except(_myProcesses).ToArray();
                List<MyProcess> listToRemove = new List<MyProcess>();
                foreach (MyProcess myProcess in _myProcesses)
                {
                    try
                    {
                        if (myProcess.ProcessOrigin.HasExited)
                        {
                            listToRemove.Add(myProcess);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                if (_token.IsCancellationRequested)
                    break;
                lock (_locker)
                {
                    foreach (MyProcess myProcess in listToRemove)
                    {
                        try
                        {
                            _myProcesses.Remove(myProcess);
                        }
                        catch (Exception e)
                        {
                            // ignored
                        }
                    }

                    //foreach (MyProcess tempListProcess in tempListProcesses)
                    //{
                        
                    //    try
                    //    {
                    //        _myProcesses.Add(tempListProcess);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        // ignored
                    //    }
                    //}
                    if (_token.IsCancellationRequested)
                        break;
                    OnPropertyChanged($"MyProcesses");
                }
                if(stopwatch.ElapsedMilliseconds < 5000)
                Thread.Sleep(5000 - (int)stopwatch.ElapsedMilliseconds);
                Console.WriteLine(stopwatch.ElapsedMilliseconds);
                if (_token.IsCancellationRequested)
                    break;
            }
        }
        private void StopWorkingThreads()
        {
            _tokenSource.Cancel();
            _workingThreadList.Join(2000);
            _workingThreadList.Abort();
            _workingThreadList = null;
            _workingThreadMeta.Join(2000);
            _workingThreadMeta.Abort();
            _workingThreadMeta = null;
        }
        #endregion
    }
}
