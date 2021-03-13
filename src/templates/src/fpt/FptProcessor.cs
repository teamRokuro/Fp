using System;
using System.Collections.Generic;
using Fp;

namespace Fpt
{
    [ProcessorInfo("Fpt", "yourDescription", "yourExtendedDescription", "yourExtension1")]
    public class FptProcessor : Processor
    {
        // Main function
        public static async System.Threading.Tasks.Task Main(string[] args) =>
            await Coordinator.CliRunFilesystemAsync<FptProcessor>(args);

        protected override void ProcessImpl()
        {
            // Implement your logic here

        }

        // Alternate segmented processing
        /*
        protected override IEnumerable<Data> ProcessSegmentedImpl()
        {
            // Implement your logic here
            
        }
        */
    }
}