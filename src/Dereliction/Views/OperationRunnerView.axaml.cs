using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dereliction.ViewModels;

namespace Dereliction.Views
{
    public class OperationRunnerView : UserControl
    {
        private readonly ScrollViewer _logScrollViewer;

        private OperationRunnerViewModel RunnerModel =>
            DataContext as OperationRunnerViewModel ?? throw new ApplicationException();

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

        public async Task RunScriptAsync(MainWindow mainWindow) => await RunnerModel.RunScriptVisualAsync(mainWindow);
    }
}
