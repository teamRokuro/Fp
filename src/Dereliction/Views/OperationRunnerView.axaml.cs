using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dereliction.Views
{
    public class OperationRunnerView : UserControl
    {
        public OperationRunnerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
