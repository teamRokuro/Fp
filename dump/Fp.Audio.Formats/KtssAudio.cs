using System.Collections.Generic;
using Fp.Intermediate;

namespace Fp.Audio.Formats
{
    /// <summary>
    ///
    /// </summary>
    public class KtssAudio : BaseAudio
    {
        /// <inheritdoc />
        protected override IEnumerable<Data> ProcessSegmentedImpl()
        {
            if (!HasExtension(".kns", ".ktss")) yield break;
            OpenFile();
            if (!HasMagic("KTSS")) yield break;
            LittleEndian = true;
            byte codec = ReadU8(0x20);
            byte version = ReadU8(0x22);
            int startOffset = ReadS32(0x24) + 0x20;
            byte numLayers = ReadU8(0x28);
            int channelCount = ReadU8(0x29) * numLayers;
            int sampleRate = ReadS32(0x2c);
            int numSamples = ReadS32(0x30);
            int loopStart = ReadS32(0x34);
            int loopLength = ReadS32(0x38);
            bool loop = loopLength > 0;
        }
    }
}
