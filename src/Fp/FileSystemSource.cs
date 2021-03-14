using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fp
{
    /// <summary>
    /// Object for I/O to some filesystem provider
    /// </summary>
    public abstract class FileSystemSource
    {
        /// <summary>
        /// Set when filesystem is being accessed in parallel
        /// (copies input streams)
        /// </summary>
        public bool ParallelAccess;

        /// <summary>
        /// Default filesystem provider for platform
        /// </summary>
        public static readonly FileSystemSource Default = new RealFileSystemSource();

        /// <summary>
        /// Normalize a path to the underlying filesystem
        /// </summary>
        /// <param name="path">Path to normalize</param>
        /// <returns>Normalized path</returns>
        public abstract string NormalizePath(string path);

        /// <summary>
        /// Get seekable read-only stream
        /// </summary>
        /// <param name="path">Path to open</param>
        /// <param name="fileMode">File open mode</param>
        /// <param name="fileShare">File sharing mode</param>
        /// <returns>Stream</returns>
        public Stream OpenRead(string path, FileMode fileMode = FileMode.Open, FileShare fileShare =
            FileShare.ReadWrite | FileShare.Delete)
        {
            Stream src = Processor.GetSeekableStream(OpenReadImpl(path, fileMode, fileShare));
            if (!ParallelAccess || src is MemoryStream) return src;
            MemoryStream ms = new(new byte[src.Length]);
            src.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Get read-only stream
        /// </summary>
        /// <param name="path">Path to open</param>
        /// <param name="fileMode">File open mode</param>
        /// <param name="fileShare">File sharing mode</param>
        /// <returns>Stream</returns>
        protected abstract Stream OpenReadImpl(string path, FileMode fileMode = FileMode.Open, FileShare fileShare =
            FileShare.ReadWrite | FileShare.Delete);

        /// <summary>
        /// Get write-only stream
        /// </summary>
        /// <param name="path">Path to open</param>
        /// <param name="fileMode">File open mode</param>
        /// <param name="fileShare">File sharing mode</param>
        /// <returns>Stream</returns>
        public abstract Stream OpenWrite(string path, FileMode fileMode = FileMode.Create,
            FileShare fileShare = FileShare.ReadWrite | FileShare.Delete);

        /// <summary>
        /// Enumerate files in a directory
        /// </summary>
        /// <param name="path">Directory to enumerate</param>
        /// <returns>Files in directory</returns>
        public abstract IEnumerable<string> EnumerateFiles(string path);

        /// <summary>
        /// Enumerate directories in a directory
        /// </summary>
        /// <param name="path">Directory to enumerate</param>
        /// <returns>Directories in directory</returns>
        public abstract IEnumerable<string> EnumerateDirectories(string path);

        /// <summary>
        /// Create directory (including parents)
        /// </summary>
        /// <param name="path">Directory to create</param>
        /// <returns>True if succeeded</returns>
        public abstract bool CreateDirectory(string path);

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="path">File path to check</param>
        /// <returns>True if exists</returns>
        public abstract bool FileExists(string path);


        /// <summary>
        /// Check if directory exists
        /// </summary>
        /// <param name="path">Directory path to check</param>
        /// <returns>True if exists</returns>
        public abstract bool DirectoryExists(string path);

        private class RealFileSystemSource : FileSystemSource
        {
            protected override Stream OpenReadImpl(string path, FileMode fileMode = FileMode.Open,
                FileShare fileShare = FileShare.ReadWrite | FileShare.Delete)
                => new FileStream(path, fileMode, FileAccess.Read, fileShare);

            public override Stream OpenWrite(string path, FileMode fileMode = FileMode.Create,
                FileShare fileShare = FileShare.ReadWrite | FileShare.Delete)
                => new FileStream(path, fileMode, FileAccess.Write, fileShare);

            public override IEnumerable<string> EnumerateFiles(string path)
                => Directory.EnumerateFiles(path);

            public override IEnumerable<string> EnumerateDirectories(string path)
                => Directory.EnumerateDirectories(path);

            public override bool CreateDirectory(string path)
                => Directory.CreateDirectory(path).Exists;

            public override bool FileExists(string path)
                => File.Exists(path);

            public override bool DirectoryExists(string path)
                => Directory.Exists(path);

            public override string NormalizePath(string path) => Path.GetFullPath(path);
        }

        internal class SegmentedFileSystemSource : FileSystemSource,
            IEnumerable<(string path, Stream stream)>
        {
            private readonly FileSystemSource _source;
            private readonly Dictionary<string, Stream> _outputEntries;
            private readonly HashSet<string> _dirs;
            private readonly bool _proxyWrites;

            internal SegmentedFileSystemSource(FileSystemSource source, bool proxyWrites,
                IEnumerable<BufferData<byte>>? existingEntries = null)
            {
                _source = source;
                _proxyWrites = proxyWrites;
                _outputEntries = new Dictionary<string, Stream>();
                _dirs = new HashSet<string>();
                if (existingEntries == null) return;
                foreach (var existingEntry in existingEntries)
                {
                    string path = existingEntry.BasePath.NormalizeAndStripWindowsDrive();
                    _dirs.Add(Path.GetDirectoryName(path) ?? Path.GetFullPath("/"));
                    _outputEntries.Add(path, new MStream(existingEntry.Buffer));
                }
            }

            protected override Stream OpenReadImpl(string path, FileMode fileMode = FileMode.Open,
                FileShare fileShare = FileShare.Delete | FileShare.None | FileShare.Read | FileShare.ReadWrite |
                                      FileShare.Write)
            {
                path = NormalizePath(path);
                if (_source.FileExists(path))
                    return _source.OpenRead(path, fileMode, fileShare);
                if (_outputEntries.TryGetValue(path, out var stream))
                    return stream;
                throw new FileNotFoundException();
            }

            public override Stream OpenWrite(string path, FileMode fileMode = FileMode.Create,
                FileShare fileShare = FileShare.Delete | FileShare.None | FileShare.Read | FileShare.ReadWrite |
                                      FileShare.Write)
            {
                if (_proxyWrites) return _source.OpenWrite(path, fileMode, fileShare);
                path = NormalizePath(path);
                MemoryStream stream = new();
                _outputEntries.Add(path, stream);
                _dirs.Add(Path.GetDirectoryName(path) ?? Path.GetFullPath("/"));
                return stream;
            }

            public override IEnumerable<string> EnumerateFiles(string path)
                => _source.EnumerateFiles(NormalizePath(path));

            public override IEnumerable<string> EnumerateDirectories(string path)
                => _source.EnumerateDirectories(NormalizePath(path));

            public override bool CreateDirectory(string path)
            {
                _dirs.Add(NormalizePath(path));
                return true;
            }

            public override bool FileExists(string path)
            {
                path = NormalizePath(path);
                return _source.FileExists(path) || _outputEntries.ContainsKey(path);
            }

            public override bool DirectoryExists(string path)
            {
                path = NormalizePath(path);
                return _source.DirectoryExists(path) || _dirs.Contains(path);
            }

            public IEnumerator<(string path, Stream stream)> GetEnumerator()
            {
                return _outputEntries.Select(kvp => (kvp.Key, kvp.Value)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override string NormalizePath(string path) => path.NormalizeAndStripWindowsDrive();
        }
    }
}
