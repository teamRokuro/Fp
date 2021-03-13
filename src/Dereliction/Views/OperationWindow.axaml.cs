#if DEBUG
using Avalonia;
#endif
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dereliction.Views
{
    public class OperationWindow : Window
    {
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
