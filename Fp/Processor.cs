using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

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
        /// Currently running <see cref="ScriptingSegmentedProcessor"/> on this thread.
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
        public Stream? InputStream;

        /// <summary>
        /// Length of input stream for current file if opened
        /// </summary>
        public long InputLength;

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
        public Stream? OutputStream;

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
            InputRootDirectory = inputRoot.NormalizePath();
            InputFile = Path.Combine(InputRootDirectory, file).NormalizePath();
            InputDirectory = Path.GetDirectoryName(InputFile) ?? throw new ArgumentException("File is root");
            OutputRootDirectory = outputRoot.NormalizePath();
            OutputDirectory = Join(false,
                OutputRootDirectory, InputDirectory.Substring(InputRootDirectory.Length)).NormalizePath();
            InputStream = null;
            OutputStream = null;
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
                        Logger.LogError(e, $"Exception occurred during processing:\n{e}");
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
                if (Nop) continue;
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
            using var enumerator = ProcessSegmentedImpl().GetEnumerator();
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

        #endregion

        #region Logging utilities

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogInfo(string log) => Logger.LogInformation(log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogWarn(string log) => Logger.LogWarning(log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogFail(string log) => Logger.LogError(log);

        #endregion

        #region Lifecycle

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public virtual void Cleanup()
        {
            InputStream?.Dispose();
            OutputStream?.Dispose();
            InputStream = null;
            OutputStream = null;
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
}
