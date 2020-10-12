using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Running;

namespace Fp.Bench
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (Avx2.IsSupported)
                BenchmarkRunner.Run<Avx2Bench>();
        }
    }
}
