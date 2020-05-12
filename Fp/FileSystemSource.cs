using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fp {
    /// <summary>
    /// Object for I/O to some filesystem provider
    /// </summary>
    public abstract class FileSystemSource {
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
        /// Get seekable read-only stream
        /// </summary>
        /// <param name="path">Path to open</param>
        /// <param name="fileMode">File open mode</param>
        /// <param name="fileShare">File sharing mode</param>
        /// <returns>Stream</returns>
        public Stream OpenRead(string path, FileMode fileMode = FileMode.Open, FileShare fileShare =
            FileShare.ReadWrite | FileShare.Delete) {
            var src = Processor.GetSeekableStream(OpenReadImpl(path, fileMode, fileShare));
            if (!ParallelAccess || src is MemoryStream) return src;
            var ms = new MemoryStream(new byte[src.Length]);
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

        private class RealFileSystemSource : FileSystemSource {
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
        }

        internal class SegmentedFileSystemSource : FileSystemSource,
            IEnumerable<(string path, byte[] buffer, int offset, int length)> {
            private readonly FileSystemSource _source;
            private readonly List<(string, MemoryStream)> _outputEntries;
            private readonly HashSet<string> _dirs;

            internal SegmentedFileSystemSource(FileSystemSource source) {
                _source = source;
                _outputEntries = new List<(string, MemoryStream)>();
                _dirs = new HashSet<string>();
            }

            protected override Stream OpenReadImpl(string path, FileMode fileMode = FileMode.Open,
                FileShare fileShare = FileShare.Delete | FileShare.None | FileShare.Read | FileShare.ReadWrite |
                                      FileShare.Write)
                => _source.OpenRead(path, fileMode, fileShare);

            public override Stream OpenWrite(string path, FileMode fileMode = FileMode.Create,
                FileShare fileShare = FileShare.Delete | FileShare.None | FileShare.Read | FileShare.ReadWrite |
                                      FileShare.Write) {
                var stream = new MemoryStream();
                _outputEntries.Add((path, stream));
                _dirs.Add(Path.GetDirectoryName(path));
                return stream;
            }

            public override IEnumerable<string> EnumerateFiles(string path)
                => _source.EnumerateFiles(path);

            public override IEnumerable<string> EnumerateDirectories(string path)
                => _source.EnumerateDirectories(path);

            public override bool CreateDirectory(string path) {
                _dirs.Add(path);
                return true;
            }

            public override bool FileExists(string path)
                => _source.FileExists(path) || _outputEntries.Any(x =>
                    string.Equals(x.Item1, path, StringComparison.InvariantCultureIgnoreCase));

            public override bool DirectoryExists(string path)
                => _source.DirectoryExists(path) || _dirs.Any(x =>
                    string.Equals(x, path, StringComparison.InvariantCultureIgnoreCase));

            public IEnumerator<(string path, byte[] buffer, int offset, int length)> GetEnumerator() {
                foreach (var (path, stream) in _outputEntries)
                    yield return (path, stream.GetBuffer(), 0, (int) stream.Length);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
    }
}