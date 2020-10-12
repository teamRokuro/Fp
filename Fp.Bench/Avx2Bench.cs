using System;
using BenchmarkDotNet.Attributes;

namespace Fp.Bench
{
    public class Avx2Bench
    {
        private const byte _value = 0xdd;
        private const int _n0 = 1024;
        private const int _n1 = 32 * 1024;
        private const int _n2 = 1024 * 1024;
        private const int _nMax = 8 * 1024 * 1024;
        private readonly byte[] _array;

        [Params(_n0, _n1, _n2, _nMax)] public int N { get; set; }

        public Avx2Bench()
        {
            _array = new byte[_nMax];
        }

        [Benchmark]
        public void XorSse2()
        {
            Processor.ApplyXorSse2(_array.AsSpan(0, N), _value);
        }

        [Benchmark]
        public void XorAvx2()
        {
            Processor.ApplyXorAvx2(_array.AsSpan(0, N), _value);
        }

        [Benchmark]
        public void XorFallback()
        {
            Processor.ApplyXorFallback(_array.AsSpan(0, N), _value);
        }
    }
}
