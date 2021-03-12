using System;

namespace Fp
{
    /// <summary>
    /// PCM metadata
    /// </summary>
    public record PcmInfo
    {
        /// <summary>
        /// 16 for PCM.  This is the size of the
        /// rest of the Subchunk which follows this number.
        /// </summary>
        public int SubChunk1Size { get; init; }

        /// <summary>
        /// PCM = 1 (i.e. Linear quantization)
        /// Values other than 1 indicate some
        /// form of compression.
        /// </summary>
        public short AudioFormat { get; init; }

        /// <summary>
        /// Mono = 1, Stereo = 2, etc.
        /// </summary>
        public short NumChannels { get; init; }

        /// <summary>
        /// Number of samples.
        /// </summary>
        public int NumSamples { get; init; }

        /// <summary>
        /// 8000, 44100, etc.
        /// </summary>
        public int SampleRate { get; init; }

        /// <summary>
        /// == SampleRate * NumChannels * BitsPerSample/8
        /// </summary>
        public int ByteRate { get; init; }

        /// <summary>
        /// == NumChannels * BitsPerSample/8
        /// The number of bytes for one sample including
        /// all channels.
        /// </summary>
        public short BlockAlign { get; init; }

        /// <summary>
        /// 8 bits = 8, 16 bits = 16, etc.
        /// </summary>
        public short BitsPerSample { get; init; }

        /// <summary>
        /// if PCM, then doesn't exist
        /// </summary>
        public short ExtraParamSize { get; init; }

        /// <summary>
        /// space for extra parameters
        /// </summary>
        public ReadOnlyMemory<byte>? ExtraParams { get; init; }

        /// <summary>
        /// == NumSamples * NumChannels * BitsPerSample/8
        /// This is the number of bytes in the data.
        /// You can also think of this as the size
        /// of the read of the subchunk following this
        /// number.
        /// </summary>
        public int SubChunk2Size { get; init; }

        /// <summary>
        /// Create new instance of <see cref="PcmInfo"/>
        /// </summary>
        /// <param name="subChunk1Size">16 for PCM.  This is the size of the
        /// rest of the Subchunk which follows this number.</param>
        /// <param name="audioFormat">PCM = 1 (i.e. Linear quantization)
        /// Values other than 1 indicate some
        /// form of compression.</param>
        /// <param name="numChannels">Mono = 1, Stereo = 2, etc.</param>
        /// <param name="sampleRate">8000, 44100, etc.</param>
        /// <param name="byteRate">== SampleRate * NumChannels * BitsPerSample/8</param>
        /// <param name="blockAlign">== NumChannels * BitsPerSample/8
        /// The number of bytes for one sample including
        /// all channels.</param>
        /// <param name="bitsPerSample">8 bits = 8, 16 bits = 16, etc.</param>
        /// <param name="extraParamSize">if PCM, then doesn't exist</param>
        /// <param name="extraParams">space for extra parameters</param>
        /// <param name="subChunk2Size">== NumSamples * NumChannels * BitsPerSample/8
        /// This is the number of bytes in the data.
        /// You can also think of this as the size
        /// of the read of the subchunk following this
        /// number.</param>
        public PcmInfo(int subChunk1Size, short audioFormat, short numChannels, int sampleRate, int byteRate,
            short blockAlign, short bitsPerSample, short extraParamSize, Memory<byte>? extraParams,
            int subChunk2Size)
        {
            SubChunk1Size = subChunk1Size;
            AudioFormat = audioFormat;
            NumChannels = numChannels;
            NumSamples = subChunk2Size * 8 / numChannels / bitsPerSample;
            SampleRate = sampleRate;
            ByteRate = byteRate;
            BlockAlign = blockAlign;
            BitsPerSample = bitsPerSample;
            ExtraParamSize = extraParamSize;
            ExtraParams = ((ReadOnlyMemory<byte>?)extraParams)?.CloneBuffer();
            SubChunk2Size = subChunk2Size;
        }

        /// <summary>
        /// Create new instance of <see cref="PcmInfo"/>
        /// </summary>
        /// <param name="subChunk1Size">16 for PCM.  This is the size of the
        /// rest of the Subchunk which follows this number.</param>
        /// <param name="audioFormat">PCM = 1 (i.e. Linear quantization)
        /// Values other than 1 indicate some
        /// form of compression.</param>
        /// <param name="numChannels">Mono = 1, Stereo = 2, etc.</param>
        /// <param name="sampleRate">8000, 44100, etc.</param>
        /// <param name="byteRate">== SampleRate * NumChannels * BitsPerSample/8</param>
        /// <param name="blockAlign">== NumChannels * BitsPerSample/8
        /// The number of bytes for one sample including
        /// all channels.</param>
        /// <param name="bitsPerSample">8 bits = 8, 16 bits = 16, etc.</param>
        /// <param name="extraParamSize">if PCM, then doesn't exist</param>
        /// <param name="extraParams">space for extra parameters</param>
        /// <param name="subChunk2Size">== NumSamples * NumChannels * BitsPerSample/8
        /// This is the number of bytes in the data.
        /// You can also think of this as the size
        /// of the read of the subchunk following this
        /// number.</param>
        public PcmInfo(int subChunk1Size, short audioFormat, short numChannels, int sampleRate, int byteRate,
            short blockAlign, short bitsPerSample, short extraParamSize, ReadOnlyMemory<byte>? extraParams,
            int subChunk2Size)
        {
            SubChunk1Size = subChunk1Size;
            AudioFormat = audioFormat;
            NumChannels = numChannels;
            NumSamples = subChunk2Size * 8 / numChannels / bitsPerSample;
            SampleRate = sampleRate;
            ByteRate = byteRate;
            BlockAlign = blockAlign;
            BitsPerSample = bitsPerSample;
            ExtraParamSize = extraParamSize;
            ExtraParams = extraParams;
            SubChunk2Size = subChunk2Size;
        }

        /// <summary>
        /// Create new instance of <see cref="PcmInfo"/> for PCM data
        /// </summary>
        /// <param name="numChannels">Mono = 1, Stereo = 2, etc.</param>
        /// <param name="sampleRate">8000, 44100, etc.</param>
        /// <param name="bitsPerSample">8 bits = 8, 16 bits = 16, etc.</param>
        /// <param name="numSamples">Number of samples (shared count between channels)</param>
        public PcmInfo(short numChannels, int sampleRate, short bitsPerSample, int numSamples)
        {
            SubChunk1Size = 0x10;
            AudioFormat = 1;
            NumChannels = numChannels;
            NumSamples = numSamples;
            SampleRate = sampleRate;
            ByteRate = sampleRate * numChannels * bitsPerSample / 8;
            BlockAlign = (short)(numChannels * bitsPerSample / 8);
            BitsPerSample = bitsPerSample;
            ExtraParamSize = 0;
            ExtraParams = null;
            SubChunk2Size = numSamples * numChannels * bitsPerSample / 8;
        }
    }
}
