using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.VisualTree;
using Dereliction.Models;
using Dereliction.Views;
using Dotnet.Script.Core.Commands;
using Dotnet.Script.DependencyModel.Logging;
using Fp;
using Microsoft.CodeAnalysis;
using ReactiveUI;

namespace Dereliction.ViewModels
{
    public class OperationRunnerViewModel : ViewModelBase
    {
        private ObservableCollection<FsElement> _inputs = null!;

        public ObservableCollection<FsElement> Inputs
        {
            get => _inputs;
            set { this.RaiseAndSetIfChanged(ref _inputs, value); }
        }

        private ObservableCollection<FsElement> _outputs = null!;

        public ObservableCollection<FsElement> Outputs
        {
            get => _outputs;
            set { this.RaiseAndSetIfChanged(ref _outputs, value); }
        }

        private string _logText = null!;

        public string LogText
        {
            get => _logText;
            set { this.RaiseAndSetIfChanged(ref _logText, value); }
        }

        public OperationRunnerViewModel()
        {
            Inputs = new ObservableCollection<FsElement>();
            Outputs = new ObservableCollection<FsElement>();
            LogText = "";
            ClearLog();
            Log("Megumin");
            Log("Darkness");
            Log("Aqua");
            /*Inputs.Add(new RealFsElement("R1", @"C:\Users"));
            Inputs.Add(new RealFsElement("R2", @"C:\Users"));
            Outputs.Add(new RealFsElement("D1", @"C:\Users"));
            Outputs.Add(new RealFsElement("D2", @"C:\Users"));*/
        }

        public void SetInput()
        {
            // TODO set input
        }

        public async Task RunScript(MainWindow w)
        {
            var editorView = w.FindDescendantOfType<EditorView>();
            var state = (editorView.DataContext as EditorViewModel)!.State;
            state.Busy = true;
            state.Locked = true;
            string text = editorView.GetBody();
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        Log("Loading script...");
                        var progress = new Progress<float>();
                        progress.ProgressChanged += (_, e) => state.Percent = e * 100.0f;
                        Scripting.processors.Factories.Clear();
                        var options = new ExecuteCodeCommandOptions(text, Directory.GetCurrentDirectory(), null,
                            OptimizationLevel.Debug, false, null);
                        // TODO fp specifics
                        //await new ExecuteCodeCommand(ScriptConsole.Default, CreateLogFactory).Execute<int>(options);
                    }
                    catch (Exception e)
                    {
                        Log(e.ToString());
                    }
                });
            }
            finally
            {
                state.Busy = false;
                state.Locked = false;
            }
        }

        private Logger CreateLogFactory(Type t) => (l, m, e) => Log(m);

        public void ClearLog()
        {
            LogText = "";
        }

        public void Log(string value, bool newLine = true)
        {
            LogText += newLine ? value + '\n' : value;
        }
    }
}
