using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lab5.Tools.Navigation;
using Lab5.ViewModels;

namespace Lab5.Views
{
    /// <summary>
    /// Логика взаимодействия для ThreadsModulesView.xaml
    /// </summary>
    public partial class ThreadsModulesView : UserControl, INavigatable
    {
        public ThreadsModulesView()
        {
            InitializeComponent();
            DataContext = new ThreadModuleViewModel();
        }
    }
}
