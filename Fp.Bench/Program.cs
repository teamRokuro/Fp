using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Running;

namespace Fp.Bench
{
    internal static class Program
    {
        private static void Main()
        {
            if (Avx2.IsSupported)
                BenchmarkRunner.Run<Avx2Bench>();
        }
    }
}
