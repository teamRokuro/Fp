using System.Collections.Generic;
using Fp;

namespace Fpt {
    public class FptProcessor : Processor {
        /*
         * NuGet package:
         * Fp 0.1.1
         */
        
        // Sample main function
        //public static async System.Threading.Tasks.Task Main(string[] args) => await Coordinator.CliRunFilesystemAsync(System.Environment.CommandLine, args, System.Console.WriteLine, FileSystemSource.Default, CreateDefault);

        public static (string path, string name, Processor processor) CreateDefault() => ("", "", new FptProcessor());

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