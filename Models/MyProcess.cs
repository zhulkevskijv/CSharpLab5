using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using Lab5.Properties;

namespace Lab5.Models
{
    class MyProcess : INotifyPropertyChanged, IEquatable<MyProcess>
    {
        #region Fields

        private double _cpu;
        private double _ramPercent;
        private long _ram;
        private string _responding;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        
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
            Responding = process.Responding ? "Active" :"No Response";
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
            try
            {
                _cpuCounter = new PerformanceCounter("Process", "% Processor Time", Name);
                _cpu = _cpuCounter.NextValue()/Environment.ProcessorCount/100f;
            }
            catch (Exception)
            {
                _cpu = 0;
            }
            
            try
            {
                _ramCounter = new PerformanceCounter("Process", "Working Set - Private", Name);
                _ram = Convert.ToInt64(_ramCounter.NextValue()) / 1024;
                _ramPercent = _ram / (double)WholeRam;
            }
            catch (Exception)
            {
                _ram = 0;
                _ramPercent = 0;
            }

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

        public string Responding
        {
            get => _responding;
            private set
            {
                _responding = value;
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
            Responding = ProcessOrigin.Responding ? "Active" : "No Response";
            try
            {
                Cpu = _cpuCounter.NextValue()/Environment.ProcessorCount/100f;
            }
            catch (Exception)
            {
                Cpu = 0;
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

        
        public bool Equals(MyProcess other)
        {
            return  other != null && this.Id == other.Id;
        }
    }
}
