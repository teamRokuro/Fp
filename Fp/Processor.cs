using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Fp.Intermediate;

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
        /// Log output target
        /// </summary>
        public Action<string>? Logger;

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
        public string? InputRootDirectory;

        /// <summary>
        /// Current input directory
        /// </summary>
        public string? InputDirectory;

        /// <summary>
        /// Current input file
        /// </summary>
        public string? InputFile;

        /// <summary>
        /// Output stream for current file if opened
        /// </summary>
        public Stream? OutputStream;

        /// <summary>
        /// Root output directory
        /// </summary>
        public string? OutputRootDirectory;

        /// <summary>
        /// Current output directory
        /// </summary>
        public string? OutputDirectory;

        /// <summary>
        /// Current output file index
        /// </summary>
        public int OutputCounter;

        /// <summary>
        /// Filesystem provider for this processor
        /// </summary>
        public FileSystemSource? FileSystem;

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
        private readonly byte[] _tempBuffer = new byte[sizeof(long)];

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

        #region Main operation functions

        /// <summary>
        /// Prepare critical state for operation
        /// </summary>
        /// <param name="fileSystem">Filesystem source</param>
        /// <param name="inputRoot">Input root directory</param>
        /// <param name="outputRoot">Output root directory</param>
        /// <param name="file">Input file</param>
        public void Prepare(FileSystemSource fileSystem, string inputRoot, string outputRoot, string file)
        {
            InputRootDirectory = inputRoot;
            InputFile = file;
            InputDirectory = Path.GetDirectoryName(file) ?? throw new Exception();
            OutputRootDirectory = outputRoot;
            OutputDirectory = Join(false, outputRoot, InputDirectory.Substring(inputRoot.Length));
            InputStream = null;
            OutputStream = null;
            LittleEndian = true;
            OutputCounter = 0;
            FileSystem = fileSystem;
            SupportBackSlash = false;
        }

        /// <summary>
        /// Process current file
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        protected virtual void ProcessImpl(IReadOnlyList<string> args) => _overrideProcess = false;

        /// <summary>
        /// Process current file in parts
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Generated outputs</returns>
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        protected virtual IEnumerable<Data> ProcessSegmentedImpl(
            IReadOnlyList<string> args)
        {
            _overrideProcessSegmented = false;
            yield break;
        }

        /// <summary>
        /// Process current file
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Generated outputs</returns>
        public void Process(IReadOnlyList<string> args)
        {
            ProcessImpl(args);
            if (_overrideProcess)
            {
                return;
            }

            foreach (Data d in ProcessSegmentedImpl(args))
            {
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
        /// <param name="args">Command-line arguments</param>
        /// <returns>Generated outputs</returns>
        public IEnumerable<Data> ProcessSegmented(
            IReadOnlyList<string> args)
        {
            foreach (Data entry in ProcessSegmentedImpl(args))
            {
                yield return entry;
            }

            if (_overrideProcessSegmented)
            {
                yield break;
            }

            FileSystemSource prevFs = FileSystem ?? throw new InvalidOperationException();
            FileSystemSource.SegmentedFileSystemSource fs = new FileSystemSource.SegmentedFileSystemSource(prevFs);
            FileSystem = fs;
            try
            {
                ProcessImpl(args);
                foreach ((string path, byte[] buffer, int offset, int length) in fs)
                {
                    yield return new BufferData<byte>(path, buffer.AsMemory(offset, length));
                }
            }
            finally
            {
                FileSystem = prevFs;
            }
        }

        #endregion

        #region Logging utilities

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogInfo(string log) => Logger?.Invoke($"[{WorkerId:X2}][~]<INFO>: {log}");

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogWarn(string log) => Logger?.Invoke($"[{WorkerId:X2}][!]<WARN>: {log}");

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogFail(string log) => Logger?.Invoke($"[{WorkerId:X2}][X]<FAIL>: {log}");

        #endregion

        #region Lifecycle

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public virtual void SrcCleanup()
        {
            InputStream?.Dispose();
            OutputStream?.Dispose();
            InputStream = null;
            OutputStream = null;
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
            SrcCleanup();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
