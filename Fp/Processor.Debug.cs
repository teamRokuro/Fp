using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fp
{
    public partial class Processor
    {
        #region Debugging utilities

        /// <summary>
        /// If true, enables debugging features
        /// </summary>
        public bool Debug = false;

        /// <summary>
        /// Annotations for memory
        /// </summary>
        public readonly Dictionary<ReadOnlyMemory<byte>, List<(int offset, int length, string label, Color color)>>
            MemAnnotations =
                new Dictionary<ReadOnlyMemory<byte>, List<(int offset, int length, string label, Color color)>>();

        private int _memColorIdx;

        /// <summary>
        /// Clears stored memories and annotations
        /// </summary>
        /// <remarks>No-op if <see cref="Debug"/> is false.</remarks>
        public void MemClear()
        {
            if (!Debug) return;
            MemAnnotations.Clear();
        }

        /// <summary>
        /// Labels memory with annotation
        /// </summary>
        /// <param name="memory">Target memory</param>
        /// <param name="offset">Data offset</param>
        /// <param name="length">Data length</param>
        /// <param name="label">Annotation to add</param>
        /// <param name="color">Color, random default</param>
        /// <remarks>No-op if <see cref="Debug"/> is false.<br/>Users should not slice memory struct between label and print, uses <see cref="MemAnnotations"/> which uses the memory as a key.</remarks>
        public void MemLabel(ReadOnlyMemory<byte> memory, int offset, int length, string label, Color? color = null)
        {
            if (!Debug) return;
            if (!MemAnnotations.TryGetValue(memory,
                out List<(int offset, int length, string label, Color color)> list))
                list = MemAnnotations[memory] = new List<(int offset, int length, string label, Color color)>();
            if (color == null)
            {
                color = HexAnsiPrint.Colors[_memColorIdx];
                _memColorIdx = (_memColorIdx + 1) % (HexAnsiPrint.Colors.Count - 1);
            }

            list.Add((offset, length, label, color.Value));
        }

        /// <summary>
        /// Prints memory with associated annotations
        /// </summary>
        /// <param name="memory">Target memory</param>
        /// <param name="space">Space between bytes</param>
        /// <param name="pow2Modulus">Only display power of 2 per line</param>
        /// <remarks>No-op if <see cref="Debug"/> is false.<br/>Users should not slice memory struct between label and print, uses <see cref="MemAnnotations"/> which uses the memory as a key.</remarks>
        public void MemPrint(ReadOnlyMemory<byte> memory, bool space = true, bool pow2Modulus = true)
        {
            if (!Debug) return;
            HexAnsiPrint.Print(memory.Span,
                MemAnnotations.TryGetValue(memory, out List<(int offset, int length, string label, Color color)> list)
                    ? list.ToArray()
                    : new (int offset, int length, string label, Color color)[0], space, pow2Modulus);
        }

        #endregion
    }
}
