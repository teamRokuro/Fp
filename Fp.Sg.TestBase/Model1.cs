using Fp.Structures;
using Fp.Structures.Elements.Primitives;

namespace Fp.Sg.TestBase
{
    public class Model1 : Structure
    {
        public static readonly S32V Value1 = 4;
        public static readonly S32 Ref1 = S32L(8);
        public static readonly S32 Ref2 = S32L(Value1);
        public static readonly object Dummy;
    }
}
