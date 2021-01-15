using System;
using System.IO;
using Fp.Intermediate;
using vgmstream;

// ReSharper disable once CheckNamespace
namespace Fp
{
    /// <summary>
    /// Vgmstream utility methods.
    /// </summary>
    public static class VgmstreamUtil
    {
        /// <summary>
        /// Export PCM data.
        /// </summary>
        /// <param name="vgm">VGMStream object to use.</param>
        /// <param name="onlyStereo">If not -1, 0-based stereo channels to export.</param>
        /// <returns></returns>
        public static PcmData ToPcmData(this VGMStream vgm, int onlyStereo = -1)
        {
            return new(Path.ChangeExtension(vgm.Name, ".wav"), vgm.GetPcmInfo(onlyStereo).AsPcmInfo(),
                vgm.Export(false, true, onlyStereo));
        }

        /// <summary>
        /// Convert to <see cref="PcmInfo"/>.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Converted value.</returns>
        public static PcmInfo AsPcmInfo(this PCMInfo b) =>
            new(b.numChannels, b.sampleRate, b.bitsPerSample, b.numSamples);

        /// <summary>
        /// Convert to <see cref="PCMInfo"/>.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Converted value.</returns>
        public static PCMInfo AsPCMInfo(this PcmInfo b) => new()
        {
            numChannels = b.NumChannels,
            sampleRate = b.SampleRate,
            bitsPerSample = b.BitsPerSample,
            numSamples = b.NumSamples
        };

        /// <summary>
        /// Attempts to load PCM audio from VGMStream conversion.
        /// </summary>
        /// <param name="path">Data path.</param>
        /// <param name="memory">Memory to use.</param>
        /// <param name="data">Converted data or null.</param>
        /// <param name="streamIndex">Stream index (1-based).</param>
        /// <param name="onlyStereo">If not -1, 0-based stereo channels to export.</param>
        /// <param name="txth">TXTH configuration.</param>
        /// <returns>True if successful.</returns>
        public static bool TryVgmstream(string path, ReadOnlyMemory<byte> memory, out PcmData? data,
            int streamIndex = 0, int onlyStereo = -1, TXTHHeader? txth = null)
        {
            using var vgm = VGMStream.Load(path, memory, streamIndex, txth);
            return (data = vgm?.ToPcmData(onlyStereo)) != null;
        }

    }
}
