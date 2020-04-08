using System;
using System.Threading.Tasks;
using Fp;

namespace Fpt {
    public static class Program {
        public static async Task Main(string[] args)
            => await Coordinator.CliRunFilesystemAsync(Environment.CommandLine, args, Console.WriteLine,
                FileSystemSource.Default, FptProcessor.CreateDefault);
    }
}