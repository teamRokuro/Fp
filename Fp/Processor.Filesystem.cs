using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class Processor
    {
        #region Filesystem utilities

        /// <summary>
        /// Get seekable stream (closes base stream if replaced)
        /// </summary>
        /// <param name="stream">Base stream</param>
        /// <returns>Seekable stream</returns>
        /// <remarks>
        /// This method conditionally creates a seekable stream from a non-seekable stream by copying the
        /// stream's contents to a new <see cref="MemoryStream"/> instance. The returned object is either
        /// this newly created stream or the passed argument <paramref name="stream"/> if it was already seekable
        /// </remarks>
        public static Stream GetSeekableStream(Stream stream)
        {
            if (stream.CanSeek)
            {
                return stream;
            }

            MemoryStream ms;
            try
            {
                long length = stream.Length;
                ms = length > int.MaxValue ? new MemoryStream() : new MemoryStream(new byte[length]);
            }
            catch
            {
                ms = new MemoryStream();
            }

            stream.CopyTo(ms);
            stream.Close();
            ms.Position = 0;
            stream = ms;
            return stream;
        }

        /// <summary>
        /// Open file for reading
        /// </summary>
        /// <param name="asMain">If true, sets <see cref="InputStream"/></param>
        /// <param name="file">File to open, <see cref="InputFile"/> by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenFile(bool asMain, string? file = null)
        {
            if (FileSystem == null)
            {
                throw new InvalidOperationException();
            }

            file ??= InputFile ?? throw new InvalidOperationException();
            file = Path.Combine(InputDirectory ?? throw new InvalidOperationException(), file);
            if (!FileSystem.FileExists(file))
            {
                throw new FileNotFoundException("File not found", file);
            }

            Stream stream = FileSystem.OpenRead(file);
            if (Preload && (!(stream is MemoryStream alreadyMs) || !alreadyMs.TryGetBuffer(out _)))
            {
                MemoryStream ms = new(new byte[stream.Length]);
                stream.CopyTo(ms);
                stream.Dispose();
                stream = ms;
            }

            if (stream is FileStream)
                stream = new MultiBufferStream(stream, true);

            if (!asMain)
            {
                return stream;
            }

            InputStream?.Dispose();
            InputStream = stream;
            InputLength = InputStream.Length;

            return stream;
        }

        /// <summary>
        /// Open file for reading and set <see cref="InputStream"/>
        /// </summary>
        /// <param name="file">File to open, <see cref="InputFile"/> by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenFile(string? file = null)
            => OpenFile(true, file);

        /// <summary>
        /// Open file in <see cref="InputDirectory"/> for reading
        /// </summary>
        /// <param name="name">File to open</param>
        /// <returns>Created stream</returns>
        public Stream OpenSibling(string name)
            => OpenFile(false, name);

        /// <summary>
        /// Close stream
        /// </summary>
        /// <param name="asMain">If true, clear <see cref="InputStream"/></param>
        /// <param name="stream">Stream to close</param>
        public void CloseFile(bool asMain, Stream? stream = null)
        {
            stream ??= InputStream;
            stream?.Dispose();
            if (asMain)
            {
                InputStream = null;
            }
        }

        /// <summary>
        /// Close stream and clear <see cref="InputStream"/>
        /// </summary>
        /// <param name="stream">Stream to close</param>
        public void CloseFile(Stream? stream = null)
            => CloseFile(true, stream);

        private Stream OpenOutputFileInternal(bool sub, bool asMain, string? extension = null,
            string? filename = null)
        {
            if (FileSystem == null)
            {
                throw new InvalidOperationException();
            }

            filename = sub ? GenPathSub(extension, filename) : GenPath(extension, filename);
            Stream stream = FileSystem.OpenWrite(filename);
            if (!asMain)
            {
                return stream;
            }

            OutputStream?.Dispose();
            OutputStream = stream;

            return stream;
        }

        /// <summary>
        /// Open file for writing
        /// </summary>
        /// <param name="asMain">If true, sets <see cref="OutputStream"/></param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputFile(bool asMain, string? extension = null, string? filename = null)
            => OpenOutputFileInternal(false, asMain, extension, filename);

        /// <summary>
        /// Open file for writing and set <see cref="OutputStream"/>
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputFile(string? extension = null, string? filename = null)
            => OpenOutputFileInternal(false, true, extension, filename);

        /// <summary>
        /// Open file for writing under folder named by current file's name
        /// </summary>
        /// <param name="asMain">If true, sets <see cref="OutputStream"/></param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputSubFile(bool asMain, string? extension = null, string? filename = null)
            => OpenOutputFileInternal(true, asMain, extension, filename);

        /// <summary>
        /// Open file for writing under folder named by current file's name and set <see cref="OutputStream"/>
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputSubFile(string? extension = null, string? filename = null)
            => OpenOutputFileInternal(true, true, extension, filename);

        /// <summary>
        /// Close stream
        /// </summary>
        /// <param name="asMain">If true, clear <see cref="OutputStream"/></param>
        /// <param name="stream">Stream to close</param>
        public void CloseOutputFile(bool asMain, Stream? stream = null)
        {
            stream ??= OutputStream;
            stream?.Dispose();
            if (asMain)
            {
                OutputStream = null;
            }
        }

        /// <summary>
        /// Close stream and clear <see cref="OutputStream"/>
        /// </summary>
        /// <param name="stream">Stream to close</param>
        public void CloseOutputFile(Stream? stream = null)
            => CloseOutputFile(true, stream);

        /// <summary>
        /// Make directories above path
        /// </summary>
        /// <param name="file">Path to make parents for</param>
        /// <exception cref="IOException"> when failed to create directories</exception>
        public void MkParents(string file) => MkDirs(Path.GetDirectoryName(file));

        /// <summary>
        /// Make directories up to path
        /// </summary>
        /// <param name="dir">Path to make directories to</param>
        /// <exception cref="IOException"> when failed to create directories</exception>
        public void MkDirs(string? dir = null)
        {
            if (FileSystem == null)
            {
                throw new InvalidOperationException();
            }

            dir ??= OutputDirectory ?? throw new InvalidOperationException();
            if (!FileSystem.CreateDirectory(dir))
            {
                throw new IOException($"Failed to create target directory {dir}");
            }
        }

        /// <summary>
        /// Generate native path by replacing \ and / with native separators
        /// </summary>
        /// <param name="path">Path to change</param>
        /// <returns>Native OS compatible path</returns>
        public string MakeNative(string path)
            => path.Replace('\\', Path.AltDirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        // Change to Normalize, kill ./.. traversal

        /// <summary>
        /// Generate path under folder named by specified main file's name
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name</param>
        /// <param name="mainFile">File to use for folder name and file name (if <paramref name="filename"/> not specified)</param>
        /// <param name="mkDirs">If true, create directories in filesystem</param>
        /// <returns>Generated path</returns>
        public string GenPathSub(string? extension = null, string? filename = null, string? mainFile = null,
            bool mkDirs = true)
        {
            if (OutputDirectory == null)
            {
                throw new InvalidOperationException();
            }

            mainFile ??= InputFile ?? throw new InvalidOperationException();
            filename = filename == null
                ? $"{Path.GetFileNameWithoutExtension(mainFile)}_{OutputCounter++:D8}{extension}"
                : $"{filename}{extension}";
            string path = Join(SupportBackSlash, OutputDirectory,
                Path.GetFileName(mainFile) ?? throw new ProcessorException($"Null {nameof(mainFile)} provided"),
                filename);
            if (mkDirs)
            {
                MkParents(path);
            }

            return path;
        }

        /// <summary>
        /// Generate path
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name</param>
        /// <param name="directory">Main output directory</param>
        /// <param name="mainFile">File to use for file name (if <paramref name="filename"/> not specified)</param>
        /// <param name="mkDirs">If true, create directories in filesystem</param>
        /// <returns>Generated path</returns>
        public string GenPath(string? extension = null, string? filename = null, string? directory = null,
            string? mainFile = null, bool mkDirs = true)
        {
            if (OutputDirectory == null)
            {
                throw new InvalidOperationException();
            }

            mainFile ??= InputFile ?? throw new InvalidOperationException();
            filename = filename == null
                ? $"{Path.GetFileNameWithoutExtension(mainFile)}_{OutputCounter++:D8}{extension}"
                : $"{filename}{extension}";
            string path = Path.Combine(OutputDirectory, directory ?? string.Empty, filename);
            if (mkDirs)
            {
                MkParents(path);
            }

            return path;
        }

        /// <summary>
        /// Create path from components
        /// </summary>
        /// <param name="paths">Elements to join</param>
        /// <returns>Path</returns>
        /// <exception cref="Exception">If separator is encountered by itself</exception>
        public string Join(params string[] paths)
            => Join(SupportBackSlash, paths);

        /// <summary>
        /// Create path from components
        /// </summary>
        /// <param name="supportBackSlash">Whether to allow backslashes as separators</param>
        /// <param name="paths">Elements to join</param>
        /// <returns>Path</returns>
        /// <exception cref="ProcessorException">If separator is encountered by itself</exception>
        public static unsafe string Join(bool supportBackSlash, params string[] paths)
        {
            if (paths.Length < 2)
            {
                return paths.Length == 0 ? string.Empty : paths[0] ?? throw new ArgumentException("Null element");
            }

            int capacity = paths.Sum(path => (path ?? throw new ArgumentException("Null element")).Length);
            char[] buf = ArrayPool<char>.Shared.Rent(capacity + paths.Length - 1);
            try
            {
                Span<char> bufSpan = buf.AsSpan();
                int cIdx = 0;
                bool prevEndWithSeparator = false;
                foreach (string path in paths)
                {
                    int pathLength = path.Length;
                    if (pathLength == 0)
                    {
                        continue;
                    }

                    ReadOnlySpan<char> span = path.AsSpan();
                    char first = span[0];
                    if (first == '/' || supportBackSlash && first == '\\')
                    {
                        if (pathLength == 1)
                        {
                            throw new ProcessorException("Joining single-character separator disallowed");
                        }

                        if (prevEndWithSeparator)
                        {
                            span = span.Slice(1, --pathLength);
                        }
                    }
                    else if (cIdx != 0 && !prevEndWithSeparator)
                    {
                        bufSpan[cIdx++] = supportBackSlash ? '\\' : '/';
                    }

                    span.CopyTo(bufSpan.Slice(cIdx));
                    cIdx += span.Length;
                    char last = span[pathLength - 1];
                    prevEndWithSeparator = last == '/' || supportBackSlash && last == '\\';
                }

                fixed (char* ptr = &bufSpan.GetPinnableReference())
                {
                    return new string(ptr, 0, cIdx);
                }
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buf);
            }
        }

        #endregion
    }
}
