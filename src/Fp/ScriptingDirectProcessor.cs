using System;

namespace Fp
{
    /// <summary>
    /// Represents a processor designed for scripting.
    /// </summary>
    internal sealed class ScriptingDirectProcessor : Processor
    {
        private readonly Action _func;

        /// <summary>
        /// Creates a new instance of <see cref="ScriptingDirectProcessor"/>.
        /// </summary>
        /// <param name="func">Processing function.</param>
        public ScriptingDirectProcessor(Action func)
        {
            _func = func;
        }

        /// <inheritdoc />
        protected override void ProcessImpl()
        {
            OpenFile();
            _func();
        }
    }
}
