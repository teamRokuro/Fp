using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace Fp.Audio
{
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public struct PlayConfig
    {
        /// <summary>
        /// some of the mods below are set
        /// </summary>
        public bool ConfigSet { get; set; }

        #region modifiers

        public bool PlayForever { get; set; }
        public bool IgnoreLoop { get; set; }
        public bool ForceLoop { get; set; }
        public bool ReallyForceLoop { get; set; }
        public bool IgnoreFade { get; set; }

        #endregion

        #region processing

        public double LoopCount { get; set; }
        public int PadBegin { get; set; }
        public int TrimBegin { get; set; }
        public int BodyTime { get; set; }
        public int TrimEnd { get; set; }
        public double FadeDelay { get; set; } /* not in samples for backwards compatibility */
        public double FadeTime { get; set; }
        public int PadEnd { get; set; }

        #endregion

        public double PadBeginS { get; set; }
        public double TrimBeginS { get; set; }
        public double BodyTimeS { get; set; }

        public double TrimEndS { get; set; }

        //double fade_delay_s;
        //double fade_time_s;
        public double PadEndS { get; set; }

        #region internal flags

        public bool PadBeginSet { get; set; }
        public bool TrimBeginSet { get; set; }
        public bool BodyTimeSet { get; set; }
        public bool LoopCountSet { get; set; }
        public bool TrimEndSet { get; set; }
        public bool FadeDelaySet { get; set; }
        public bool FadeTimeSet { get; set; }
        public bool PadEndSet { get; set; }

        #endregion

        /* for lack of a better place... */
        public bool IsTxtp { get; set; }
        public bool IsMiniTxtp { get; set; }
    }
}
