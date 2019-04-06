namespace Lab5.Tools.Navigation
{
    internal enum ViewType
    {
        TaskManager,
        ThreadsModules
    }

    interface INavigationModel
    {
        void Navigate(ViewType viewType);
    }
}
