using System.Windows.Controls;
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
