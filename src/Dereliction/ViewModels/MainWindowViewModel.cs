namespace Dereliction.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public EditorViewModel Editor { get; } = new();
    }
}
