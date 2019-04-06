using System;
using System.Windows;
using Lab5.Models;


namespace Lab5.Tools.Managers
{
    internal static class StationManager
    {

        internal static MyProcess CurrentProcess { get; set; }

        internal static void CloseApp()
        {
            MessageBox.Show("ShutDown");
            Environment.Exit(1);
        }
    }
}
