﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Fp.Images.Png;

namespace Fp.Images.Png
{
    internal static class PngOpener
    {
        public static Png Open(Stream stream, IChunkVisitor? chunkVisitor = null)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException(
                    $"The provided stream of type {stream.GetType().FullName} was not readable.");
            }

            var validHeader = HasValidHeader(stream);

            if (!validHeader.IsValid)
            {
                throw new ArgumentException(
                    $"The provided stream did not start with the PNG header. Got {validHeader}.");
            }

            var crc = new byte[4];
            var imageHeader = ReadImageHeader(stream, crc);

            bool hasEncounteredImageEnd = false;

            Palette? palette = null;

            using var output = new MemoryStream();
            using (var memoryStream = new MemoryStream())
            {
                while (TryReadChunkHeader(stream, out var header))
                {
                    if (hasEncounteredImageEnd)
                    {
                        throw new InvalidOperationException(
                            $"Found another chunk {header} after already reading the IEND chunk.");
                    }

                    var bytes = new byte[header.Length];
                    int read = stream.Read(bytes, 0, bytes.Length);
                    if (read != bytes.Length)
                    {
                        throw new InvalidOperationException(
                            $"Did not read {header.Length} bytes for the {header} header, only found: {read}.");
                    }

                    if (header.IsCritical)
                    {
                        switch (header.Name)
                        {
                            case "PLTE":
                                if (header.Length % 3 != 0)
                                {
                                    throw new InvalidOperationException(
                                        $"Palette data must be multiple of 3, got {header.Length}.");
                                }

                                palette = new Palette(bytes);

                                break;
                            case "IDAT":
                                memoryStream.Write(bytes, 0, bytes.Length);
                                break;
                            case "IEND":
                                hasEncounteredImageEnd = true;
                                break;
                            default:
                                throw new NotSupportedException(
                                    $"Encountered critical header {header} which was not recognised.");
                        }
                    }

                    read = stream.Read(crc, 0, crc.Length);
                    if (read != 4)
                    {
                        throw new InvalidOperationException(
                            $"Did not read 4 bytes for the CRC, only found: {read}.");
                    }

                    int result = (int)Crc32.Calculate(Encoding.ASCII.GetBytes(header.Name), bytes);
                    int crcActual = (crc[0] << 24) + (crc[1] << 16) + (crc[2] << 8) + crc[3];

                    if (result != crcActual)
                    {
                        throw new InvalidOperationException(
                            $"CRC calculated {result} did not match file {crcActual} for chunk: {header.Name}.");
                    }

                    chunkVisitor?.Visit(stream, imageHeader, header, bytes, crc);
                }

                memoryStream.Flush();
                memoryStream.Seek(2, SeekOrigin.Begin);

                using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
                deflateStream.CopyTo(output);
                deflateStream.Close();
            }

            var bytesOut = output.ToArray();

            (byte bytesPerPixel, byte samplesPerPixel) = Decoder.GetBytesAndSamplesPerPixel(imageHeader);

            bytesOut = Decoder.Decode(bytesOut, imageHeader, bytesPerPixel, samplesPerPixel);

            return new Png(imageHeader,
                new RawPngData(bytesOut, bytesPerPixel, imageHeader.Width, imageHeader.InterlaceMethod, palette,
                    imageHeader.ColorType));
        }

        private static HeaderValidationResult HasValidHeader(Stream stream)
        {
            return new HeaderValidationResult(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(),
                stream.ReadByte(),
                stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
        }

        private static bool TryReadChunkHeader(Stream stream, out ChunkHeader chunkHeader)
        {
            chunkHeader = default;

            long position = stream.Position;
            if (!StreamHelper.TryReadHeaderBytes(stream, out var headerBytes))
            {
                return false;
            }

            int length = StreamHelper.ReadBigEndianInt32(headerBytes, 0);

            string name = Encoding.ASCII.GetString(headerBytes, 4, 4);

            chunkHeader = new ChunkHeader(position, length, name);

            return true;
        }

        private static ImageHeader ReadImageHeader(Stream stream, byte[] crc)
        {
            if (!TryReadChunkHeader(stream, out var header))
            {
                throw new ArgumentException("The provided stream did not contain a single chunk.");
            }

            if (header.Name != "IHDR")
            {
                throw new ArgumentException($"The first chunk was not the IHDR chunk: {header}.");
            }

            if (header.Length != 13)
            {
                throw new ArgumentException($"The first chunk did not have a length of 13 bytes: {header}.");
            }

            var ihdrBytes = new byte[13];
            int read = stream.Read(ihdrBytes, 0, ihdrBytes.Length);

            if (read != 13)
            {
                throw new InvalidOperationException($"Did not read 13 bytes for the IHDR, only found: {read}.");
            }

            read = stream.Read(crc, 0, crc.Length);
            if (read != 4)
            {
                throw new InvalidOperationException($"Did not read 4 bytes for the CRC, only found: {read}.");
            }

            int width = StreamHelper.ReadBigEndianInt32(ihdrBytes, 0);
            int height = StreamHelper.ReadBigEndianInt32(ihdrBytes, 4);
            byte bitDepth = ihdrBytes[8];
            byte colorType = ihdrBytes[9];
            byte compressionMethod = ihdrBytes[10];
            byte filterMethod = ihdrBytes[11];
            byte interlaceMethod = ihdrBytes[12];

            return new ImageHeader(width, height, bitDepth, (ColorType)colorType,
                (CompressionMethod)compressionMethod, (FilterMethod)filterMethod,
                (InterlaceMethod)interlaceMethod);
        }
    }
}

namespace Fp
{
    public partial class Processor
    {
        /// <summary>
        /// Read png data
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <returns>Png data</returns>
        public static Png ReadPng(Stream inputStream) => PngOpener.Open(inputStream);
    }
}
