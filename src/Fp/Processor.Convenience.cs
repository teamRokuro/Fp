using System;
using System.Collections.Generic;
using System.Linq;
using static Fp.Detector;
using static Fp.Processor;

namespace Fp
{
    #region General util

    // ReSharper disable InconsistentNaming
    public partial class Processor
    {
        /// <summary>
        /// Empty enumerable of data.
        /// </summary>
        public static readonly IEnumerable<Data> Nothing = Enumerable.Empty<Data>();

        /// <summary>
        /// Warns value as unsupported.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Empty enumerable.</returns>
        public IEnumerable<Data> unsupported(long value)
        {
            LogWarn($"Version {value:X2} unsupported");
            return Nothing;
        }

        /// <summary>
        /// Warns value as unsupported.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Empty enumerable.</returns>
        public IEnumerable<Data> unsupported(ulong value)
        {
            LogWarn($"Version {value:X2} unsupported");
            return Nothing;
        }
    }

    public partial class Scripting
    {
        /// <summary>
        /// Empty enumerable of data.
        /// </summary>
        public static readonly IEnumerable<Data> _nothing = Nothing;

        /// <summary>
        /// Warns value as unsupported.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Empty enumerable.</returns>
        public static IEnumerable<Data> unsupported(long value) => Current.unsupported(value);

        /// <summary>
        /// Warns value as unsupported.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Empty enumerable.</returns>
        public static IEnumerable<Data> unsupported(ulong value) => Current.unsupported(value);
    }

    #endregion

    #region Extension detectors

    #region WAV

    public partial class Processor
    {
        /// <summary>
        /// Detect WAV audio files.
        /// </summary>
        /// <returns>Detector.</returns>
        public Detector _WAV() => _WAV(null);

        /// <summary>
        /// Detect WAV audio files.
        /// </summary>
        /// <param name="detector">Existing detector.</param>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        internal Detector _WAV(Detector? detector, object? source = null) =>
            new(detector, source ?? detector?.Source ?? __staticDetection, o => o switch
            {
                { } when o == __staticDetection && HasMagic("RIFF") && HasMagic("WAVE", 8) => ".wav",
                ReadOnlyMemory<byte> m when HasMagic(m.Span, "RIFF") && HasMagic(m.Span, "WAVE", 8) => ".wav",
                _ => null
            });
    }

    public partial class Scripting
    {
        /// <summary>
        /// Detect WAV audio files.
        /// </summary>
        /// <returns>Detector.</returns>
        public static Detector _WAV() => Current._WAV(null);

        /// <summary>
        /// Detect WAV audio files.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        public static Detector _WAV(this object source) => Current._WAV(null, source);

        /// <summary>
        /// Detect WAV audio files.
        /// </summary>
        /// <param name="detector">Existing detector.</param>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        public static Detector _WAV(this Detector? detector, object? source = null) => Current._WAV(detector, source);
    }

    #endregion

    #region BMP

    public partial class Processor
    {
        /// <summary>
        /// Detect BMP bitmap files.
        /// </summary>
        /// <returns>Detector.</returns>
        public Detector _BMP() => _BMP(null);

        /// <summary>
        /// Detect BMP bitmap files.
        /// </summary>
        /// <param name="detector">Existing detector.</param>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        internal Detector _BMP(Detector? detector, object? source = null) =>
            new(detector, source ?? detector?.Source ?? __staticDetection, o => o switch
            {
                { } when o == __staticDetection && HasMagic("BM") && i4l[2] == InputLength => ".wav",
                ReadOnlyMemory<byte> m when HasMagic(m.Span, "BM") && i4l[m, 2] == m.Length => ".bmp",
                _ => null
            });
    }

    public partial class Scripting
    {
        /// <summary>
        /// Detect BMP bitmap files.
        /// </summary>
        /// <returns>Detector.</returns>
        public static Detector _BMP() => Current._BMP(null);

        /// <summary>
        /// Detect BMP bitmap files.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        public static Detector _BMP(this object source) => Current._BMP(null, source);

        /// <summary>
        /// Detect BMP bitmap files.
        /// </summary>
        /// <param name="detector">Existing detector.</param>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        public static Detector _BMP(this Detector? detector, object? source = null) => Current._BMP(detector, source);
    }

    #endregion

    #region Magic

    public partial class Processor
    {
        /// <summary>
        /// Detect based on magic value.
        /// </summary>
        /// <param name="magicValue">Magic value.</param>
        /// <param name="value">Target value.</param>
        /// <param name="offset">Base offset of magic.</param>
        /// <returns>Detector.</returns>
        public Detector __(string magicValue, string value, int offset = 0) =>
            __(null, magicValue, value, offset);

        /// <summary>
        /// Detect based on magic value.
        /// </summary>
        /// <param name="detector">Existing detector.</param>
        /// <param name="magicValue">Magic value.</param>
        /// <param name="value">Target value.</param>
        /// <param name="offset">Base offset of magic.</param>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        internal Detector __(Detector? detector, string magicValue, string value, int offset = 0,
            object? source = null) =>
            new(detector, source ?? detector?.Source ?? __staticDetection, o => o switch
            {
                { } when o == __staticDetection && HasMagic(magicValue, offset) => value,
                ReadOnlyMemory<byte> m when HasMagic(m.Span, magicValue, offset) => value,
                _ => null
            });
    }

    public partial class Scripting
    {
        /// <summary>
        /// Detect based on magic value.
        /// </summary>
        /// <param name="magicValue">Magic value.</param>
        /// <param name="value">Target value.</param>
        /// <param name="offset">Base offset of magic.</param>
        /// <returns>Detector.</returns>
        public static Detector __(string magicValue, string value, int offset = 0) =>
            Current.__(null, magicValue, value, offset);

        /// <summary>
        /// Detect based on magic value.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="magicValue">Magic value.</param>
        /// <param name="value">Target value.</param>
        /// <param name="offset">Base offset of magic.</param>
        /// <returns>Detector.</returns>
        public static Detector __(this object source, string magicValue, string value, int offset = 0) =>
            Current.__(null, magicValue, value, offset, source);

        /// <summary>
        /// Detect based on magic value.
        /// </summary>
        /// <param name="detector">Existing detector.</param>
        /// <param name="magicValue">Magic value.</param>
        /// <param name="value">Target value.</param>
        /// <param name="offset">Base offset of magic.</param>
        /// <param name="source">Data source.</param>
        /// <returns>Detector.</returns>
        public static Detector __(this Detector? detector, string magicValue, string value, int offset = 0,
            object? source = null) => Current.__(detector, magicValue, value, offset, source);
    }

    #endregion

    #region Fallback

    public partial class Scripting
    {
        /// <summary>
        /// Fallback to value.
        /// </summary>
        /// <param name="detector">Existing detector.</param>
        /// <param name="value">Fallback value.</param>
        /// <returns>Detector.</returns>
        public static Fallback ___(this Detector detector, string value) => new(detector, value);
    }

    #endregion

    #endregion

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Represents a detector.
    /// </summary>
    public record Detector(Detector? Prev, object Source, Func<object?, string?> DetectionFunction)
    {
        /// <summary>
        /// Detect from this detector or any that preceded it.
        /// </summary>
        /// <returns>Detected value.</returns>
        public static implicit operator string?(Detector detector)
        {
            string? v = detector.DetectionFunction(CoerceToROM(detector.Source));
            if (v != null || detector.Prev == null) return v;
            return detector.Prev;
        }

        /// <summary>
        /// Detect from this detector or any that preceded it using late-bound source.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <returns>Detected value.</returns>
        public string? Detect(object? source = null)
        {
            string? v = DetectionFunction(CoerceToROM(source ?? __staticDetection));
            if (v != null || Prev == null) return v;
            return Prev.Detect(source);
        }

        private static object? CoerceToROM(object? value) => value switch
        {
            byte[] b => (ReadOnlyMemory<byte>)b,
            ArraySegment<byte> b => (ReadOnlyMemory<byte>)b,
            Memory<byte> b => (ReadOnlyMemory<byte>)b,
            ReadOnlyMemory<byte> b => b,
            _ => value
        };

        internal static readonly object __staticDetection = new();
    }

    /// <summary>
    /// Represents a fallback detector.
    /// </summary>
    public record Fallback(Detector Prev, string Value)
    {
        /// <summary>
        /// Detect from previous detectors or fallback to value.
        /// </summary>
        /// <returns>Detected value.</returns>
        public static implicit operator string(Fallback detector)
        {
            string? v = detector.Prev;
            return v ?? detector.Value;
        }

        /// <summary>
        /// Detect from this detector or any that preceded it using late-bound source.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <returns>Detected value.</returns>
        public string Detect(object? source = null)
        {
            string? v = Prev.Detect(source);
            return v ?? Value;
        }
    }
}
