using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using Lab5.Properties;

namespace Lab5.Models
{
    class MyProcess : INotifyPropertyChanged
    {
        #region Fields

        private double _cpu;
        private double _ramPercent;
        private long _ram;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        private bool _ok = true;
        private static readonly long WholeRam;

        #endregion

        #region Constructor

        static MyProcess()
        {
            string Query = "SELECT Capacity FROM Win32_PhysicalMemory";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
            UInt64 capacity = 0;
            foreach (var wniPart in searcher.Get())
            {
                capacity += Convert.ToUInt64(wniPart.Properties["Capacity"].Value);
            }
            WholeRam = (long)capacity / 1024;
        }

        internal MyProcess(Process process)
        {
            ProcessOrigin = process;
            Name = process.ProcessName;
            Id = process.Id;
            ThreadNumber = process.Threads.Count;
            try
            {
                StartTime = process.StartTime;
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                FilePath = process.MainModule.FileName;
            }
            catch (Exception)
            {
                FilePath = "Can't get file path";
            }
            _cpuCounter = new PerformanceCounter("Process", "% Processor Time", Name);
            _ramCounter = new PerformanceCounter("Process", "Working Set - Private", Name);
            UpdateMeta();
        }

        #endregion

        #region Properties
        public string Name { get; }

        public int Id { get; }

        public DateTime StartTime { get; }

        public string FilePath { get; }

        public double Cpu
        {
            get { return _cpu; }
            private set
            {
                _cpu = value;
                OnPropertyChanged();
            }
        }

        public long Ram
        {
            get { return _ram; }
            private set
            {
                _ram = value;
                OnPropertyChanged();
            }
        }

        public double RamPercent
        {
            get => _ramPercent;
            private set
            {
                _ramPercent = value;
                OnPropertyChanged();
            }
        }
        public int ThreadNumber { get; }

        public Process ProcessOrigin { get; }

        #endregion


        public void Terminate()
        {
            ProcessOrigin.Kill();
        }



        public void UpdateMeta()
        {
            if (_ok)
            {
                try
                {
                    Cpu = _cpuCounter.NextValue() / Environment.ProcessorCount / 100f;
                }
                catch (Exception)
                {
                    Cpu = 0;
                    _ok = false;
                }
                try
                {
                    Ram = Convert.ToInt64(_ramCounter.NextValue()) / 1024;
                    RamPercent = _ram / (double)WholeRam;
                }
                catch (Exception)
                {
                    Ram = 0;
                    RamPercent = 0;
                    _ok = false;
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
