using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using static Fp.Processor;

namespace Fp
{
    /// <summary>
    /// Base type for file processors
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
    public partial class Processor : IDisposable
    {
        #region Properties and fields

        /// <summary>
        /// Currently running <see cref="Processor"/> on this thread.
        /// </summary>
        public static Processor Current => _current ?? throw new InvalidOperationException();

        [ThreadStatic] internal static Processor? _current;

        /// <summary>
        /// Per-thread instance.
        /// </summary>
        public static Processor Instance => _instance ??= new Processor();

        [ThreadStatic] private static Processor? _instance;

        private const int StringDefaultCapacity = 4 * 1024;
        private const int StringExcessiveCapacity = 128 * 1024;

        /// <summary>
        /// True if current platform allows backslashes as separators (modifies path creation / operation behaviour)
        /// </summary>
        public static readonly bool PlatformSupportBackSlash =
            Path.DirectorySeparatorChar == '\\' || Path.AltDirectorySeparatorChar == '\\';

        /// <summary>
        /// Hint for segmented processor to perform dry-run
        /// </summary>
        public bool Dry;

        /// <summary>
        /// Whether to allow backslash as separator
        /// </summary>
        public bool SupportBackSlash;

        /// <summary>
        /// ID of worker thread processor is using
        /// </summary>
        public int WorkerId;

        /// <summary>
        /// Arguments.
        /// </summary>
        public IReadOnlyList<string> Args = null!;

        /// <summary>
        /// Log output target
        /// </summary>
        public ILogger Logger = null!;

        /// <summary>
        /// Whether to preload newly opened file input streams to <see cref="MemoryStream"/>
        /// </summary>
        public bool Preload;

        /// <summary>
        /// Input stream for current file if opened
        /// </summary>
        public Stream? InputStream
        {
            get => _inputStream;
            set => SetInputStream(value);
        }

        internal Stream? _inputStream;

        /// <summary>
        /// Input stream stack
        /// </summary>
        private readonly Stack<Stream?> _inputStack = new(new[] {(Stream?)null});

        /// <summary>
        /// Length of input stream for current file if opened
        /// </summary>
        public long InputLength => _inputStream?.Length ?? throw new InvalidOperationException();

        /// <summary>
        /// Root input directory
        /// </summary>
        public string InputRootDirectory = null!;

        /// <summary>
        /// Current input directory
        /// </summary>
        public string InputDirectory = null!;

        /// <summary>
        /// Current input file
        /// </summary>
        public string InputFile = null!;

        /// <summary>
        /// Output stream for current file if opened
        /// </summary>
        public Stream? OutputStream { get; set; }

        /// <summary>
        /// Root output directory
        /// </summary>
        public string OutputRootDirectory = null!;

        /// <summary>
        /// Current output directory
        /// </summary>
        public string OutputDirectory = null!;

        /// <summary>
        /// Current output file index
        /// </summary>
        public int OutputCounter;

        /// <summary>
        /// Filesystem provider for this processor
        /// </summary>
        public FileSystemSource FileSystem = null!;

        /// <summary>
        /// Origin factory for this processor, if available
        /// </summary>
        public ProcessorFactory? Source;

        /// <summary>
        /// Whether to read input as little-endian
        /// </summary>
        public bool LittleEndian
        {
            get => _littleEndian;
            set
            {
                _littleEndian = value;
                _swap = BitConverter.IsLittleEndian ^ _littleEndian;
            }
        }

        /// <summary>
        /// Set by subclasses to indicate if no more processors
        /// should be run on the current input file (not applicable
        /// in multithreaded environment)
        /// </summary>
        [SuppressMessage("ReSharper", "UnassignedField.Global")]
        public bool Lock;

        private bool _littleEndian;

        private bool _swap;

        private bool _overrideProcess = true;
        private bool _overrideProcessSegmented = true;

        private MemoryStream TempMs => _tempMs ??= new MemoryStream();
        private MemoryStream? _tempMs;
        private static byte[] TempBuffer => _tempBuffer ??= new byte[sizeof(long)];
        [ThreadStatic] private static byte[]? _tempBuffer;

        private Encoder Utf8Encoder => _utf8Encoder ??= Encoding.UTF8.GetEncoder();
        private Encoder? _utf8Encoder;
        private Encoder?[] Utf16Encoders => _utf16Encoders ??= new Encoder?[GUtf16Encodings.Length];
        private Encoder?[]? _utf16Encoders;

        private Encoder GetUtf16Encoder(bool bigEndian, bool bom)
        {
            int i = (bigEndian ? 1 : 0) + (bom ? 2 : 0);
            return Utf16Encoders[i] ??= GUtf16Encodings[i].GetEncoder();
        }

        private static Encoding GetUtf16Encoding(bool bigEndian, bool bom) =>
            GUtf16Encodings[(bigEndian ? 1 : 0) + (bom ? 2 : 0)];

        private static Encoding[] GUtf16Encodings => _gUtf16Encodings ??= new Encoding[]
        {
            new UnicodeEncoding(false, false), new UnicodeEncoding(true, false), new UnicodeEncoding(false, true),
            new UnicodeEncoding(true, true)
        };

        private static Encoding[]? _gUtf16Encodings;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="Processor"/>.
        /// </summary>
        public Processor()
        {
            InitEncodingDecodingHelpers();
        }

        #endregion

        #region Main operation functions

        /// <summary>
        /// Prepare critical state for operation
        /// </summary>
        /// <param name="fileSystem">Filesystem source</param>
        /// <param name="inputRoot">Input root directory</param>
        /// <param name="outputRoot">Output root directory</param>
        /// <param name="file">Input file</param>
        /// <param name="configuration">Additional configuration object.</param>
        /// <param name="workerId">Worker ID.</param>
        public void Prepare(FileSystemSource fileSystem, string inputRoot, string outputRoot, string file,
            ProcessorConfiguration? configuration = null, int workerId = 0)
        {
            Cleanup(true);
            InputRootDirectory = fileSystem.NormalizePath(inputRoot);
            InputFile = fileSystem.NormalizePath(Path.Combine(InputRootDirectory, file));
            InputDirectory = Path.GetDirectoryName(InputFile) ?? throw new ArgumentException("File is root");
            OutputRootDirectory = fileSystem.NormalizePath(outputRoot);
            OutputDirectory = fileSystem.NormalizePath(Join(false,
                OutputRootDirectory, InputDirectory.Substring(InputRootDirectory.Length)));
            LittleEndian = true;
            OutputCounter = 0;
            FileSystem = fileSystem;
            SupportBackSlash = false;
            WorkerId = workerId;
            if (configuration == null) return;
            Debug = configuration.Debug;
            Nop = configuration.Nop;
            Preload = configuration.Preload;
            Logger = configuration.Logger;
            Args = configuration.Args;
        }

        /// <summary>
        /// Checks if processor will accept filepath based on extension and <see cref="Source"/>.<see cref="ProcessorFactory.Info"/>.<see cref="ProcessorInfo.Extensions"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool CheckExtension(string path) =>
            Source?.Info.Extensions is not { } exts
            || exts.Length == 0
            || PathHasExtension(path, exts);

        /// <summary>
        /// Process layered content using additional processor.
        /// </summary>
        /// <param name="main">Main file.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="additionalFiles">Additional files to pass to processor.</param>
        /// <typeparam name="T">Processor type.</typeparam>
        public void SubProcess<T>(BufferData<byte> main, string[]? args = null,
            IEnumerable<BufferData<byte>>? additionalFiles = null)
            where T : Processor, new()
        {
            IEnumerable<BufferData<byte>> seq = new[] {main};
            if (additionalFiles != null) seq = seq.Concat(additionalFiles);
            var child = new T();
            var layer1 = new FileSystemSource.SegmentedFileSystemSource(FileSystem, true, seq);
            child.Prepare(layer1, InputRootDirectory, OutputRootDirectory, main.BasePath);
            child.Debug = Debug;
            child.Nop = Nop;
            child.Preload = Preload;
            child.Logger = Logger;
            child.Args = args ?? new string[0];
            try
            {
                if (Debug)
                    child.Process();
                else
                {
                    try
                    {
                        child.Process();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Exception occurred during processing:{Exception}", e);
                    }
                }
            }
            finally
            {
                child.Cleanup();
            }
        }

        /// <summary>
        /// Process current file
        /// </summary>
        protected virtual void ProcessImpl() => _overrideProcess = false;

        /// <summary>
        /// Process current file in parts
        /// </summary>
        /// <returns>Generated outputs</returns>
        protected virtual IEnumerable<Data> ProcessSegmentedImpl()
        {
            _overrideProcessSegmented = false;
            yield break;
        }

        /// <summary>
        /// Process current file
        /// </summary>
        public void Process()
        {
            ShieldProcess();
            if (_overrideProcess) return;
            foreach (Data d in ShieldProcessSegmented())
            {
                if (Nop || d is MetaData && !Debug) continue;
                using Data data = d;
                using Stream stream =
                    (FileSystem ?? throw new InvalidOperationException()).OpenWrite(
                        GenPath(data.DefaultFormat.GetExtension(), data.BasePath));
                data.WriteConvertedData(stream, data.DefaultFormat);
            }
        }

        /// <summary>
        /// Process current file in parts
        /// </summary>
        /// <returns>Generated outputs</returns>
        public IEnumerable<Data> ProcessSegmented()
        {
            foreach (Data entry in ShieldProcessSegmented()) yield return entry;
            if (_overrideProcessSegmented) yield break;
            FileSystemSource prevFs = FileSystem ?? throw new InvalidOperationException();
            FileSystemSource.SegmentedFileSystemSource fs = new(prevFs, false);
            FileSystem = fs;
            try
            {
                ShieldProcess();
                foreach ((string path, Stream stream) in fs)
                    yield return new BufferData<byte>(path, GetMemory(stream));
            }
            finally
            {
                FileSystem = prevFs;
            }
        }

        /// <summary>
        /// Process current file
        /// </summary>
        protected void ShieldProcess()
        {
            _current = this;
            try
            {
                ProcessImpl();
            }
            finally
            {
                _current = null;
            }
        }

        /// <summary>
        /// Process current file in parts
        /// </summary>
        /// <returns>Generated outputs</returns>
        protected IEnumerable<Data> ShieldProcessSegmented()
        {
            try
            {
                _current = this;
                using var enumerator = ProcessSegmentedImpl().GetEnumerator();
                _current = null;
                bool has;
                do
                {
                    _current = this;
                    try
                    {
                        has = enumerator.MoveNext();
                    }
                    finally
                    {
                        _current = null;
                    }

                    if (has) yield return enumerator.Current!;
                } while (has);
            }
            finally
            {
                _current = null;
            }
        }

        #endregion

        #region Logging utilities

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogInfo(string log) => Logger.LogInformation("{Log}", log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogWarn(string log) => Logger.LogWarning("{Log}", log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogFail(string log) => Logger.LogError("{Log}", log);

        #endregion

        #region Factory utilities

        /// <summary>
        /// Creates a factory for the specified type, obtaining information from applied <see cref="ProcessorInfoAttribute"/> if possible
        /// </summary>
        /// <typeparam name="T">Processor type</typeparam>
        /// <returns>Processor factory</returns>
        public static ProcessorFactory GetFactory<T>() where T : Processor, new()
        {
            ProcessorInfo? processorInfo = null;
            try
            {
                var attrs = typeof(T).GetCustomAttributes(typeof(ProcessorInfoAttribute), true);
                if (attrs.FirstOrDefault() is ProcessorInfoAttribute attr) processorInfo = attr.Info;
            }
            catch
            {
                // When reflection doesn't work, just fallback
            }

            return new GenericNewProcessorFactory<T>(processorInfo);
        }

        #endregion

        #region I/O stack

        /// <summary>
        /// Manages a processor's stream stack
        /// </summary>
        private sealed class InputStackContext : IDisposable
        {
            /// <summary>
            /// Processor to operate on
            /// </summary>
            private readonly Processor _processor;

            /// <summary>
            /// Stream to push
            /// </summary>
            private readonly Stream _stream;

            private bool _disposed;

            /// <summary>
            /// Create new instance of <see cref="InputStackContext"/>
            /// </summary>
            /// <param name="processor">Processor to operate on</param>
            /// <param name="stream">Stream to push</param>
            /// <remarks>
            /// This constructor also calls <see cref="PushInput"/>
            /// </remarks>
            internal InputStackContext(Processor processor, Stream stream)
            {
                _processor = processor;
                _stream = stream;
                _processor.PushInput(_stream);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _stream.Dispose();
                _processor.PopInput();
            }
        }

        private void PushInput(Stream? stream)
        {
            _inputStack.Push(stream);
            _inputStream = stream;
        }

        private void PopInput()
        {
            if (_inputStack.Count == 1)
                throw new InvalidOperationException("Cannot pop input stack at root level");
            _inputStack.Pop();
            _inputStream = _inputStack.Peek();
        }

        private void SetInputStream(Stream? stream)
        {
            _inputStack.Pop();
            _inputStack.Push(stream);
            _inputStream = stream;
        }

        /// <summary>
        /// Creates a region of and overwrites current <see cref="InputStream"/>
        /// </summary>
        /// <param name="offset">Offset relative to current stream</param>
        /// <param name="length">Length of region</param>
        /// <returns>Disposable context that will restore previous <see cref="InputStream"/> when disposed</returns>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="InputStream"/> is null or not seekable</exception>
        public IDisposable Region(long offset, long? length = null)
        {
            if (_inputStream == null)
                throw new InvalidOperationException($"Cannot make region when {nameof(InputStream)} is null");
            if (!_inputStream.CanSeek)
                throw new InvalidOperationException($"Cannot make region when {nameof(InputStream)} is not seekable");

            var ins = _inputStream;
            // Go through sub-streams if possible to reduce chaining
            while (ins is SStream {CanSeek: true} sStr)
            {
                offset += sStr.Offset;
                ins = sStr.BaseStream;
            }

            ins.Position = offset;
            return new InputStackContext(this, new SStream(ins, length ?? ins.Length - offset));
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Cleanup resources
        /// </summary>
        /// <param name="warn">Warn if resources were not previously cleaned up</param>
        public virtual void Cleanup(bool warn = false)
        {
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                if (warn) Logger.LogWarning("Input stream was not disposed prior to cleanup call");
                InputStream = null;
            }

            if (OutputStream != null)
            {
                OutputStream.Dispose();
                if (warn) Logger.LogWarning("Output stream was not disposed prior to cleanup call");
                OutputStream = null;
            }

            MemClear();
        }

        #endregion

        #region Dispose pattern

        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">False if called from finalizer</param>
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        // ReSharper disable once UnusedParameter.Global
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Cleanup();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    // ReSharper disable InconsistentNaming
    public partial class Scripting
    {
        #region Properties

        /// <summary>
        /// Current file path.
        /// </summary>
        public static FpPath _file => FpPath.GetFromString(Current.InputFile) ?? throw new InvalidOperationException();

        /// <summary>
        /// Current file path without extension.
        /// </summary>
        public static FpPath _fileNoExt =>
            new(Path.GetFileNameWithoutExtension(Current.InputFile), Current.InputDirectory);

        /// <summary>
        /// Current file name.
        /// </summary>
        public static string _name => Path.GetFileName(Current.InputFile);

        /// <summary>
        /// Current file length.
        /// </summary>
        public static long _length => Current.InputLength;

        /// <summary>
        /// Current file name without extension.
        /// </summary>
        public static string _nameNoExt => Path.GetFileNameWithoutExtension(Current.InputFile);

        /// <summary>
        /// Current file name.
        /// </summary>
        public static FpPath _namePath => FpPath.GetFromString(_name) ?? throw new InvalidOperationException();

        /// <summary>
        /// Current file name without extension.
        /// </summary>
        public static FpPath _namePathNoExt =>
            FpPath.GetFromString(_nameNoExt) ?? throw new InvalidOperationException();

        #endregion

        #region Logging

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public static void info(string log) => Current.LogInfo(log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public static void warn(string log) => Current.LogWarn(log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public static void fail(string log) => Current.LogFail(log);

        #endregion
    }
    // ReSharper restore InconsistentNaming
}
