using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Fp.Images;

namespace Fp.Images
{
    /// <summary>
    /// DXT1/DXT5 texture decoder
    /// </summary>
    /// <remarks>
    /// Implementation was adapted from C++ code found at<br/>
    /// https://github.com/Benjamin-Dobell/s3tc-dxt-decompression
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class DxtDecoder
    {
        /// <summary>
        /// Helper method that packs RGBA channels into a single 4 byte pixel
        /// </summary>
        /// <param name="r">red channel</param>
        /// <param name="g">green channel</param>
        /// <param name="b">blue channel</param>
        /// <param name="a">alpha channel</param>
        /// <returns>packed rgba</returns>
        public static uint Le_PackRgba(byte r, byte g, byte b, byte a) => (uint)((a << 24) | (b << 16) | (g << 8) | r);

        /// <summary>
        /// Helper method that packs RGBA channels into a single 4 byte pixel
        /// </summary>
        /// <param name="r">red channel</param>
        /// <param name="g">green channel</param>
        /// <param name="b">blue channel</param>
        /// <param name="a">alpha channel</param>
        /// <returns>packed rgba</returns>
        public static uint Be_PackRgba(byte r, byte g, byte b, byte a) => (uint)((r << 24) | (g << 16) | (b << 8) | a);

        /// <summary>
        /// Decompresses one block of a DXT1 texture and stores the resulting pixels at the appropriate offset in <paramref name="image"/>.
        /// </summary>
        /// <param name="x">x-coordinate of the first pixel in the block</param>
        /// <param name="y">y-coordinate of the first pixel in the block</param>
        /// <param name="width">width of the texture being decompressed</param>
        /// <param name="blockStorage">pointer to the block to decompress</param>
        /// <param name="image">pointer to image where the decompressed pixel data should be stored</param>
        public static unsafe void Le_DecompressBlockDxt1(uint x, uint y, uint width, byte* blockStorage, uint* image)
        {
            ushort color0 = *(ushort*)blockStorage;
            ushort color1 = *(ushort*)(blockStorage + 2);

            uint temp;

            temp = (uint)((color0 >> 11) * 255 + 16);
            byte r0 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color0 & 0x07E0) >> 5) * 255 + 32);
            byte g0 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color0 & 0x001F) * 255 + 16);
            byte b0 = (byte)((temp / 32 + temp) / 32);

            temp = (uint)((color1 >> 11) * 255 + 16);
            byte r1 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color1 & 0x07E0) >> 5) * 255 + 32);
            byte g1 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color1 & 0x001F) * 255 + 16);
            byte b1 = (byte)((temp / 32 + temp) / 32);

            uint code = *(uint*)(blockStorage + 4);

            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    uint finalColor = 0;
                    byte positionCode = (byte)((code >> 2 * (4 * j + i)) & 0x03);

                    if (color0 > color1)
                    {
                        switch (positionCode)
                        {
                            case 0:
                                finalColor = Le_PackRgba(r0, g0, b0, 255);
                                break;
                            case 1:
                                finalColor = Le_PackRgba(r1, g1, b1, 255);
                                break;
                            case 2:
                                finalColor = Le_PackRgba((byte)((2 * r0 + r1) / 3), (byte)((2 * g0 + g1) / 3),
                                    (byte)((2 * b0 + b1) / 3), 255);
                                break;
                            case 3:
                                finalColor = Le_PackRgba((byte)((r0 + 2 * r1) / 3), (byte)((g0 + 2 * g1) / 3),
                                    (byte)((b0 + 2 * b1) / 3), 255);
                                break;
                        }
                    }
                    else
                    {
                        switch (positionCode)
                        {
                            case 0:
                                finalColor = Le_PackRgba(r0, g0, b0, 255);
                                break;
                            case 1:
                                finalColor = Le_PackRgba(r1, g1, b1, 255);
                                break;
                            case 2:
                                finalColor = Le_PackRgba((byte)((r0 + r1) / 2), (byte)((g0 + g1) / 2),
                                    (byte)((b0 + b1) / 2), 255);
                                break;
                            case 3:
                                finalColor = Le_PackRgba(0, 0, 0, 255);
                                break;
                        }
                    }

                    if (x + i < width)
                        image[(y + j) * width + x + i] = finalColor;
                }
            }
        }

        /// <summary>
        /// Decompresses one block of a DXT1 texture and stores the resulting pixels at the appropriate offset in <paramref name="image"/>.
        /// </summary>
        /// <param name="x">x-coordinate of the first pixel in the block</param>
        /// <param name="y">y-coordinate of the first pixel in the block</param>
        /// <param name="width">width of the texture being decompressed</param>
        /// <param name="blockStorage">pointer to the block to decompress</param>
        /// <param name="image">pointer to image where the decompressed pixel data should be stored</param>
        public static unsafe void Be_DecompressBlockDxt1(uint x, uint y, uint width, byte* blockStorage, uint* image)
        {
            ushort color0 = BinaryPrimitives.ReverseEndianness(*(ushort*)blockStorage);
            ushort color1 = BinaryPrimitives.ReverseEndianness(*(ushort*)(blockStorage + 2));

            uint temp;

            temp = (uint)((color0 >> 11) * 255 + 16);
            byte r0 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color0 & 0x07E0) >> 5) * 255 + 32);
            byte g0 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color0 & 0x001F) * 255 + 16);
            byte b0 = (byte)((temp / 32 + temp) / 32);

            temp = (uint)((color1 >> 11) * 255 + 16);
            byte r1 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color1 & 0x07E0) >> 5) * 255 + 32);
            byte g1 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color1 & 0x001F) * 255 + 16);
            byte b1 = (byte)((temp / 32 + temp) / 32);

            uint code = *(uint*)(blockStorage + 4);

            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    uint finalColor = 0;
                    byte positionCode = (byte)((code >> 2 * (4 * j + i)) & 0x03);

                    if (color0 > color1)
                    {
                        switch (positionCode)
                        {
                            case 0:
                                finalColor = Be_PackRgba(r0, g0, b0, 255);
                                break;
                            case 1:
                                finalColor = Be_PackRgba(r1, g1, b1, 255);
                                break;
                            case 2:
                                finalColor = Be_PackRgba((byte)((2 * r0 + r1) / 3), (byte)((2 * g0 + g1) / 3),
                                    (byte)((2 * b0 + b1) / 3), 255);
                                break;
                            case 3:
                                finalColor = Be_PackRgba((byte)((r0 + 2 * r1) / 3), (byte)((g0 + 2 * g1) / 3),
                                    (byte)((b0 + 2 * b1) / 3), 255);
                                break;
                        }
                    }
                    else
                    {
                        switch (positionCode)
                        {
                            case 0:
                                finalColor = Be_PackRgba(r0, g0, b0, 255);
                                break;
                            case 1:
                                finalColor = Be_PackRgba(r1, g1, b1, 255);
                                break;
                            case 2:
                                finalColor = Be_PackRgba((byte)((r0 + r1) / 2), (byte)((g0 + g1) / 2),
                                    (byte)((b0 + b1) / 2), 255);
                                break;
                            case 3:
                                finalColor = Be_PackRgba(0, 0, 0, 255);
                                break;
                        }
                    }

                    if (x + i < width)
                        image[(y + j) * width + x + i] = finalColor;
                }
            }
        }

        /// <summary>
        /// Decompresses all the blocks of a DXT1 compressed texture and stores the resulting pixels in <paramref name="image"/>.
        /// </summary>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="blockStorage">pointer to compressed DXT1 blocks</param>
        /// <param name="image">pointer to the image where the decompressed pixels will be stored</param>
        public static unsafe void BlockDecompressImageDxt1(uint width, uint height, ReadOnlySpan<byte> blockStorage,
            Span<uint> image)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            fixed (byte* pBlockStorage = &blockStorage.GetPinnableReference())
            {
                byte* pCBlockStorage = pBlockStorage;
                fixed (uint* pImage = &image.GetPinnableReference())
                {
                    if (BitConverter.IsLittleEndian)
                        for (uint j = 0; j < blockCountY; j++)
                        {
                            for (uint i = 0; i < blockCountX; i++)
                                Le_DecompressBlockDxt1(i * 4, j * 4, width, pCBlockStorage + i * 8, pImage);
                            pCBlockStorage += blockCountX * 8;
                        }
                    else
                        for (uint j = 0; j < blockCountY; j++)
                        {
                            for (uint i = 0; i < blockCountX; i++)
                                Be_DecompressBlockDxt1(i * 4, j * 4, width, pCBlockStorage + i * 8, pImage);
                            pCBlockStorage += blockCountX * 8;
                        }
                }
            }
        }

        /// <summary>
        /// Decompresses one block of a DXT5 texture and stores the resulting pixels at the appropriate offset in <paramref name="image"/>.
        /// </summary>
        /// <param name="x">x-coordinate of the first pixel in the block</param>
        /// <param name="y">y-coordinate of the first pixel in the block</param>
        /// <param name="width">width of the texture being decompressed</param>
        /// <param name="blockStorage">pointer to the block to decompress</param>
        /// <param name="image">pointer to image where the decompressed pixel data should be stored</param>
        public static unsafe void Le_DecompressBlockDxt5(uint x, uint y, uint width, byte* blockStorage, uint* image)
        {
            byte alpha0 = *blockStorage;
            byte alpha1 = *(blockStorage + 1);

            byte* bits = blockStorage + 2;
            uint alphaCode1 = (uint)(bits[2] | (bits[3] << 8) | (bits[4] << 16) | (bits[5] << 24));
            ushort alphaCode2 = (ushort)(bits[0] | (bits[1] << 8));

            ushort color0 = *(ushort*)(blockStorage + 8);
            ushort color1 = *(ushort*)(blockStorage + 10);

            uint temp;

            temp = (uint)((color0 >> 11) * 255 + 16);
            byte r0 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color0 & 0x07E0) >> 5) * 255 + 32);
            byte g0 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color0 & 0x001F) * 255 + 16);
            byte b0 = (byte)((temp / 32 + temp) / 32);

            temp = (uint)((color1 >> 11) * 255 + 16);
            byte r1 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color1 & 0x07E0) >> 5) * 255 + 32);
            byte g1 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color1 & 0x001F) * 255 + 16);
            byte b1 = (byte)((temp / 32 + temp) / 32);

            uint code = *(uint*)(blockStorage + 12);

            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    int alphaCodeIndex = 3 * (4 * j + i);
                    int alphaCode;

                    if (alphaCodeIndex <= 12)
                    {
                        alphaCode = (alphaCode2 >> alphaCodeIndex) & 0x07;
                    }
                    else if (alphaCodeIndex == 15)
                    {
                        alphaCode = (alphaCode2 >> 15) | (byte)((alphaCode1 << 1) & 0x06);
                    }
                    else // alphaCodeIndex >= 18 && alphaCodeIndex <= 45
                    {
                        alphaCode = (int)((alphaCode1 >> (alphaCodeIndex - 16)) & 0x07);
                    }

                    byte finalAlpha;
                    if (alphaCode == 0)
                    {
                        finalAlpha = alpha0;
                    }
                    else if (alphaCode == 1)
                    {
                        finalAlpha = alpha1;
                    }
                    else
                    {
                        if (alpha0 > alpha1)
                        {
                            finalAlpha = (byte)(((8 - alphaCode) * alpha0 + (alphaCode - 1) * alpha1) / 7);
                        }
                        else
                        {
                            if (alphaCode == 6)
                                finalAlpha = 0;
                            else if (alphaCode == 7)
                                finalAlpha = 255;
                            else
                                finalAlpha = (byte)(((6 - alphaCode) * alpha0 + (alphaCode - 1) * alpha1) / 5);
                        }
                    }

                    byte colorCode = (byte)((code >> 2 * (4 * j + i)) & 0x03);

                    uint finalColor = 0;
                    switch (colorCode)
                    {
                        case 0:
                            finalColor = Le_PackRgba(r0, g0, b0, finalAlpha);
                            break;
                        case 1:
                            finalColor = Le_PackRgba(r1, g1, b1, finalAlpha);
                            break;
                        case 2:
                            finalColor = Le_PackRgba((byte)((2 * r0 + r1) / 3), (byte)((2 * g0 + g1) / 3),
                                (byte)((2 * b0 + b1) / 3), finalAlpha);
                            break;
                        case 3:
                            finalColor = Le_PackRgba((byte)((r0 + 2 * r1) / 3), (byte)((g0 + 2 * g1) / 3),
                                (byte)((b0 + 2 * b1) / 3), finalAlpha);
                            break;
                    }

                    if (x + i < width)
                        image[(y + j) * width + x + i] = finalColor;
                }
            }
        }

        /// <summary>
        /// Decompresses one block of a DXT5 texture and stores the resulting pixels at the appropriate offset in <paramref name="image"/>.
        /// </summary>
        /// <param name="x">x-coordinate of the first pixel in the block</param>
        /// <param name="y">y-coordinate of the first pixel in the block</param>
        /// <param name="width">width of the texture being decompressed</param>
        /// <param name="blockStorage">pointer to the block to decompress</param>
        /// <param name="image">pointer to image where the decompressed pixel data should be stored</param>
        public static unsafe void Be_DecompressBlockDxt5(uint x, uint y, uint width, byte* blockStorage, uint* image)
        {
            byte alpha0 = *blockStorage;
            byte alpha1 = *(blockStorage + 1);

            byte* bits = blockStorage + 2;
            uint alphaCode1 = (uint)(bits[2] | (bits[3] << 8) | (bits[4] << 16) | (bits[5] << 24));
            ushort alphaCode2 = (ushort)(bits[0] | (bits[1] << 8));

            ushort color0 = BinaryPrimitives.ReverseEndianness(*(ushort*)(blockStorage + 8));
            ushort color1 = BinaryPrimitives.ReverseEndianness(*(ushort*)(blockStorage + 10));

            uint temp;

            temp = (uint)((color0 >> 11) * 255 + 16);
            byte r0 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color0 & 0x07E0) >> 5) * 255 + 32);
            byte g0 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color0 & 0x001F) * 255 + 16);
            byte b0 = (byte)((temp / 32 + temp) / 32);

            temp = (uint)((color1 >> 11) * 255 + 16);
            byte r1 = (byte)((temp / 32 + temp) / 32);
            temp = (uint)(((color1 & 0x07E0) >> 5) * 255 + 32);
            byte g1 = (byte)((temp / 64 + temp) / 64);
            temp = (uint)((color1 & 0x001F) * 255 + 16);
            byte b1 = (byte)((temp / 32 + temp) / 32);

            uint code = *(uint*)(blockStorage + 12);

            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    int alphaCodeIndex = 3 * (4 * j + i);
                    int alphaCode;

                    if (alphaCodeIndex <= 12)
                    {
                        alphaCode = (alphaCode2 >> alphaCodeIndex) & 0x07;
                    }
                    else if (alphaCodeIndex == 15)
                    {
                        alphaCode = (alphaCode2 >> 15) | (byte)((alphaCode1 << 1) & 0x06);
                    }
                    else // alphaCodeIndex >= 18 && alphaCodeIndex <= 45
                    {
                        alphaCode = (int)((alphaCode1 >> (alphaCodeIndex - 16)) & 0x07);
                    }

                    byte finalAlpha;
                    if (alphaCode == 0)
                    {
                        finalAlpha = alpha0;
                    }
                    else if (alphaCode == 1)
                    {
                        finalAlpha = alpha1;
                    }
                    else
                    {
                        if (alpha0 > alpha1)
                        {
                            finalAlpha = (byte)(((8 - alphaCode) * alpha0 + (alphaCode - 1) * alpha1) / 7);
                        }
                        else
                        {
                            if (alphaCode == 6)
                                finalAlpha = 0;
                            else if (alphaCode == 7)
                                finalAlpha = 255;
                            else
                                finalAlpha = (byte)(((6 - alphaCode) * alpha0 + (alphaCode - 1) * alpha1) / 5);
                        }
                    }

                    byte colorCode = (byte)((code >> 2 * (4 * j + i)) & 0x03);

                    uint finalColor = 0;
                    switch (colorCode)
                    {
                        case 0:
                            finalColor = Be_PackRgba(r0, g0, b0, finalAlpha);
                            break;
                        case 1:
                            finalColor = Be_PackRgba(r1, g1, b1, finalAlpha);
                            break;
                        case 2:
                            finalColor = Be_PackRgba((byte)((2 * r0 + r1) / 3), (byte)((2 * g0 + g1) / 3),
                                (byte)((2 * b0 + b1) / 3), finalAlpha);
                            break;
                        case 3:
                            finalColor = Be_PackRgba((byte)((r0 + 2 * r1) / 3), (byte)((g0 + 2 * g1) / 3),
                                (byte)((b0 + 2 * b1) / 3), finalAlpha);
                            break;
                    }

                    if (x + i < width)
                        image[(y + j) * width + x + i] = finalColor;
                }
            }
        }

        /// <summary>
        /// Decompresses all the blocks of a DXT5 compressed texture and stores the resulting pixels in <paramref name="image"/>.
        /// </summary>
        /// <param name="width">Texture width</param>
        /// <param name="height">Texture height</param>
        /// <param name="blockStorage">pointer to compressed DXT5 blocks</param>
        /// <param name="image">pointer to the image where the decompressed pixels will be stored</param>
        public static unsafe void BlockDecompressImageDxt5(uint width, uint height, ReadOnlySpan<byte> blockStorage,
            Span<uint> image)
        {
            uint blockCountX = (width + 3) / 4;
            uint blockCountY = (height + 3) / 4;

            fixed (byte* pBlockStorage = &blockStorage.GetPinnableReference())
            {
                byte* pCBlockStorage = pBlockStorage;
                fixed (uint* pImage = &image.GetPinnableReference())
                {
                    if (BitConverter.IsLittleEndian)
                        for (uint j = 0; j < blockCountY; j++)
                        {
                            for (uint i = 0; i < blockCountX; i++)
                                Le_DecompressBlockDxt5(i * 4, j * 4, width, pCBlockStorage + i * 16, pImage);
                            pCBlockStorage += blockCountX * 16;
                        }
                    else
                        for (uint j = 0; j < blockCountY; j++)
                        {
                            for (uint i = 0; i < blockCountX; i++)
                                Be_DecompressBlockDxt5(i * 4, j * 4, width, pCBlockStorage + i * 16, pImage);
                            pCBlockStorage += blockCountX * 16;
                        }
                }
            }
        }
    }
}

namespace Fp
{
    public partial class Processor
    {
        /// <summary>
        /// Decompress DXT1-compressed image
        /// </summary>
        /// <param name="src">Source buffer</param>
        /// <param name="img">Target buffer</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static void DecodeDxt1(ReadOnlySpan<byte> src, Span<uint> img,int width, int height) =>
            DxtDecoder.BlockDecompressImageDxt1((uint)width, (uint)height, src, img);

        /// <summary>
        /// Decompress DXT5-compressed image
        /// </summary>
        /// <param name="src">Source buffer</param>
        /// <param name="img">Target buffer</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static void DecodeDxt5(ReadOnlySpan<byte> src, Span<uint> img, int width, int height) =>
            DxtDecoder.BlockDecompressImageDxt5((uint)width, (uint)height, src, img);
    }

    public partial class Scripting
    {
        /// <summary>
        /// Decompress DXT1-compressed image
        /// </summary>
        /// <param name="src">Source buffer</param>
        /// <param name="img">Target buffer</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static void decodeDxt1(ReadOnlyMemory<byte> src, Memory<uint> img, int width, int height) =>
            Processor.DecodeDxt1(src.Span, img.Span, width, height);

        /// <summary>
        /// Decompress DXT5-compressed image
        /// </summary>
        /// <param name="src">Source buffer</param>
        /// <param name="img">Target buffer</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static void decodeDxt5(ReadOnlyMemory<byte> src, Memory<uint> img, int width, int height) =>
            Processor.DecodeDxt5(src.Span, img.Span, width, height);

    }
}
