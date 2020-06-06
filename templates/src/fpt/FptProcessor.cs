using System.Collections.Generic;
using Fp;

namespace Fpt {
    [ProcessorInfo("Fpt", "yourDescription", "yourExtendedDescription", "yourExtension1")]
    public class FptProcessor : Processor {
        /*
         * NuGet package:
         * Fp 0.3.0
         */

        // Main function
        public static async System.Threading.Tasks.Task Main(string[] args) =>
            await Coordinator.CliRunFilesystemAsync(System.Environment.CommandLine, args, System.Console.WriteLine,
                FileSystemSource.Default, () => new FptProcessor());

        protected override void ProcessImpl(IReadOnlyList<string> args) {
            // Implement your logic here
        }

        // Alternate segmented processing
        /*
        protected override IEnumerable<(string path, byte[] buffer, int offset, int length)> ProcessSegmentedImpl(IReadOnlyList<string> args) {
            // Implement your logic here
            
        }
        */
    }
}