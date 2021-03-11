using System;

namespace Fp
{
    /// <summary>
    /// Exception thrown during execution of <see cref="Processor"/>'s <see cref="Processor.ProcessImpl"/> function
    /// </summary>
    public class ProcessorException : Exception
    {
        /// <summary>
        /// Initialize exception with message
        /// </summary>
        /// <param name="message">Textual information about exception</param>
        public ProcessorException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialize exception with message
        /// </summary>
        /// <param name="message">Textual information about exception</param>
        /// <param name="innerException">Exception that was thrown inside processor</param>
        public ProcessorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
