using System.Diagnostics.CodeAnalysis;

#pragma warning disable 1591

namespace Fp.Audio
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public struct PlayState
    {
        public int InputChannels { get; set; }
        public int OutputChannels { get; set; }

        public int PadBeginDuration { get; set; }
        public int PadBeginLeft { get; set; }
        public int TrimBeginDuration { get; set; }
        public int TrimBeginLeft { get; set; }
        public int BodyDuration { get; set; }
        public int FadeDuration { get; set; }
        public int FadeLeft { get; set; }
        public int FadeStart { get; set; }

        public int PadEndDuration { get; set; }

        //int32_t pad_end_left;
        public int PadEndStart { get; set; }

        /// <summary>
        /// total samples that the stream lasts (after applying all config)
        /// </summary>
        public int PlayDuration { get; set; }

        /// <summary>
        /// absolute sample where stream is
        /// </summary>
        public int PlayPosition { get; set; }
    }
}
