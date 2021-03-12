using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dereliction.Views
{
    public class MainWindow : Window
    {
        private OperationWindow _operationWindow;
        public MainWindow()
        {
            InitializeComponent();
            _operationWindow = new OperationWindow();
            _operationWindow.Show();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
