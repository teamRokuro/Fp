using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dereliction.Models;
using Dereliction.Views;
using Fp;
using Microsoft.Extensions.Logging;
using ReactiveUI;

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

        private string _outputDirectory = string.Empty;

        public string OutputDirectory
        {
            get => _outputDirectory;
            set { this.RaiseAndSetIfChanged(ref _outputDirectory, value); }
        }

        private bool _directOutput;

        public bool DirectOutput
        {
            get => _directOutput;
            set { this.RaiseAndSetIfChanged(ref _directOutput, value); }
        }

        public OperationStateModel State { get; } = new();

        private readonly MsLogger _msLogger;

        public OperationRunnerViewModel()
        {
            _msLogger = new MsLogger(this);
            Inputs = new ObservableCollection<RealFsElement>();
            Outputs = new ObservableCollection<FsElement>();
            ClearLog();
        }

        #region Public surface

        public async Task AddDirectory(Window window) =>
            await new OpenFolderDialog().ShowAsync(window).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    string file = t.Result;
                    if (file.Length == 0)
                        return;
                    AddInput(file);
                }
            });

        public async Task AddFiles(Window window) =>
            await new OpenFileDialog {AllowMultiple = false}.ShowAsync(window).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    foreach (var f in t.Result)
                        AddInput(f);
                }
            });

        public void AddInput(string path) =>
            Inputs.Add(new RealFsElement(Path.GetFileName(path), Path.GetFullPath(path)));

        public void ClearInputs() => Inputs.Clear();

        public void Quit(OperationWindow mw) => mw.OnQuitClicked(mw, EventArgs.Empty);

        public Task RunScriptVisualAsync(MainWindow w)
        {
            var view = w.FindDescendantOfType<EditorView>();
            var viewModel = (view.DataContext as EditorViewModel)!;
            return RunScriptAsync(view.GetBody(), viewModel.EditorState.CurrentFile, viewModel.OperationState);
        }

        public async Task RunScriptAsync(string text, string? directory, OperationStateModel? state)
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
                        Processor.Registered.Factories.Clear();

                        await fpx.Program.LoadAsync(text, directory, Log);

                        p.Report(0.2f);

                        Log("Registered processors:");
                        foreach (var x in Processor.Registered.Factories)
                        {
                            var i = x.Info;
                            string exts = i.Extensions.Length == 0
                                ? "all"
                                : new StringBuilder().AppendJoin(", ", i.Extensions).ToString();
                            Log($"{i.Name}\nExtensions: {exts}\n{i.ExtendedDescription}\n");
                        }

                        Log("Creating processors...");
                        var processors = Processor.Registered.Factories
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
                            if (!processor.processor.CheckExtension(fake)) continue;
                            Log($"{fake} <{processor.name}>");
                            foreach (var data in Coordinator.OperateFileSegmented(processor.processor, fake,
                                fakeRoot,
                                configuration with {OutputRootDirectory = fakeRoot}, inputFilesystem, 0))
                            {
                                Log($" > {data}");
                                if (DirectOutput)
                                {
                                    if (!data.Dry && data.DefaultFormat != CommonFormat.ExportUnsupported)
                                    {
                                        try
                                        {
                                            using var d = data;
                                            string bp = processor.processor.GenPath(d.DefaultFormat.GetExtension(),
                                                d.BasePath, mkDirs: false);
                                            string fp = Path.Join(OutputDirectory, bp);
                                            string? dir = Path.GetDirectoryName(fp);
                                            if (dir != null) Directory.CreateDirectory(dir);
                                            await using var stream = File.OpenWrite(fp);
                                            d.WriteConvertedData(stream, d.DefaultFormat);
                                        }
                                        catch (Exception e)
                                        {
                                            Log(e.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    results.Add(data);
                                    Outputs.Add(new DataFsElement(Path.Combine(fakeRoot, data.BasePath), data));
                                }
                            }

                            p.Report(0.3f + 0.7f * ++idx / count);
                        }


                        p.Report(1.0f);
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

        #endregion

        public async Task SetOutputDirectoryAsync(Window window)
        {
            await new OpenFolderDialog().ShowAsync(window).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    string sourcePath = t.Result;
                    if (sourcePath.Length == 0)
                        return;
                    OutputDirectory = sourcePath;
                }
            });
        }


        private class DeInputFileSystemSource : FileSystemSource
        {
            private readonly InputModel _inputModel;

            public DeInputFileSystemSource(InputModel inputModel)
            {
                _inputModel = inputModel;
            }

            public override string NormalizePath(string path) => path.NormalizeAndStripWindowsDrive();

            protected override Stream OpenReadImpl(string path, FileMode fileMode = FileMode.Open,
                FileShare fileShare = FileShare.None | FileShare.Read | FileShare.Write | FileShare.ReadWrite |
                                      FileShare.Delete) =>
                _inputModel.TryGetFile(NormalizePath(path), out string? realPath)
                    ? File.OpenRead(realPath)
                    : throw new FileNotFoundException();

            public override Stream OpenWrite(string path, FileMode fileMode = FileMode.Create,
                FileShare fileShare = FileShare.None | FileShare.Read | FileShare.Write | FileShare.ReadWrite |
                                      FileShare.Delete) => throw new NotSupportedException();

            public override IEnumerable<string> EnumerateFiles(string path) =>
                _inputModel.TryGetDirectory(NormalizePath(path),
                    out List<(string fakePath, bool isDirectory)>? children)
                    ? children.Where(c => !c.isDirectory).Select(c => c.fakePath)
                    : throw new FileNotFoundException();

            public override IEnumerable<string> EnumerateDirectories(string path) =>
                _inputModel.TryGetDirectory(NormalizePath(path),
                    out List<(string fakePath, bool isDirectory)>? children)
                    ? children.Where(c => c.isDirectory).Select(c => c.fakePath)
                    : throw new FileNotFoundException();

            public override bool CreateDirectory(string path) => throw new NotSupportedException();

            public override bool FileExists(string path) => _inputModel.TryGetFile(NormalizePath(path), out string? _);

            public override bool DirectoryExists(string path) =>
                _inputModel.TryGetDirectory(NormalizePath(path), out string? _);
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
                    string fakeRoot = $"{Path.DirectorySeparatorChar}{i}";
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
                if (Leaves.TryGetValue(path, out (string realPath, bool isDirectory) x) &&
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
                if (Directories.TryGetValue(path, out var child))
                {
                    children = child;
                    return true;
                }

                children = null;
                return false;
            }

            public bool TryGetDirectory(string path, [NotNullWhen(true)] out string? realPath)
            {
                if (Leaves.TryGetValue(path, out (string realPath, bool isDirectory) x) &&
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

        private readonly AutoResetEvent _are = new(true);

        private void ClearLog()
        {
            _are.WaitOne();
            State.HardUp(_ => "");
            _are.Set();
        }

        private void Log(string? value, bool newLine = true)
        {
            _are.WaitOne();
            State.HardUp(src => src + value switch
            {
                { } v when newLine => v + '\n',
                { } v => v,
                _ when newLine => "\n",
                _ => "",
            });
            _are.Set();
        }

        private class MsLogger : ILogger
        {
            private readonly OperationRunnerViewModel _parent;
            public MsLogger(OperationRunnerViewModel parent) => _parent = parent;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter) =>
                _parent.Log(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => default!;
        }
    }
}
