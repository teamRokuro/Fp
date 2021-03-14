#if DEBUG
using Avalonia;
#endif
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using static System.Runtime.InteropServices.RuntimeInformation;

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

        #region Menu File

        public static string AddFilesHeader => IsOSPlatform(OSPlatform.OSX) ? "Add Files..." : "Add _Files...";

        public static KeyGesture AddFilesGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.O, KeyModifiers.Meta)
            : new KeyGesture(Key.O, KeyModifiers.Control);

        private async void OnAddFilesClicked(object? sender, EventArgs e) => await RunnerView.AddFiles(this);

        public static string AddDirectoryHeader => IsOSPlatform(OSPlatform.OSX) ? "Add Directory..." : "Add _Directory...";

        public static KeyGesture AddDirectoryGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.O, KeyModifiers.Meta | KeyModifiers.Shift)
            : new KeyGesture(Key.O, KeyModifiers.Control | KeyModifiers.Shift);

        private async void OnAddDirectoryClicked(object? sender, EventArgs e) => await RunnerView.AddDirectory(this);

        public static KeyGesture ClearGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.N, KeyModifiers.Meta)
            : new KeyGesture(Key.N, KeyModifiers.Control);

        public static string ClearHeader => IsOSPlatform(OSPlatform.OSX) ? "Clear Inputs" : "_Clear Inputs";
        private void OnClearClicked(object? sender, EventArgs e) => RunnerView.ClearInputs();

        public static string QuitHeader => IsOSPlatform(OSPlatform.OSX) ? $"Quit {Program.PROGRAM_NAME}" : "E_xit";

        public static KeyGesture QuitGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.Q, KeyModifiers.Meta)
            : new KeyGesture(Key.F4, KeyModifiers.Alt);

        private void OnQuitClicked(object? sender, EventArgs e) => Main.OnQuitClicked(sender, e);

        #endregion

        #region Menu Run

        public static string RunScriptHeader =>
            IsOSPlatform(OSPlatform.OSX) ? "Run Script" : "_Run Script";

        public static KeyGesture RunScriptGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.R, KeyModifiers.Meta)
            : new KeyGesture(Key.R, KeyModifiers.Control);

        private async void OnRunScriptClicked(object? sender, EventArgs e) => await RunScriptAsync(Main);

        #endregion
    }
}
