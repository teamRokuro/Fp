using System;

namespace Fp
{
    /// <summary>
    /// Represents a processor that will set <see cref="Processor.Current"/> when run.
    /// </summary>
    internal sealed class CurrentDirectProcessor : Processor
    {
        private readonly Action _func;
        private bool _proc;

        /// <summary>
        /// Creates a new instance of <see cref="CurrentDirectProcessor"/>.
        /// </summary>
        /// <param name="func">Processing function.</param>
        public CurrentDirectProcessor(Action func)
        {
            _func = func;
        }

        /// <inheritdoc />
        protected override void ProcessImpl()
        {
            if (_proc) return;
            _proc = true;
            _current = this;
            try
            {
                _func();
            }
            finally
            {
                _current = null;
            }
        }
    }
}
