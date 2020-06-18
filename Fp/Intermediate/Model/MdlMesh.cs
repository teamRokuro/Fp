using System;

namespace Fp.Intermediate.Model {
    /// <summary>
    /// Stores mesh data
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    public sealed class MdlMesh : ICloneable {
        /// <summary>
        /// Bones in this mesh
        /// </summary>
        public Bone[] Bones;

        /// <summary>
        /// Sub-meshes in this mesh
        /// </summary>
        public MdlSubMesh[] SubMeshes;

        /// <summary>
        /// This mesh's unique name
        /// </summary>
        public string UniqueName;

        /// <summary>
        /// The base variant type name
        /// </summary>
        public string VariantTypeName;

        /// <summary>
        /// The unique name of the mesh originally fit against
        /// </summary>
        public string FitUniqueName;

        /// <inheritdoc />
        public object Clone() {
            var res = new MdlMesh {
                Bones = new Bone[Bones.Length],
                SubMeshes = new MdlSubMesh[SubMeshes.Length],
                UniqueName = UniqueName,
                VariantTypeName = VariantTypeName,
                FitUniqueName = FitUniqueName
            };
            Array.Copy(Bones, res.Bones, Bones.Length);
            for (int i = 0; i < SubMeshes.Length; i++)
                res.SubMeshes[i] = (MdlSubMesh) SubMeshes[i].Clone();
            return res;
        }
    }
}
