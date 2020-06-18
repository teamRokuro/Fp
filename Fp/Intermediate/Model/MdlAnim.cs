using System;

namespace Fp.Intermediate.Model {
    /// <summary>
    /// Stores animation data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    public class MdlAnim : ICloneable {
        /// <summary>
        /// The bones modified in this animation
        /// </summary>
        public Bone[] Bones;

        /// <summary>
        /// Animations per bone
        /// </summary>
        public MdlSubAnim[] BoneSubAnims;

        /// <inheritdoc />
        public object Clone()
        {
            MdlAnim res = new MdlAnim();
            res.Bones = (Bone[])Bones?.Clone();

            if (BoneSubAnims != null)
            {
                res.BoneSubAnims = new MdlSubAnim[BoneSubAnims.Length];
                for (int i = 0; i < BoneSubAnims.Length; i++)
                    res.BoneSubAnims[i] = (MdlSubAnim)BoneSubAnims[i].Clone();
            }

            return res;
        }
    }
}
