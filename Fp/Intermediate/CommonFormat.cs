namespace Fp.Intermediate
{
    /// <summary>
    /// Common file formats
    /// </summary>
    public enum CommonFormat
    {
        /// <summary>
        /// Generic data
        /// </summary>
        Generic,

        /// <summary>
        /// Deflate-compressed PNG image
        /// </summary>
        PngDeflate,

        /// <summary>
        /// JPEG image
        /// </summary>
        Jpeg,

        /// <summary>
        /// PCM WAV file
        /// </summary>
        PcmWave,

        /// <summary>
        /// Export for data format is not supported
        /// </summary>
        ExportUnsupported
    }
}
