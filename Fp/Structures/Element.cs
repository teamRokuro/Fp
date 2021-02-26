using System.Collections.Generic;
using System.Linq;

namespace Fp.Structures
{
    public abstract record Element
    {
        public IEnumerable<Element> GetDependencies() =>
            new[] {this}.Concat(Dependencies.SelectMany(d => d.GetDependencies()));

        public virtual IEnumerable<Element> Dependencies => Enumerable.Empty<Element>();
    }
}
