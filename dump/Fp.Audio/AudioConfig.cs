namespace Fp.Audio
{
    /// <summary>
    /// Audio configuration.
    /// </summary>
    public struct AudioConfig
    {
        #region basic config

        /// <summary>
        /// the actual max number of samples
        /// </summary>
        public int NumSamples { get; set; }

        /// <summary>
        /// sample rate in Hz
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// number of channels
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        /// type of encoding
        /// </summary>
        public Coding Coding { get; set; }

        /// <summary>
        /// type of layout
        /// </summary>
        public Layout Layout { get; set; }

        /// <summary>
        /// type of metadata
        /// </summary>
        public Format Format { get; set; }

        #endregion

        #region looping config

        /// <summary>
        /// is this stream looped?
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// first sample of the loop (included in the loop)
        /// </summary>
        public int LoopStartSample { get; set; }

        /// <summary>
        /// last sample of the loop (not included in the loop)
        /// </summary>
        public int LoopEndSample { get; set; }

        #endregion

        #region layouts/block config

        /// <summary>
        /// interleave, or block/frame size (depending on the codec)
        /// </summary>
        public uint InterleaveBlockSize { get; set; }

        /// <summary>
        /// different interleave for first block
        /// </summary>
        public uint InterleaveFirstBlockSize { get; set; }

        /// <summary>
        /// data skipped before interleave first (needed to skip other channels)
        /// </summary>
        public uint InterleaveFirstSkip { get; set; }

        /// <summary>
        /// smaller interleave for last block
        /// </summary>
        public uint InterleaveLastBlockSize { get; set; }

        /// <summary>
        /// for codecs with configurable size
        /// </summary>
        public uint FrameSize { get; set; }

        #endregion

        #region subsong config

        /// <summary>
        /// for multi-stream formats (0=not set/one stream, 1=one stream)
        /// </summary>
        public int NumStreams { get; set; }

        /// <summary>
        /// selected subsong (also 1-based)
        /// </summary>
        public int StreamIndex { get; set; }

        /// <summary>
        /// info to properly calculate bitrate in case of subsongs
        /// </summary>
        public uint StreamSize { get; set; }

        #endregion

        /// <summary>
        /// name of the current stream (info), if the file stores it and it's filled
        /// </summary>
        public string StreamName { get; set; }

        #region mapping config (info for plugins)

        #endregion

        /// <summary>
        /// order: FL FR FC LFE BL BR FLC FRC BC SL SR etc (WAVEFORMATEX flags where FL=lowest bit set)
        /// </summary>
        public uint ChannelLayout { get; set; }

        #region other config

        /// <summary>
        /// search for dual stereo (file_L.ext + file_R.ext = single stereo file)
        /// </summary>
        public bool AllowDualStereo { get; set; }

        #endregion

        #region layout/block state

        /// <summary>
        /// actual data size of an entire block (ie. may be fixed, include padding/headers, etc)
        /// </summary>
        public uint FullBlockSize { get; set; }

        /// <summary>
        /// sample point within the file (for loop detection)
        /// </summary>
        public int CurrentSample { get; set; }

        /// <summary>
        /// number of samples into the current block/interleave/segment/etc
        /// </summary>
        public int SamplesIntoBlock { get; set; }

        /// <summary>
        /// start of this block (offset of block header)
        /// </summary>
        public int CurrentBlockOffset { get; set; }

        /// <summary>
        /// size in usable bytes of the block we're in now (used to calculate num_samples per block)
        /// </summary>
        public uint CurrentBlockSize { get; set; }

        /// <summary>
        /// size in samples of the block we're in now (used over current_block_size if possible)
        /// </summary>
        public int CurrentBlockSamples { get; set; }

        /// <summary>
        /// offset of header of the next block
        /// </summary>
        public int NextBlockOffset  { get; set; }

        #endregion

        #region loop state (saved when loop is hit to restore later)

        /// <summary>
        /// saved from current_sample (same as loop_start_sample, but more state-like)
        /// </summary>
        public int LoopCurrentSample { get; set; }

        /// <summary>
        /// saved from samples_into_block
        /// </summary>
        public int LoopSamplesIntoBlock { get; set; }

        /// <summary>
        /// saved from current_block_offset
        /// </summary>
        public int LoopBlockOffset { get; set; }

        /// <summary>
        /// saved from current_block_size
        /// </summary>
        public uint LoopBlockSize { get; set; }

        /// <summary>
        /// saved from current_block_samples
        /// </summary>
        public int LoopBlockSamples { get; set; }

        /// <summary>
        /// saved from next_block_offset
        /// </summary>
        public int LoopNextBlockOffset { get; set; }

        /// <summary>
        /// save config when loop is hit, but first time only
        /// </summary>
        public int HitLoop { get; set; }

        #endregion

        #region decoder config/state

        /// <summary>
        /// little/big endian marker; name is left vague but usually means big endian
        /// </summary>
        public int CodecEndian { get; set; }

        /// <summary>
        /// flags for codecs or layouts with minor variations; meaning is up to them
        /// </summary>
        public int CodecConfig { get; set; }

        /// <summary>
        /// WS ADPCM: output bytes for this block
        /// </summary>
        public int WsOutputSize { get; set; }

        #endregion

        #region main state

        /// <summary>
        /// array of channels
        /// </summary>
        public nint ch;

        /// <summary>
        /// shallow copy of channels as they were at the beginning of the stream (for resets)
        /// </summary>
        public nint start_ch;

        /// <summary>
        /// shallow copy of channels as they were at the loop point (for loops)
        /// </summary>
        public nint loop_ch;

        /// <summary>
        /// shallow copy of the VGMSTREAM as it was at the beginning of the stream (for resets)
        /// </summary>
        public nint start_vgmstream;

        /// <summary>
        /// state for mixing effects
        /// </summary>
        public nint mixing_data;

        #endregion

        #region Optional data

        /// <summary>
        /// Optional data the codec needs for the whole stream. This is for codecs too
        /// different from vgmstream's structure to be reasonably shoehorned.
        /// Note also that support must be added for resetting, looping and
        /// closing for every codec that uses this, as it will not be handled.
        /// </summary>
        public nint codec_data;

        /// <summary>
        /// Optional data the codec needs for the whole stream, for special layouts.
        /// layout_data + codec_data may exist at the same time.
        /// </summary>
        public nint layout_data;

        #endregion

        #region play config/state

        /// <summary>
        /// config can be used
        /// </summary>
        public bool ConfigEnabled { get; set; }

        /// <summary>
        /// player config (applied over decoding)
        /// </summary>
        public PlayConfig Config { get; set; }

        /// <summary>
        /// player state (applied over decoding)
        /// </summary>
        public PlayState PlayState { get; set; }

        /// <summary>
        /// counter of complete loops (1=looped once)
        /// </summary>
        public int LoopCount { get; set; }

        /// <summary>
        /// max loops before continuing with the stream end (loops forever if not set)
        /// </summary>
        public int LoopTarget { get; set; }

        /// <summary>
        /// garbage buffer used for seeking/trimming
        /// </summary>
        public byte[] TmpBuf { get; set; }

        #endregion
    }
}
