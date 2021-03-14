using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dereliction.Models;
using Dereliction.Views;
using Dotnet.Script.Core;
using Dotnet.Script.Core.Commands;
using Dotnet.Script.DependencyModel.Logging;
using Fp;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using LogLevel = Dotnet.Script.DependencyModel.Logging.LogLevel;

namespace Dereliction.ViewModels
{
    public class OperationRunnerViewModel : ViewModelBase
    {
        private ObservableCollection<RealFsElement> _inputs = null!;

        public ObservableCollection<RealFsElement> Inputs
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

        public OperationStateModel State { get; } = new();

        private readonly Tw _tw;
        private readonly MsLogger _msLogger;

        public OperationRunnerViewModel()
        {
            _tw = new Tw(this);
            _msLogger = new MsLogger(this);
            Inputs = new ObservableCollection<RealFsElement>();
            Outputs = new ObservableCollection<FsElement>();
            ClearLog();
            /*Inputs.Add(new RealFsElement("R1", @"C:\Users"));
            Inputs.Add(new RealFsElement("R2", @"C:\Users"));
            Outputs.Add(new RealFsElement("D1", @"C:\Users"));
            Outputs.Add(new RealFsElement("D2", @"C:\Users"));*/
        }

        public void AddDirectory(Window window) =>
            new OpenFolderDialog().ShowAsync(window).ContinueWith(AddDirectoryFromTask);

        private void AddDirectoryFromTask(Task<string> t)
        {
            if (!t.IsCanceled)
            {
                string file = t.Result;
                if (file.Length == 0)
                    return;
                AddInput(file);
            }
        }

        public void AddFile(Window window) =>
            new OpenFileDialog {AllowMultiple = false}.ShowAsync(window).ContinueWith(AddFileFromTask);

        private void AddFileFromTask(Task<string[]> t)
        {
            if (!t.IsCanceled)
            {
                string file = t.Result[0];
                if (file.Length == 0)
                    return;
                AddInput(file);
            }
        }

        public void ClearInputs() => Inputs.Clear();

        private void AddInput(string path) =>
            Inputs.Add(new RealFsElement(Path.GetFileName(path), Path.GetFullPath(path)));

        public Task RunScript(MainWindow w)
        {
            var editorView = w.FindDescendantOfType<EditorView>();
            var state = (editorView.DataContext as EditorViewModel)!.OperationState;
            return RunScriptCore(editorView.GetBody(), state);
        }

        private async Task RunScriptCore(string text, OperationStateModel? state)
        {
            if (state != null)
            {
                state.Busy = true;
                state.Locked = true;
            }

            State.Busy = true;
            State.Locked = true;
            var progress = new Progress<float>();
            progress.ProgressChanged += (_, e) =>
            {
                float percent = e * 100.0f;
                if (state != null) state.Percent = percent;
                State.Percent = percent;
            };
            var p = (IProgress<float>)progress;
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        p.Report(0.0f);

                        ClearLog();
                        Outputs.Clear();

                        Scripting.processors.Factories.Clear();
                        var options = new ExecuteCodeCommandOptions(text, Directory.GetCurrentDirectory(),
                            new[] {Scripting.NO_EXECUTE_CLI},
                            OptimizationLevel.Debug, false, null);
                        Log("Executing script...");
                        await new ExecuteCodeCommand(new ScriptConsole(_tw, TextReader.Null, _tw), CreateLogFactory)
                            .Execute<int>(options);
                        Log("Execution finished.");

                        p.Report(0.2f);

                        Log("Registered processors:");
                        foreach (var x in Scripting.processors.Factories)
                        {
                            var i = x.Info;
                            string exts = i.Extensions.Length == 0
                                ? "all"
                                : new StringBuilder().AppendJoin(", ", i.Extensions).ToString();
                            Log($"{i.Name}: {i.Description}\nExtensions: {exts}\n{i.ExtendedDescription}\n");
                        }

                        Log("Creating processors...");
                        var processors = Scripting.processors.Factories
                            .Select(f => (name: f.Info.Name, processor: f.CreateProcessor())).ToArray();
                        var configuration =
                            new ProcessorConfiguration("", 1, false, true, false, _msLogger, Array.Empty<string>());

                        Log("Loading input tree...");
                        var inputModel = InputModel.Create(Inputs);
                        var inputFilesystem = new DeInputFileSystemSource(inputModel);

                        p.Report(0.3f);

                        Log("Processing tree...");
                        HashSet<Data> results = new();
                        float idx = 0, count = inputModel.Inputs.Count * processors.Length;
                        foreach ((string fakeRoot, string fake) in inputModel.Inputs)
                        foreach (var processor in processors)
                        {
                            Log($"{fake} <{processor.name}>");
                            foreach (var data in Coordinator.OperateFileSegmented(processor.processor, fake,
                                fakeRoot,
                                configuration with {OutputRootDirectory = fakeRoot}, inputFilesystem, 0))
                            {
                                results.Add(data);
                                Outputs.Add(new DataFsElement(Path.Combine(fakeRoot, data.BasePath), data));
                            }

                            p.Report(0.3f + 0.7f * ++idx / count);

                            await Task.Delay(1000);
                        }

                        Log("\nTree processing complete");

                        p.Report(1.0f);

                        Log("\nResults:");
                        foreach (var data in results) Log(data.ToString()!);
                        Log("\nOperation complete.");
                    }
                    catch (Exception e)
                    {
                        Log(e.ToString());
                    }
                });
            }
            finally
            {
                if (state != null)
                {
                    state.Busy = false;
                    state.Locked = false;
                }

                State.Busy = false;
                State.Locked = false;
            }
        }

        private class DeInputFileSystemSource : FileSystemSource
        {
            private readonly InputModel _inputModel;

            public DeInputFileSystemSource(InputModel inputModel)
            {
                _inputModel = inputModel;
            }

            protected override Stream OpenReadImpl(string path, FileMode fileMode = FileMode.Open,
                FileShare fileShare = FileShare.None | FileShare.Read | FileShare.Write | FileShare.ReadWrite |
                                      FileShare.Delete) =>
                _inputModel.TryGetFile(path, out string? realPath)
                    ? File.OpenRead(realPath)
                    : throw new FileNotFoundException();

            public override Stream OpenWrite(string path, FileMode fileMode = FileMode.Create,
                FileShare fileShare = FileShare.None | FileShare.Read | FileShare.Write | FileShare.ReadWrite |
                                      FileShare.Delete) => throw new NotSupportedException();

            public override IEnumerable<string> EnumerateFiles(string path) =>
                _inputModel.TryGetDirectory(path, out List<(string fakePath, bool isDirectory)>? children)
                    ? children.Where(c => !c.isDirectory).Select(c => c.fakePath)
                    : throw new FileNotFoundException();

            public override IEnumerable<string> EnumerateDirectories(string path) =>
                _inputModel.TryGetDirectory(path, out List<(string fakePath, bool isDirectory)>? children)
                    ? children.Where(c => c.isDirectory).Select(c => c.fakePath)
                    : throw new FileNotFoundException();

            public override bool CreateDirectory(string path) => throw new NotSupportedException();

            public override bool FileExists(string path) => _inputModel.TryGetFile(path, out string? _);

            public override bool DirectoryExists(string path) => _inputModel.TryGetDirectory(path, out string? _);
        }

        private class InputModel
        {
            public readonly Dictionary<string, List<(string fakePath, bool isDirectory)>> Directories = new();
            public readonly Dictionary<string, (string realPath, bool isDirectory)> Leaves = new();
            public readonly List<(string fakeRoot, string fake)> Inputs = new();

            private InputModel(IEnumerable<RealFsElement> elements)
            {
                int i = 0;
                foreach (var e in elements)
                {
                    string real = e.RealPath;
                    string fakeRoot = $"{Path.VolumeSeparatorChar}{i}".NormalizePath();
                    string fake = Path.Combine(fakeRoot, e.Name);
                    Add(fakeRoot, real, fake, out _);
                    i++;
                }
            }

            private bool Add(string fakeRoot, string real, string fake, out bool isDirectory)
            {
                if (File.Exists(real))
                {
                    isDirectory = false;
                    Leaves.Add(fake, (real, false));
                    Inputs.Add((fakeRoot, fake));
                    return true;
                }

                if (Directory.Exists(real))
                {
                    isDirectory = true;
                    Leaves.Add(fake, (real, true));
                    List<(string fakePath, bool isDirectory)> files = new();
                    Directories.Add(fake, files);
                    foreach (var fse in Directory.GetFileSystemEntries(fake))
                    {
                        string fakeSub = Path.GetFileName(fse);
                        if (Add(fakeRoot, fse, Path.Combine(fake, fakeSub), out bool isSubDirectory))
                            files.Add((fakeSub, isSubDirectory));
                    }

                    return true;
                }

                isDirectory = false;
                return false;
            }

            public bool TryGetFile(string path, [NotNullWhen(true)] out string? realPath)
            {
                if (Leaves.TryGetValue(path.NormalizePath(), out (string realPath, bool isDirectory) x) &&
                    !x.isDirectory)
                {
                    realPath = x.realPath;
                    return true;
                }

                realPath = null;
                return false;
            }

            public bool TryGetDirectory(string path,
                [NotNullWhen(true)] out List<(string fakePath, bool isDirectory)>? children)
            {
                if (Directories.TryGetValue(path.NormalizePath(), out var child))
                {
                    children = child;
                    return true;
                }

                children = null;
                return false;
            }

            public bool TryGetDirectory(string path, [NotNullWhen(true)] out string? realPath)
            {
                if (Leaves.TryGetValue(path.NormalizePath(), out (string realPath, bool isDirectory) x) &&
                    x.isDirectory)
                {
                    realPath = x.realPath;
                    return true;
                }

                realPath = null;
                return false;
            }

            public static InputModel Create(IEnumerable<RealFsElement> elements) => new(elements);
        }

        // ReSharper disable once UnusedParameter.Local
        private Logger CreateLogFactory(Type t) => (l, m, e) =>
        {
            switch (l)
            {
                case LogLevel.Trace:
                case LogLevel.Warning:
                case LogLevel.Critical:
                case LogLevel.Error:
                    Log(m);
                    break;
                case LogLevel.Debug:
#if DEBUG
                    Log(m);
#endif
                    break;
            }
        };

        public void ClearLog()
        {
            State.LogText = "";
        }

        public void Log(string value, bool newLine = true)
        {
            State.LogText += newLine ? value + '\n' : value;
        }

        private class MsLogger : ILogger
        {
            private readonly OperationRunnerViewModel _parent;
            public MsLogger(OperationRunnerViewModel parent) => _parent = parent;

            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter) =>
                _parent.Log(formatter(state, exception));

            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => default!;
        }

        private class Tw : TextWriter
        {
            private readonly OperationRunnerViewModel _parent;
            public Tw(OperationRunnerViewModel parent) => _parent = parent;
            public override Encoding Encoding => Encoding.Unicode;
            public override void Write(char value) => _parent.State.LogText += value;
        }
    }
}
