using System;
using System.Collections.Generic;

namespace Fp
{
    /// <summary>
    /// Represents a processor that will set <see cref="Processor.Current"/> when run.
    /// </summary>
    internal sealed class CurrentSegmentedProcessor : Processor
    {
        private readonly IEnumerator<Data> _enumerator;

        /// <summary>
        /// Creates a new instance of <see cref="CurrentSegmentedProcessor"/>.
        /// </summary>
        /// <param name="enumerator">Current enumerator.</param>
        public CurrentSegmentedProcessor(Func<IEnumerable<Data>> enumerator)
        {
            _enumerator = enumerator().GetEnumerator();
        }

        /// <inheritdoc />
        protected override IEnumerable<Data> ProcessSegmentedImpl()
        {
            bool has;
            do
            {
                _current = this;
                try
                {
                    has = _enumerator.MoveNext();
                }
                finally
                {
                    _current = null;
                }

                if (has) yield return _enumerator.Current!;
            } while (has);
        }
    }
}
