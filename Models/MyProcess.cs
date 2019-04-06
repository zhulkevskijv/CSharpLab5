using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;
using Lab5.Properties;

namespace Lab5.Models
{
    class MyProcess
    {
        #region Fields

        private Process _process;
        private readonly string _name;
        private readonly int _id;
        private readonly DateTime _startTime;
        private readonly string _filePath;
        private readonly double _cpu;
        private readonly double _ramPercent;
        private readonly long _ram;
        private readonly int _threadNumber;
        private PerformanceCounter _cpuCounter;
        private static long _wholeRam;

        #endregion

        #region Constructor

        internal MyProcess(Process process)
        {
            _process = process;
            _name = process.ProcessName;
            _id = process.Id;
            _threadNumber = process.Threads.Count;
            try
            {
                _startTime = process.StartTime;
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                _filePath = process.MainModule.FileName;
            }
            catch (Exception)
            {
                _filePath = "";
            }
            try
            {
                _cpuCounter = new PerformanceCounter("Process", "% Processor Time", _name);
                _cpu = _cpuCounter.NextValue();
            }
            catch (Exception)
            {
                _cpu = 0;
            }
            PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set - Private", _name);
            _ram = Convert.ToInt64(ramCounter.NextValue()) / 1024;
            //_ram=process.PrivateMemorySize64/1024;
            string Query = "SELECT Capacity FROM Win32_PhysicalMemory";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(Query);
            UInt64 capacity = 0;
            foreach (ManagementObject WniPART in searcher.Get())
            {
                capacity += Convert.ToUInt64(WniPART.Properties["Capacity"].Value);
            }
            _wholeRam = (long)capacity / 1024;
            _ramPercent = _ram / (double)_wholeRam;
        }

        #endregion

        #region Properties
        public string Name
        {
            get { return _name; }
        }

        public int Id
        {
            get { return _id; }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public double Cpu
        {
            get { return _cpu; }
        }

        public double Ram
        {
            get { return _ram; }
        }

        public double RamPercent
        {
            get { return _ramPercent; }
        }
        public int ThreadNumber
        {
            get { return _threadNumber; }
        }

        public Process ProcessOrigin
        {
            get { return _process; }
        }

        #endregion


        public void Terminate()
        {
            _process.Kill();
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
