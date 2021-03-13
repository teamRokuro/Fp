namespace Dereliction.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public EditorViewModel Editor => new();
    }
}
