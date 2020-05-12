using System;

namespace Fp.Images.Png {
    /// <summary>
    /// Provides convenience methods for indexing into a raw byte array to extract pixel values.
    /// </summary>
    public class RawPngData {
        /// <summary>
        /// Raw data
        /// </summary>
        public readonly byte[] Data;

        private readonly int _bytesPerPixel;
        private readonly int _width;
        private readonly Palette _palette;
        private readonly ColorType _colorType;
        private readonly int _rowOffset;

        /// <summary>
        /// Create a new <see cref="RawPngData"/>.
        /// </summary>
        /// <param name="data">The decoded pixel data as bytes.</param>
        /// <param name="bytesPerPixel">The number of bytes in each pixel.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="interlaceMethod">The interlace method used.</param>
        /// <param name="palette">The palette for images using indexed colors.</param>
        /// <param name="colorType">The color type.</param>
        public RawPngData(byte[] data, int bytesPerPixel, int width, InterlaceMethod interlaceMethod, Palette palette,
            ColorType colorType) {
            if (width < 0) {
                throw new ArgumentOutOfRangeException($"Width must be greater than or equal to 0, got {width}.");
            }

            Data = data ?? throw new ArgumentNullException(nameof(data));
            _bytesPerPixel = bytesPerPixel;
            _width = width;
            _palette = palette;
            _colorType = colorType;
            _rowOffset = interlaceMethod == InterlaceMethod.Adam7 ? 0 : 1;
        }

        /// <summary>
        /// Get pixel value
        /// </summary>
        /// <param name="x">X offset</param>
        /// <param name="y">Y offset</param>
        /// <returns>Pixel value</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Pixel GetPixel(int x, int y) {
            var rowStartPixel = _rowOffset + _rowOffset * y + _bytesPerPixel * _width * y;

            var pixelStartIndex = rowStartPixel + _bytesPerPixel * x;

            var first = Data[pixelStartIndex];

            if (_palette != null) {
                return _palette.GetPixel(first);
            }

            switch (_bytesPerPixel) {
                case 1:
                    return new Pixel(first, first, first, 255, true);
                case 2:
                    switch (_colorType) {
                        case ColorType.None: {
                            byte second = Data[pixelStartIndex + 1];
                            var value = ToSingleByte(first, second);
                            return new Pixel(value, value, value, 255, true);
                        }
                        default:
                            return new Pixel(first, first, first, Data[pixelStartIndex + 1], true);
                    }

                case 3:
                    return new Pixel(first, Data[pixelStartIndex + 1], Data[pixelStartIndex + 2], 255, false);
                case 4:
                    switch (_colorType) {
                        case ColorType.None | ColorType.AlphaChannelUsed: {
                            var second = Data[pixelStartIndex + 1];
                            var firstAlpha = Data[pixelStartIndex + 2];
                            var secondAlpha = Data[pixelStartIndex + 3];
                            var gray = ToSingleByte(first, second);
                            var alpha = ToSingleByte(firstAlpha, secondAlpha);
                            return new Pixel(gray, gray, gray, alpha, true);
                        }
                        default:
                            return new Pixel(first, Data[pixelStartIndex + 1], Data[pixelStartIndex + 2],
                                Data[pixelStartIndex + 3], false);
                    }
                case 6:
                    return new Pixel(first, Data[pixelStartIndex + 2], Data[pixelStartIndex + 4], 255, false);
                case 8:
                    return new Pixel(first, Data[pixelStartIndex + 2], Data[pixelStartIndex + 4],
                        Data[pixelStartIndex + 6], false);
                default:
                    throw new InvalidOperationException($"Unreconized number of bytes per pixel: {_bytesPerPixel}.");
            }
        }

        private static byte ToSingleByte(byte first, byte second) {
            var us = (first << 8) + second;
            var result = (byte) Math.Round(255 * us / (double) ushort.MaxValue);
            return result;
        }
    }
}