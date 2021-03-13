using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Dereliction.Views
{
    public class OperationRunnerView : UserControl
    {
        private readonly ScrollViewer _logScrollViewer;

        public OperationRunnerView()
        {
            InitializeComponent();
            _logScrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LogScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentDelta.Y != 0) _logScrollViewer.ScrollToEnd();
        }
    }
}
