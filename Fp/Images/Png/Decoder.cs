using System;

namespace Fp.Images.Png
{
    internal static class Decoder
    {
        public static (byte bytesPerPixel, byte samplesPerPixel) GetBytesAndSamplesPerPixel(ImageHeader header)
        {
            int bitDepthCorrected = (header.BitDepth + 7) / 8;

            byte samplesPerPixel = SamplesPerPixel(header);

            return ((byte)(samplesPerPixel * bitDepthCorrected), samplesPerPixel);
        }

        public static byte[] Decode(byte[] decompressedData, ImageHeader header, byte bytesPerPixel,
            byte samplesPerPixel)
        {
            switch (header.InterlaceMethod)
            {
                case InterlaceMethod.None:
                {
                    int bytesPerScanline = BytesPerScanline(header, samplesPerPixel);

                    int currentRowStartByteAbsolute = 1;
                    for (int rowIndex = 0; rowIndex < header.Height; rowIndex++)
                    {
                        FilterType filterType = (FilterType)decompressedData[currentRowStartByteAbsolute - 1];

                        int previousRowStartByteAbsolute = rowIndex + bytesPerScanline * (rowIndex - 1);

                        int end = currentRowStartByteAbsolute + bytesPerScanline;
                        for (int currentByteAbsolute = currentRowStartByteAbsolute;
                            currentByteAbsolute < end;
                            currentByteAbsolute++)
                        {
                            ReverseFilter(decompressedData, filterType, previousRowStartByteAbsolute,
                                currentRowStartByteAbsolute, currentByteAbsolute,
                                currentByteAbsolute - currentRowStartByteAbsolute, bytesPerPixel);
                        }

                        currentRowStartByteAbsolute += bytesPerScanline + 1;
                    }

                    return decompressedData;
                }
                case InterlaceMethod.Adam7:
                {
                    int pixelsPerRow = header.Width * bytesPerPixel;
                    byte[] newBytes = new byte[header.Height * pixelsPerRow];
                    int i = 0;
                    int previousStartRowByteAbsolute = -1;
                    // 7 passes
                    for (int pass = 0; pass < 7; pass++)
                    {
                        int numberOfScanlines = Adam7.GetNumberOfScanlinesInPass(header, pass);
                        int numberOfPixelsPerScanline = Adam7.GetPixelsPerScanlineInPass(header, pass);

                        if (numberOfScanlines <= 0 || numberOfPixelsPerScanline <= 0)
                        {
                            continue;
                        }

                        for (int scanlineIndex = 0; scanlineIndex < numberOfScanlines; scanlineIndex++)
                        {
                            FilterType filterType = (FilterType)decompressedData[i++];
                            int rowStartByte = i;

                            for (int j = 0; j < numberOfPixelsPerScanline; j++)
                            {
                                (int x, int y) pixelIndex =
                                    Adam7.GetPixelIndexForScanlineInPass(header, pass, scanlineIndex, j);
                                for (int k = 0; k < bytesPerPixel; k++)
                                {
                                    int byteLineNumber = j * bytesPerPixel + k;
                                    ReverseFilter(decompressedData, filterType, previousStartRowByteAbsolute,
                                        rowStartByte, i, byteLineNumber, bytesPerPixel);
                                    i++;
                                }

                                int start = pixelsPerRow * pixelIndex.y + pixelIndex.x * bytesPerPixel;
                                Array.ConstrainedCopy(decompressedData, rowStartByte + j * bytesPerPixel, newBytes,
                                    start, bytesPerPixel);
                            }

                            previousStartRowByteAbsolute = rowStartByte;
                        }
                    }

                    return newBytes;
                }
                default:
                    throw new ArgumentOutOfRangeException($"Invalid interlace method: {header.InterlaceMethod}.");
            }
        }

        private static byte SamplesPerPixel(ImageHeader header)
        {
            switch (header.ColorType)
            {
                case ColorType.None:
                    return 1;
                case ColorType.PaletteUsed:
                    return 1;
                case ColorType.ColorUsed:
                    return 3;
                case ColorType.AlphaChannelUsed:
                    return 2;
                case ColorType.ColorUsed | ColorType.AlphaChannelUsed:
                    return 4;
                default:
                    return 0;
            }
        }

        private static int BytesPerScanline(ImageHeader header, byte samplesPerPixel)
        {
            int width = header.Width;

            switch (header.BitDepth)
            {
                case 1:
                    return (width + 7) / 8;
                case 2:
                    return (width + 3) / 4;
                case 4:
                    return (width + 1) / 2;
                case 8:
                case 16:
                    return width * samplesPerPixel * (header.BitDepth / 8);
                default:
                    return 0;
            }
        }

        private static void ReverseFilter(byte[] data, FilterType type, int previousRowStartByteAbsolute,
            int rowStartByteAbsolute, int byteAbsolute, int rowByteIndex, int bytesPerPixel)
        {
            byte GetLeftByteValue()
            {
                int leftIndex = rowByteIndex - bytesPerPixel;
                byte leftValue = leftIndex >= 0 ? data[rowStartByteAbsolute + leftIndex] : (byte)0;
                return leftValue;
            }

            byte GetAboveByteValue()
            {
                int upIndex = previousRowStartByteAbsolute + rowByteIndex;
                return upIndex >= 0 ? data[upIndex] : (byte)0;
            }

            byte GetAboveLeftByteValue()
            {
                int index = previousRowStartByteAbsolute + rowByteIndex - bytesPerPixel;
                return index < previousRowStartByteAbsolute || previousRowStartByteAbsolute < 0
                    ? (byte)0
                    : data[index];
            }

            // Moved out of the switch for performance.
            if (type == FilterType.Up)
            {
                int above = previousRowStartByteAbsolute + rowByteIndex;
                if (above < 0)
                {
                    return;
                }

                data[byteAbsolute] += data[above];
                return;
            }

            if (type == FilterType.Sub)
            {
                int leftIndex = rowByteIndex - bytesPerPixel;
                if (leftIndex < 0)
                {
                    return;
                }

                data[byteAbsolute] += data[rowStartByteAbsolute + leftIndex];
                return;
            }

            switch (type)
            {
                case FilterType.None:
                    return;
                case FilterType.Average:
                    data[byteAbsolute] += (byte)((GetLeftByteValue() + GetAboveByteValue()) / 2);
                    break;
                case FilterType.Paeth:
                    byte a = GetLeftByteValue();
                    byte b = GetAboveByteValue();
                    byte c = GetAboveLeftByteValue();
                    data[byteAbsolute] += GetPaethValue(a, b, c);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Computes a simple linear function of the three neighboring pixels (left, above, upper left),
        /// then chooses as predictor the neighboring pixel closest to the computed value.
        /// </summary>
        private static byte GetPaethValue(byte a, byte b, byte c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            if (pa <= pb && pa <= pc)
            {
                return a;
            }

            return pb <= pc ? b : c;
        }
    }
}
