#if DEBUG
using Avalonia;
#endif
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dereliction.Views
{
    public class OperationWindow : Window
    {
        public MainWindow Main { get; init; } = null!;
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
    }
}
