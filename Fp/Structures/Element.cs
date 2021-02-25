using System.Collections.Generic;

namespace Fp.Structures
{
    public abstract record Element
    {
        public abstract IEnumerable<Element> GetDependencies();
    }
}
