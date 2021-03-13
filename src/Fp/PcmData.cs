using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace Fp
{
    /// <summary>
    /// PCM audio data
    /// </summary>
    public class PcmData : BufferData<byte>
    {
        private static readonly byte[] _chunkNames =
        {
            0x52, 0x49, 0x46, 0x46, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20, 0x64, 0x61, 0x74, 0x61
        };

        /// <summary>
        /// PCM metadata
        /// </summary>
        public readonly PcmInfo PcmInfo;

        private bool _disposed;

        /// <summary>
        /// Create new instance of <see cref="PcmData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="pcmInfo"></param>
        public PcmData(string basePath, PcmInfo pcmInfo) : base(basePath, pcmInfo.SubChunk2Size)
        {
            Dry = true;
            PcmInfo = pcmInfo;
        }

        /// <summary>
        /// Create new instance of <see cref="PcmData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="pcmInfo">PCM metadata</param>
        /// <param name="memoryOwner">Owner of PCM data buffer</param>
        /// <param name="count">Length of content</param>
        public PcmData(string basePath, PcmInfo pcmInfo, IMemoryOwner<byte> memoryOwner,
            int? count = default) : base(basePath, memoryOwner, count)
        {
            PcmInfo = pcmInfo;
        }

        /// <summary>
        /// Create new instance of <see cref="PcmData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="pcmInfo">PCM metadata</param>
        /// <param name="buffer">PCM data</param>
        public PcmData(string basePath, PcmInfo pcmInfo, ReadOnlyMemory<byte> buffer) : base(basePath, buffer)
        {
            PcmInfo = pcmInfo;
        }

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.PcmWave;

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<object, object>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            if (_disposed)
                throw new ObjectDisposedException(nameof(PcmData));
            switch (format)
            {
                case CommonFormat.PcmWave:
                    WritePcmWave(outputStream, PcmInfo, Buffer.Span);
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override object Clone()
        {
            if (Dry)
                return new PcmData(BasePath, PcmInfo);
            if (_disposed)
                throw new ObjectDisposedException(nameof(PcmData));
            return new PcmData(BasePath, PcmInfo, DataUtil.CloneBuffer(Buffer));
        }

        // http://soundfile.sapp.org/doc/WaveFormat/
        private static void WritePcmWave(Stream outputStream, PcmInfo pcmInfo, ReadOnlySpan<byte> data)
        {
            int hLen = 12 + 8 + pcmInfo.SubChunk1Size + 8;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(hLen);
            Span<byte> bufferSpan = buffer.AsSpan(0, hLen);
            try
            {
                // RIFF (main chunk)
                _chunkNames.AsSpan(0, 4).CopyTo(bufferSpan.Slice(0));
                BinaryPrimitives.WriteInt32LittleEndian(bufferSpan.Slice(4),
                    4 + 8 + pcmInfo.SubChunk1Size + 8 + pcmInfo.SubChunk2Size);
                _chunkNames.AsSpan(4, 4).CopyTo(bufferSpan.Slice(8));
                // fmt (subchunk1)
                _chunkNames.AsSpan(8, 4).CopyTo(bufferSpan.Slice(0xC));
                BinaryPrimitives.WriteInt32LittleEndian(bufferSpan.Slice(0x10), pcmInfo.SubChunk1Size);
                BinaryPrimitives.WriteInt16LittleEndian(bufferSpan.Slice(0x14), pcmInfo.AudioFormat);
                BinaryPrimitives.WriteInt16LittleEndian(bufferSpan.Slice(0x16), pcmInfo.NumChannels);
                BinaryPrimitives.WriteInt32LittleEndian(bufferSpan.Slice(0x18), pcmInfo.SampleRate);
                BinaryPrimitives.WriteInt32LittleEndian(bufferSpan.Slice(0x1C), pcmInfo.ByteRate);
                BinaryPrimitives.WriteInt16LittleEndian(bufferSpan.Slice(0x20), pcmInfo.BlockAlign);
                BinaryPrimitives.WriteInt16LittleEndian(bufferSpan.Slice(0x22), pcmInfo.BitsPerSample);
                if (pcmInfo.SubChunk1Size != 0x10)
                {
                    BinaryPrimitives.WriteInt16LittleEndian(bufferSpan.Slice(0x24), pcmInfo.ExtraParamSize);
                    pcmInfo.ExtraParams?.Span.Slice(0, pcmInfo.ExtraParamSize).CopyTo(bufferSpan.Slice(0x26));
                }

                // data (subchunk2)
                int dataPos = 12 + 8 + pcmInfo.SubChunk1Size;
                _chunkNames.AsSpan(0xC, 4).CopyTo(bufferSpan.Slice(dataPos));
                BinaryPrimitives.WriteInt32LittleEndian(bufferSpan.Slice(dataPos + 4), pcmInfo.SubChunk2Size);

                outputStream.Write(buffer, 0, hLen);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            Processor.WriteBaseSpan(outputStream, data);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override string ToString() => $"PCM {{ Path = {BasePath}, Buffer Length = {Count}, Info = {PcmInfo} }}";
    }

    public partial class Processor
    {
        /// <summary>
        /// Creates PCM audio data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="info">PCM information.</param>
        /// <param name="memory">Audio buffer.</param>
        /// <returns>Data object.</returns>
        public static PcmData Audio(FpPath path, PcmInfo info, ReadOnlyMemory<byte> memory) =>
            new(path.AsCombined(), info, memory);

        /// <summary>
        /// Creates PCM audio data object.
        /// </summary>
        /// <param name="name">Path.</param>
        /// <param name="info">PCM information.</param>
        /// <param name="memory">Audio buffer.</param>
        /// <returns>Data object.</returns>
        public static PcmData Audio(string name, PcmInfo info, ReadOnlyMemory<byte> memory) =>
            new(name, info, memory);
    }

    public partial class Scripting
    {
        /// <summary>
        /// Creates PCM audio data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="info">PCM information.</param>
        /// <param name="memory">Audio buffer.</param>
        /// <returns>Data object.</returns>
        public static Data audio(this FpPath path, PcmInfo info, ReadOnlyMemory<byte> memory) =>
            Processor.Audio(path, info, memory);

        /// <summary>
        /// Creates PCM audio data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="info">PCM information.</param>
        /// <param name="memory">Audio buffer.</param>
        /// <returns>Data object.</returns>
        public static Data audio(this string path, PcmInfo info, ReadOnlyMemory<byte> memory) =>
            Processor.Audio(path, info, memory);
    }
}
