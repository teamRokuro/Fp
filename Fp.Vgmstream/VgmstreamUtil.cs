using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        /// <returns>Exported PCM data.</returns>
        public static PcmData ToPcmData(this VGMStream vgm, int onlyStereo = -1)
        {
            return new(Path.ChangeExtension(vgm.Name, $"_{vgm.Data.stream_index:D6}.wav"),
                vgm.GetPcmInfo(onlyStereo).AsPcmInfo(), vgm.Export(false, true, onlyStereo));
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
        private static bool TryGetVgmstreams(string path, ReadOnlyMemory<byte> memory, out List<VGMStream>? streams,
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

            int count = Math.Max(vgm.Data.num_streams, 1);
            streams = new List<VGMStream>();
            for (int i = 0; i < count; i++)
            {
                var entry = VGMStream.Load(path, memory, i + 1, txth);
                if (entry != null) streams.Add(entry);
            }

            return true;
        }

        /// <summary>
        /// Gets vgmstream content or input stream as generic buffer.
        /// </summary>
        /// <param name="path">Data path.</param>
        /// <param name="memory">Memory to use.</param>
        /// <returns>Generated inputs.</returns>
        public static IEnumerable<Data> GetVgmstreamsOrDefault(string path, ReadOnlyMemory<byte> memory)
        {
            if (!TryGetVgmstreams(path, memory, out var streams))
                return new[] {new BufferData<byte>(path, memory)};
            return streams!.Select(s =>
            {
                var mm = new XMemoryManager(VGMStreamAPI.wrap_get_pcm_length(s.VgmstreamPtr, -1));
                VGMStreamAPI.wrap_basic_export_pcm(s.VgmstreamPtr, -1, mm.Ptr, mm.Length);
                var data = s.Data;
                var res = new PcmData(Path.ChangeExtension(s.Name, $"_{data.stream_index:D6}.wav"),
                    s.GetPcmInfo().AsPcmInfo(), mm.Memory);
                s.Dispose();
                return res;
            });
        }

        private class XMemoryManager : MemoryManager<byte>
        {
            public nint Ptr { get; private set; }
            public int Length { get; }

            public XMemoryManager(int length)
            {
                Ptr = Marshal.AllocHGlobal(length);
                Length = length;
            }

            public override unsafe Span<byte> GetSpan() => new((void*)Ptr, Length);

            public override unsafe MemoryHandle Pin(int elementIndex = 0) => new((void*)Ptr);

            public override void Unpin()
            {
            }

            protected override void Dispose(bool disposing)
            {
                if (Ptr != 0) Marshal.FreeHGlobal(Ptr);
                Ptr = (nint)0;
            }
        }
    }
}
