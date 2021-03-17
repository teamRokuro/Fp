using System;
using System.Collections.Generic;
using Fp;

Processor.Run<FptProcessor>(args,
    "Fpt",
    "yourDescription",
    ".yourExtension1");
public class FptProcessor : Processor
{
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

// Segmented, automatic OpenFile()
/*
public class FptProcessor : Processor
{
    protected override IEnumerable<Data> ProcessData()
    {
        // Implement your logic here
        
    }
}
*/