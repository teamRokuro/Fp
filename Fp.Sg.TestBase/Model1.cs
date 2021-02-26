using Fp.Structures;

namespace Fp.Sg.TestBase
{
    public class Model1 : Structure
    {
        public static i4 Ref0 = li4(0);
        public static i4 Ref1 = li4(8);
        public static i4 Ref2 = li4(Ref0);
        public static i4 Ref2_2 = li4(Ref0 + 8);
    }
}
