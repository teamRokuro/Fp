using System.Numerics;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fp.Intermediate.Model {
    /// <summary>
    /// Stores information about bone on a mesh
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct Bone {
        /// <summary>
        /// Type of bone this instance represents
        /// </summary>
        public BoneType Type { get; set; }

        /// <summary>
        /// Name of original bone on mesh
        /// </summary>
        public string BoneName { get; set; }

        /// <summary>
        /// 4x4 float matrix with base transform
        /// </summary>
        public Matrix4x4 BindPose { get; set; }
    }
}
