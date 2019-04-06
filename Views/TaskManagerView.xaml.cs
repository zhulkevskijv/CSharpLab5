using System.Windows.Controls;
using Lab5.Tools.Navigation;
using Lab5.ViewModels;

namespace Lab5.Views
{
    /// <summary>
    /// Логика взаимодействия для PersonListView.xaml
    /// </summary>
    public partial class TaskManagerView : UserControl, INavigatable
    {
        public TaskManagerView()
        {
            InitializeComponent();
            DataContext = new TaskManagerViewModel();
        }
    }
}
