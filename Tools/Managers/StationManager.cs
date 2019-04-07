using System;
using System.Windows;
using Lab5.Models;


namespace Lab5.Tools.Managers
{
    internal static class StationManager
    {
        public static event Action StopThreads;

        internal static MyProcess CurrentProcess { get; set; }

        internal static void CloseApp()
        {
            MessageBox.Show("Closing app");
            StopThreads?.Invoke();
            Environment.Exit(1);
        }
    }
}
