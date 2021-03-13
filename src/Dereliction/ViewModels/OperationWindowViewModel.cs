namespace Dereliction.ViewModels
{
    public class OperationWindowViewModel : ViewModelBase
    {
        public OperationRunnerViewModel OperationRunner { get; } = new();
    }
}
