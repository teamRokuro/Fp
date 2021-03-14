using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Fp
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class FpUtil
    {
        private static readonly HashSet<string> _emptyStringHashSet = new();

        /// <summary>
        /// Isolate flags from argument list.
        /// </summary>
        /// <param name="arguments">Argument lines to sort.</param>
        /// <param name="optKeys">Option keys.</param>
        /// <returns>Flags, options, and arguments.</returns>
        /// <remarks>https://github.com/The-Council-of-Wills/HacknetSharp/blob/58d5ccdf09203a38092b01aba3fea5df3e06d483/src/HacknetSharp.Server/ServerUtil.cs#L436</remarks>
        public static (HashSet<string> flags, Dictionary<string, string> opts, List<string> args) IsolateFlags(
            this IReadOnlyList<string> arguments, IReadOnlyCollection<string>? optKeys = null)
        {
            optKeys ??= _emptyStringHashSet;
            HashSet<string> flags = new();
            Dictionary<string, string> opts = new();
            List<string> args = new();
            bool argTime = false;
            for (int i = 0; i < arguments.Count; i++)
            {
                string? str = arguments[i];
                if (argTime)
                {
                    args.Add(str);
                    continue;
                }

                if (str.Length < 2 || str[0] != '-')
                {
                    args.Add(str);
                    continue;
                }

                if (str[1] == '-')
                {
                    if (str.Length == 2)
                    {
                        argTime = true;
                        continue;
                    }

                    string id = str.Substring(2);
                    if (optKeys.Contains(id))
                    {
                        string? res = GetArg(arguments, i);
                        if (res != null) opts[id] = res;
                        i++;
                    }
                    else
                        flags.Add(id);
                }
                else
                {
                    string firstId = str[1].ToString();
                    if (str.Length == 2 && optKeys.Contains(firstId))
                    {
                        string? res = GetArg(arguments, i);
                        if (res != null) opts[firstId] = res;
                        i++;
                    }
                    else
                        flags.UnionWith(str.Skip(1).Select(c => c.ToString()));
                }
            }

            return (flags, opts, args);
        }

        private static string? GetArg(IReadOnlyList<string> list, int i)
        {
            if (i + 1 < list.Count)
            {
                return list[i + 1];
            }

            return null;
        }

        /// <summary>
        /// Slices an array and allocates a new array.
        /// </summary>
        /// <param name="array">Source.</param>
        /// <param name="start">Start index.</param>
        /// <param name="length">Length.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Allocated array.</returns>
        public static T[] SliceAlloc<T>(this T[] array, int start, int length)
        {
            return array.AsSpan().SliceAlloc(start, length);
        }

        /// <summary>
        /// Slices a span and allocates a new array.
        /// </summary>
        /// <param name="span">Source.</param>
        /// <param name="start">Start index.</param>
        /// <param name="length">Length.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Allocated array.</returns>
        public static T[] SliceAlloc<T>(this Span<T> span, int start, int length)
        {
            return span.Slice(start, length).ToArray();
        }

        /// <summary>
        /// Slices a span and allocates a new array.
        /// </summary>
        /// <param name="span">Source.</param>
        /// <param name="start">Start index.</param>
        /// <param name="length">Length.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Allocated array.</returns>
        public static T[] SliceAlloc<T>(this ReadOnlySpan<T> span, int start, int length)
        {
            return span.Slice(start, length).ToArray();
        }

        /// <summary>
        /// Skip over bits matching specified value.
        /// </summary>
        /// <param name="array">Bit array to use.</param>
        /// <param name="i">Index to modify.</param>
        /// <param name="skipValue">Value to skip over.</param>
        public static void SkipBits(this BitArray array, ref int i, bool skipValue)
        {
            while (i < array.Length && array[i] == skipValue) i++;
        }

        /// <summary>
        /// Dispose of an enumerable's elements after they have been yielded.
        /// </summary>
        /// <param name="enumerable">Enumerable.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Input elements.</returns>
        public static IEnumerable<T?> PostDispose<T>(this IEnumerable<T?> enumerable) where T : IDisposable
        {
            foreach (var x in enumerable)
            {
                yield return x;
                x?.Dispose();
            }
        }

        /// <summary>
        /// Create tuple sequence from enumerable.
        /// </summary>
        /// <param name="enumerable">Enumerable.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Tuples joining contiguous elements (packed, mutually exclusive).</returns>
        public static IEnumerable<ValueTuple<T, T>> Tuplify2<T>(this IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var v0 = enumerator.Current;
                if (!enumerator.MoveNext()) yield break;
                var v1 = enumerator.Current;
                yield return (v0, v1);
            }
        }

        /// <summary>
        /// Create tuple sequence from enumerable.
        /// </summary>
        /// <param name="enumerable">Enumerable.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Tuples joining contiguous elements (packed, mutually exclusive).</returns>
        public static IEnumerable<ValueTuple<T, T, T>> Tuplify3<T>(this IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var v0 = enumerator.Current;
                if (!enumerator.MoveNext()) yield break;
                var v1 = enumerator.Current;
                if (!enumerator.MoveNext()) yield break;
                var v2 = enumerator.Current;
                yield return (v0, v1, v2);
            }
        }

        /// <summary>
        /// Create tuple sequence from enumerable.
        /// </summary>
        /// <param name="enumerable">Enumerable.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Tuples joining contiguous elements (packed, mutually exclusive).</returns>
        public static IEnumerable<ValueTuple<T, T, T, T>> Tuplify4<T>(this IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var v0 = enumerator.Current;
                if (!enumerator.MoveNext()) yield break;
                var v1 = enumerator.Current;
                if (!enumerator.MoveNext()) yield break;
                var v2 = enumerator.Current;
                if (!enumerator.MoveNext()) yield break;
                var v3 = enumerator.Current;
                yield return (v0, v1, v2, v3);
            }
        }

        private static readonly bool _subNormalize = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Normalize the specified path.
        /// </summary>
        /// <param name="path">Path to normalize.</param>
        /// <returns>Normalized path.</returns>
        public static string NormalizeAndStripWindowsDrive(this string path)
        {
            string sub = Path.GetFullPath(path);
            if (_subNormalize && sub.Length >= 2 && char.IsLetter(sub[0]) && sub[1] == Path.VolumeSeparatorChar)
                sub = sub.Substring(2);
            return sub;
        }
    }
}
