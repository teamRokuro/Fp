#if DEBUG
using Avalonia;
#endif
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Dereliction.ViewModels;

namespace Dereliction.Views
{
    public class OperationWindow : Window
    {
        public MainWindow Main { get; init; } = null!;
        private OperationRunnerView RunnerView => this.FindDescendantOfType<OperationRunnerView>();

        public OperationWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async Task RunScriptAsync(MainWindow mainWindow) => await RunnerView.RunScriptAsync(mainWindow);
    }
}
