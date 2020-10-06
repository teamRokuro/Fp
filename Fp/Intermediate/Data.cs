using System;
using System.Collections.Generic;
using System.IO;

namespace Fp.Intermediate
{
    /// <summary>
    /// Intermediate-format data container
    /// </summary>
    public abstract class Data : IDisposable, ICloneable
    {
        /// <summary>
        /// Base path of resource
        /// </summary>
        public readonly string BasePath;

        /// <summary>
        /// Default format for container
        /// </summary>
        public abstract CommonFormat DefaultFormat { get; }

        /// <summary>
        /// If true, object does not contain complete data, e.g. for <see cref="WriteConvertedData"/>
        /// </summary>
        public bool Dry { get; protected set; }

        /// <summary>
        /// Create instance of <see cref="Data"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        protected Data(string basePath)
        {
            BasePath = basePath;
        }

        /// <summary>
        /// Get stream of data converted to common file format
        /// </summary>
        /// <param name="outputStream">Target stream</param>
        /// <param name="format">Requested file format</param>
        /// <param name="formatOptions">Format-specific options</param>
        /// <returns>False if requested format is not supported</returns>
        public abstract bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<object, object>? formatOptions = null);

        /// <inheritdoc />
        public abstract void Dispose();

        /// <inheritdoc />
        public abstract object Clone();

        #region Code copied / tweaked from ProjectOtori

        /*
         * ProjectOtori\Assets\Astolfo\CadmonCommon.cs
         * Obtained 10/6/2020
         * TODO copy back modifications to astolfo codebase for convenience
         * (supports boxed input, automatically redirects string to parsed)
         */

        /// <summary>
        /// Cast number
        /// </summary>
        /// <param name="value">Input value</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Converted value</returns>
        /// <exception cref="ApplicationException"></exception>
        public static unsafe TOut CastNumber<TIn, TOut>(TIn value)
            where TOut : unmanaged
        {
            if (value?.GetType() == typeof(string))
            {
                if (typeof(TOut) == typeof(float) || typeof(TOut) == typeof(double))
                    return CastNumber<double, TOut>(double.Parse(value.ToString()!));
                if (typeof(TOut) == typeof(ulong))
                    return CastNumber<ulong, TOut>(ulong.Parse(value.ToString()!));
                return CastNumber<long, TOut>(long.Parse(value.ToString()!));
            }

            TOut target;
            if (typeof(TOut) == typeof(byte))
            {
                *(byte*)&target = CastByte(value);
                return target;
            }

            if (typeof(TOut) == typeof(sbyte))
            {
                *(sbyte*)&target = CastSByte(value);
                return target;
            }

            if (typeof(TOut) == typeof(ushort))
            {
                *(ushort*)&target = CastUShort(value);
                return target;
            }

            if (typeof(TOut) == typeof(short))
            {
                *(short*)&target = CastShort(value);
                return target;
            }

            if (typeof(TOut) == typeof(uint))
            {
                *(uint*)&target = CastUInt(value);
                return target;
            }

            if (typeof(TOut) == typeof(int))
            {
                *(int*)&target = CastInt(value);
                return target;
            }

            if (typeof(TOut) == typeof(ulong))
            {
                *(ulong*)&target = CastULong(value);
                return target;
            }

            if (typeof(TOut) == typeof(long))
            {
                *(long*)&target = CastLong(value);
                return target;
            }

            if (typeof(TOut) == typeof(float))
            {
                *(float*)&target = CastFloat(value);
                return target;
            }

            if (typeof(TOut) == typeof(double))
            {
                *(double*)&target = CastDouble(value);
                return target;
            }

            throw new ApplicationException($"Unsupported output type {typeof(TOut)}");
        }

        private static byte CastByte<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => (byte)b,
                ushort b => (byte)b,
                short b => (byte)b,
                uint b => (byte)b,
                int b => (byte)b,
                ulong b => (byte)b,
                long b => (byte)b,
                float b => (byte)b,
                double b => (byte)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(byte)}")
            };
        }

        private static sbyte CastSByte<TValue>(TValue number)
        {
            return number switch
            {
                byte b => (sbyte)b,
                sbyte b => b,
                ushort b => (sbyte)b,
                short b => (sbyte)b,
                uint b => (sbyte)b,
                int b => (sbyte)b,
                ulong b => (sbyte)b,
                long b => (sbyte)b,
                float b => (sbyte)b,
                double b => (sbyte)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(sbyte)}")
            };
        }

        private static ushort CastUShort<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => (ushort)b,
                ushort b => b,
                short b => (ushort)b,
                uint b => (ushort)b,
                int b => (ushort)b,
                ulong b => (ushort)b,
                long b => (ushort)b,
                float b => (ushort)b,
                double b => (ushort)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(ushort)}")
            };
        }

        private static short CastShort<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => b,
                ushort b => (short)b,
                short b => b,
                uint b => (short)b,
                int b => (short)b,
                ulong b => (short)b,
                long b => (short)b,
                float b => (short)b,
                double b => (short)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(short)}")
            };
        }

        private static uint CastUInt<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => (uint)b,
                ushort b => b,
                short b => (uint)b,
                uint b => b,
                int b => (uint)b,
                ulong b => (uint)b,
                long b => (uint)b,
                float b => (uint)b,
                double b => (uint)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(uint)}")
            };
        }

        private static int CastInt<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => b,
                ushort b => b,
                short b => b,
                uint b => (int)b,
                int b => b,
                ulong b => (int)b,
                long b => (int)b,
                float b => (int)b,
                double b => (int)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(int)}")
            };
        }


        private static ulong CastULong<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => (ulong)b,
                ushort b => b,
                short b => (ulong)b,
                uint b => b,
                int b => (ulong)b,
                ulong b => b,
                long b => (ulong)b,
                float b => (ulong)b,
                double b => (ulong)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(ulong)}")
            };
        }

        private static long CastLong<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => b,
                ushort b => b,
                short b => b,
                uint b => b,
                int b => b,
                ulong b => (long)b,
                long b => b,
                float b => (long)b,
                double b => (long)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(long)}")
            };
        }

        private static float CastFloat<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => b,
                ushort b => b,
                short b => b,
                uint b => b,
                int b => b,
                ulong b => b,
                long b => b,
                float b => b,
                double b => (float)b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(float)}")
            };
        }

        private static double CastDouble<TValue>(TValue number)
        {
            return number switch
            {
                byte b => b,
                sbyte b => b,
                ushort b => b,
                short b => b,
                uint b => b,
                int b => b,
                ulong b => b,
                long b => b,
                float b => b,
                double b => b,
                _ => throw new InvalidCastException(
                    $"Could not cast from type {number?.GetType().FullName ?? "null"} to {typeof(double)}")
            };
        }

        #endregion
    }
}
