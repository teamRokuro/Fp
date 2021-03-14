#if DEBUG
using Avalonia;
#endif
using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Dereliction.ViewModels;
using static System.Runtime.InteropServices.RuntimeInformation;

namespace Dereliction.Views
{
    public class MainWindow : Window
    {
        private readonly OperationWindow _operationWindow;
        private EditorView EditorView => this.FindDescendantOfType<EditorView>();
        private bool _shutdownWindow;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            _operationWindow = new OperationWindow {Main = this, DataContext = new OperationWindowViewModel()};
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

        public static KeyGesture NewGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.N, KeyModifiers.Meta)
            : new KeyGesture(Key.N, KeyModifiers.Control);

        public static string NewHeader => IsOSPlatform(OSPlatform.OSX) ? "New" : "_New";
        private async void OnNewClicked(object? sender, EventArgs e) => await EditorView.NewFileAsync();

        public static string OpenHeader => IsOSPlatform(OSPlatform.OSX) ? "Open..." : "_Open...";

        public static KeyGesture OpenGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.O, KeyModifiers.Meta)
            : new KeyGesture(Key.O, KeyModifiers.Control);

        private async void OnOpenClicked(object? sender, EventArgs e) => await EditorView.OpenFileAsync();

        public static string SaveHeader => IsOSPlatform(OSPlatform.OSX) ? "Save" : "_Save";

        public static KeyGesture SaveGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.S, KeyModifiers.Meta)
            : new KeyGesture(Key.S, KeyModifiers.Control);

        private async void OnSaveClicked(object? sender, EventArgs e) => await EditorView.SaveFileAsync();

        public static string OpenExecutionHeader =>
            IsOSPlatform(OSPlatform.OSX) ? "Open Execution View" : "Open _Execution View";

        public static KeyGesture OpenExecutionGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.E, KeyModifiers.Meta)
            : new KeyGesture(Key.E, KeyModifiers.Control);

        private void OnOpenExecutionClicked(object? sender, EventArgs e) => ShowOperationView();

        public static string RunScriptHeader =>
            IsOSPlatform(OSPlatform.OSX) ? "Run Script" : "_Run Script";

        public static KeyGesture RunScriptGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.R, KeyModifiers.Meta)
            : new KeyGesture(Key.R, KeyModifiers.Control);

        private async void OnRunScriptClicked(object? sender, EventArgs e)
        {
            ShowOperationView();
            await _operationWindow.RunScriptAsync(this);
        }

        public static string QuitHeader => IsOSPlatform(OSPlatform.OSX) ? $"Quit {Program.PROGRAM_NAME}" : "E_xit";

        public static KeyGesture QuitGesture => IsOSPlatform(OSPlatform.OSX)
            ? new KeyGesture(Key.Q, KeyModifiers.Meta)
            : new KeyGesture(Key.F4, KeyModifiers.Alt);

        private void OnQuitClicked(object? sender, EventArgs e) => Environment.Exit(0);
    }
}
