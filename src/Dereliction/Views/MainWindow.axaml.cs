#if DEBUG
using Avalonia;
#endif
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dereliction.ViewModels;

namespace Dereliction.Views
{
    public class MainWindow : Window
    {
        private readonly OperationWindow _operationWindow;
        private bool _shutdownWindow;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _operationWindow = new OperationWindow {DataContext = new OperationWindowViewModel()};
            _operationWindow.Closing += (s, e) =>
            {
                if (_shutdownWindow) return;
                ((Window?)s)?.Hide();
                e.Cancel = true;
            };
            Closing += (_, _) =>
            {
                _shutdownWindow = true;
                _operationWindow.Close();
            };
        }

        public void ShowOperationView() => _operationWindow.Show();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
