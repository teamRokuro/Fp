using System;
using System.Collections.Generic;
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
        /// Convert to <see cref="Intermediate.PcmInfo"/>.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Converted value.</returns>
        public static Intermediate.PcmInfo AsPcmInfo(this vgmstream.PcmInfo b) =>
            new(b.numChannels, b.sampleRate, b.bitsPerSample, b.numSamples);

        /// <summary>
        /// Convert to <see cref="vgmstream.PcmInfo"/>.
        /// </summary>
        /// <param name="b">Value to convert.</param>
        /// <returns>Converted value.</returns>
        public static vgmstream.PcmInfo AsPCMInfo(this Intermediate.PcmInfo b) => new()
        {
            numChannels = b.NumChannels,
            sampleRate = b.SampleRate,
            bitsPerSample = b.BitsPerSample,
            numSamples = b.NumSamples
        };

        /// <summary>
        /// Attempts to load streams from vgmstream.
        /// </summary>
        /// <param name="path">Data path.</param>
        /// <param name="memory">Memory to use.</param>
        /// <param name="streams">Generated streams.</param>
        /// <param name="txth">TXTH configuration.</param>
        /// <returns>True if successful.</returns>
        public static bool TryGetVgmstreams(string path, ReadOnlyMemory<byte> memory, out List<VGMStream>? streams,
            TxthHeader? txth = null)
        {
            if (!VGMStreamAPI.IsSupported)
            {
                streams = null;
                return false;
            }

            using var vgm = VGMStream.Load(path, memory, 0, txth);
            if (vgm == null)
            {
                streams = null;
                return false;
            }

            int count = vgm.Data.num_streams;
            streams = new List<VGMStream>();
            for (int i = 0; i < count; i++)
            {
                var entry = VGMStream.Load(path, memory, i + 1, txth);
                if (entry != null) streams.Add(entry);
            }

            return true;
        }
    }
}
