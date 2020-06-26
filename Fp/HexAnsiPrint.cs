using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fp
{
    /// <summary>
    /// Prints hex text with ANSI color codes for labelled sections
    /// </summary>
    public static class HexAnsiPrint
    {
        /// <summary>
        /// Maximum width of offset label
        /// </summary>
        public const int PosWidth = 10;

        /// <summary>
        /// Maximum width of annotation label
        /// </summary>
        public const int TextWidth = 16;

        /// <summary>
        /// Supported color sequences
        /// </summary>
        public static readonly Dictionary<Color, string> Sequences = new Dictionary<Color, string>
        {
            {Color.Black, "\u001b[30;1m"},
            {Color.Red, "\u001b[31;1m"},
            {Color.Green, "\u001b[32;1m"},
            {Color.Yellow, "\u001b[33;1m"},
            {Color.Blue, "\u001b[34;1m"},
            {Color.Magenta, "\u001b[35;1m"},
            {Color.Cyan, "\u001b[36;1m"},
            {Color.White, "\u001b[37;1m"},
        };

        /// <summary>
        /// Supported colors
        /// </summary>
        public static readonly IReadOnlyList<Color> Colors = new List<Color>
        {
            Color.Black,
            Color.Red,
            Color.Green,
            Color.Yellow,
            Color.Blue,
            Color.Magenta,
            Color.Cyan,
            Color.White
        };

        /// <summary>
        /// Print hex text
        /// </summary>
        /// <param name="data">Data to print</param>
        /// <param name="annotations">Data annotations</param>
        /// <param name="space">Space between bytes</param>
        /// <param name="pow2Modulus">Only display power of 2 per line</param>
        /// <exception cref="ApplicationException"></exception>
        public static void Print(ReadOnlySpan<byte> data,
            (int offset, int length, string label, Color color)[] annotations,
            bool space = true, bool pow2Modulus = false)
        {
            int width = Console.WindowWidth;
            int availableSpace = width - TextWidth - PosWidth - 2 - 1;
            int charWidth = space ? 3 : 2;
            if (availableSpace < charWidth) throw new ApplicationException("Console width too small for output");
            int w = availableSpace / charWidth;
            if (pow2Modulus)
            {
                int mod = 1;
                while (mod <= w)
                {
                    mod <<= 1;
                }

                w = mod >> 1;
            }

            int left = data.Length;
            int cur = 0;
            int annotationOffset = 0;
            Queue<(int offset, int length, string label, Color color)> annotationQueue =
                new Queue<(int offset, int length, string label, Color color)>();
            Queue<(int offset, int length, string label, Color color)> annotationPrintQueue =
                new Queue<(int offset, int length, string label, Color color)>();

            while (left > 0)
            {
                int curLine = 0;
                foreach ((int offset, int length, string label, Color color) x in annotations.AsSpan(annotationOffset)
                )
                {
                    if (x.offset >= cur + w) break;
                    annotationQueue.Enqueue(x);
                    annotationPrintQueue.Enqueue(x);
                    annotationOffset++;
                }

                Console.Write($"{Sequences[Color.White]}0x{{0:X{PosWidth}}} ", cur);
                for (; curLine < w && cur < data.Length; curLine++)
                {
                    bool consumed = false;
                    foreach ((int offset, int length, string label, Color color) x in annotationQueue)
                        if (x.offset <= cur && x.offset + x.length > cur)
                        {
                            consumed = true;
                            Console.Write($"{Sequences[x.color]}{data[cur]:X2}{(space ? " " : "")}");
                            break;
                        }

                    if (!consumed)
                        Console.Write($"{Sequences[Color.White]}{data[cur]:X2}{(space ? " " : "")}");
                    cur++;
                }

                while (annotationQueue.Count > 0)
                {
                    (int offset, int length, string label, Color color) result = annotationQueue.Peek();
                    if (result.offset + result.length <= cur) annotationQueue.Dequeue();
                    else break;
                }

                if (annotationPrintQueue.Count > 0)
                {
                    (int offset, int length, string label, Color color) result = annotationPrintQueue.Dequeue();
                    Console.Write($"{(space ? "" : " ")}{Sequences[result.color]}");
                    Console.Write(result.label.Length > TextWidth
                        ? result.label.Substring(0, TextWidth)
                        : result.label);
                }

                Console.WriteLine();

                left -= curLine;
            }
        }
    }
}
