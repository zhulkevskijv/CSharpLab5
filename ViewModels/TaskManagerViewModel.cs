using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Data;
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
            BindingOperations.EnableCollectionSynchronization(_myProcesses, _locker);
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            StartWorkingThreads();
            StationManager.StopThreads += StopWorkingThreads;

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
                //ObservableCollection<MyProcess> toUpdate = new ObservableCollection<MyProcess>(_myProcesses);
                List<MyProcess> kek = new List<MyProcess>(_myProcesses);
                foreach (var myProcess in kek)
                {
                    myProcess.UpdateMeta();
                    if (_token.IsCancellationRequested)
                        break;
                }
                OnPropertyChanged($"MyProcesses");
                Console.WriteLine($@"THIS IS {stopwatch.ElapsedMilliseconds}");
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
                List<MyProcess> listToRemove = new List<MyProcess>();
                List<MyProcess> tempListProcesses = new List<MyProcess>();
                foreach (Process process in Process.GetProcesses())
                {
                    bool found = false;
                    foreach (MyProcess myProcess in _myProcesses)
                    {
                        if (myProcess.Id == process.Id)
                            found = true;

                    }
                    if (!found)
                        tempListProcesses.Add(new MyProcess(process));
                }
                if (_token.IsCancellationRequested)
                    break;
                foreach (MyProcess myProcess in _myProcesses)
                {
                    bool found = false;
                    foreach (Process process in Process.GetProcesses())
                    {
                        if (myProcess.Id == process.Id)
                            found = true;
                    }
                    if (!found)
                        listToRemove.Add(myProcess);
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
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    foreach (MyProcess tempListProcess in tempListProcesses)
                    {
                        try
                        {
                            _myProcesses.Add(tempListProcess);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    if (_token.IsCancellationRequested)
                        break;
                    OnPropertyChanged($"MyProcesses");
                }
                if (stopwatch.ElapsedMilliseconds < 5000)
                    Thread.Sleep(5000 - (int)stopwatch.ElapsedMilliseconds);
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
