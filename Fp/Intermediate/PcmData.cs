using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using static System.Buffers.ArrayPool<byte>;

namespace Fp.Intermediate
{
    /// <summary>
    /// PCM audio data
    /// </summary>
    public class PcmData : Data
    {
        private static readonly byte[] _chunkNames =
        {
            0x52, 0x49, 0x46, 0x46, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x64, 0x20, 0x64, 0x61, 0x74, 0x61
        };

        /// <summary>
        /// PCM metadata
        /// </summary>
        public readonly PcmInfo PcmInfo;

        /// <summary>
        /// PCM data
        /// </summary>
        public readonly ArraySegment<byte>? Samples;

        /// <summary>
        /// Create new instance of <see cref="PcmData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="pcmInfo"></param>
        public PcmData(string basePath, PcmInfo pcmInfo) : base(basePath)
        {
            Dry = true;
            PcmInfo = pcmInfo;
            Samples = null;
        }

        /// <summary>
        /// Create new instance of <see cref="PcmData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="pcmInfo">PCM metadata</param>
        /// <param name="samples">PCM data, or null for dry</param>
        public PcmData(string basePath, PcmInfo pcmInfo, ArraySegment<byte>? samples = null) : base(basePath)
        {
            Dry = !samples.HasValue;
            PcmInfo = pcmInfo;
            Samples = samples;
        }

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.PcmWave;

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            switch (format)
            {
                case CommonFormat.PcmWave:
                    WritePcmWave(outputStream, PcmInfo, Samples!.Value);
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override Data GetCompact(bool requireNew = false)
        {
            bool paramCompact = PcmInfo.ExtraParams.HasValue &&
                                IntermediateUtil.IsCompactSegment(PcmInfo.ExtraParams.Value);
            bool sampleCompact = Samples.HasValue && IntermediateUtil.IsCompactSegment(Samples.Value);
            if (!requireNew && paramCompact && sampleCompact)
                return this;
            var info = PcmInfo;
            if (info.ExtraParams.HasValue && (requireNew || !paramCompact))
                info.ExtraParams = IntermediateUtil.CopySegment(info.ExtraParams.Value);
            var samples = Samples;
            if (Samples.HasValue && (requireNew || !sampleCompact))
                samples = IntermediateUtil.CopySegment(Samples.Value);
            return new PcmData(BasePath, info, samples);
        }


        // http://soundfile.sapp.org/doc/WaveFormat/
        private static void WritePcmWave(Stream outputStream, PcmInfo pcmInfo, Span<byte> data)
        {
            int hLen = 12 + 8 + pcmInfo.SubChunk1Size + 8;
            byte[] buffer = Shared.Rent(hLen);
            var bufferSpan = buffer.AsSpan(0, hLen);
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
                    pcmInfo.ExtraParams?.AsSpan(0, pcmInfo.ExtraParamSize).CopyTo(bufferSpan.Slice(0x26));
                }

                // data (subchunk2)
                int dataPos = 12 + 8 + pcmInfo.SubChunk1Size;
                _chunkNames.AsSpan(0xC, 4).CopyTo(bufferSpan.Slice(dataPos));
                BinaryPrimitives.WriteInt32LittleEndian(bufferSpan.Slice(dataPos + 4), pcmInfo.SubChunk2Size);

                outputStream.Write(buffer, 0, hLen);
            }
            finally
            {
                Shared.Return(buffer);
            }

            buffer = Shared.Rent(4096);
            try
            {
                Processor.WriteBaseSpan(outputStream, data);
            }
            finally
            {
                Shared.Return(buffer);
            }
        }
    }
}
