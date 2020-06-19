using System;
using System.Collections.Generic;
using Fp;
using Fp.Intermediate;

namespace Fpt {
    [ProcessorInfo("Fpt", "yourDescription", "yourExtendedDescription", "yourExtension1")]
    public class FptProcessor : Processor {
        /*
         * NuGet package:
         * Fp 0.3.6
         */

        // Main function
        public static async System.Threading.Tasks.Task Main(string[] args) =>
            await Coordinator.CliRunFilesystemAsync(Environment.CommandLine, args, Console.WriteLine,
                FileSystemSource.Default, () => new FptProcessor());

        protected override void ProcessImpl(IReadOnlyList<string> args) {
            // Implement your logic here
        }

        // Alternate segmented processing
        /*
        protected override IEnumerable<Data> ProcessSegmentedImpl(IReadOnlyList<string> args) {
            // Implement your logic here
            
        }
        */
    }
}