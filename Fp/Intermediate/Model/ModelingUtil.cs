using System;
using System.Collections.Generic;

namespace Fp.Intermediate.Model {
    /// <summary>
    /// Model manipulation utilities
    /// </summary>
    public static class ModelUtil {
        /// <summary>
        /// Refit mesh against mesh variant originally used
        /// </summary>
        /// <param name="baseVariant">Original variant</param>
        /// <param name="targetVariant">Target variant</param>
        /// <param name="sourceMesh">Mesh to recreate</param>
        /// <param name="malleable">Optional pre-allocated mesh to modify</param>
        /// <returns>Newly allocated refit mesh</returns>
        public static MdlMesh Refit(MdlMesh baseVariant, MdlMesh targetVariant, MdlMesh sourceMesh,
            MdlMesh malleable = null) {
            if (baseVariant == null)
                throw new ArgumentNullException(nameof(baseVariant));
            if (targetVariant == null)
                throw new ArgumentNullException(nameof(targetVariant));
            if (sourceMesh == null)
                throw new ArgumentNullException(nameof(sourceMesh));
            var res = malleable ?? (MdlMesh) sourceMesh.Clone();
            res.FitUniqueName = baseVariant.UniqueName;
            Span<float> delta = stackalloc float[3];
            foreach (var sub in res.SubMeshes) {
                for (int iSubMesh = 0; iSubMesh < sub.VertexCount; iSubMesh++) {
                    int closestBaseIdx = 0;
                    int closestBaseVert = 0;
                    float closestLenSq = float.MaxValue;
                    for (int iBaseIdx = 0; iBaseIdx < baseVariant.SubMeshes.Length; iBaseIdx++) {
                        var baseSubMesh = baseVariant.SubMeshes[iBaseIdx];
                        for (int iBaseVert = 0; iBaseVert < baseSubMesh.VertexCount; iBaseVert++) {
                            float curLenSq = SqDist(sub.Vertices, iSubMesh, baseSubMesh.Vertices, iBaseVert, 3);
                            if (!(curLenSq < closestLenSq)) continue;
                            closestBaseIdx = iBaseIdx;
                            closestBaseVert = iBaseVert;
                            closestLenSq = curLenSq;
                        }
                    }

                    // Placeholder basic single-vertex delta
                    Sub(baseVariant.SubMeshes[closestBaseIdx].Vertices, closestBaseVert,
                        targetVariant.SubMeshes[closestBaseIdx].Vertices, closestBaseVert, delta, 0, 3);
                    Add(sub.Vertices, iSubMesh, delta, 0, sub.Vertices, iSubMesh, 3);
                    // Determine face
                    // Procedural transformation
                }
            }

            return res;
        }

        /// <summary>
        /// Synchronize bones of a mesh with a prototype mesh
        /// </summary>
        /// <param name="protoMesh">Prototype to match bones against</param>
        /// <param name="sourceMesh">Mesh to recreate</param>
        /// <param name="res">Newly allocated bone synchronized mesh</param>
        /// <param name="malleable">Optional pre-allocated mesh to modify</param>
        /// <returns>True if success (all source mesh bones had matching prototype mesh bones)</returns>
        public static bool BoneSync(MdlMesh protoMesh, MdlMesh sourceMesh, out MdlMesh res,
            MdlMesh malleable = null) {
            if (protoMesh == null)
                throw new ArgumentNullException(nameof(protoMesh));
            if (sourceMesh == null)
                throw new ArgumentNullException(nameof(sourceMesh));
            res = malleable ?? (MdlMesh) sourceMesh.Clone();
            var boneMap = MapBones(protoMesh.Bones);
            foreach (var sub in res.SubMeshes) {
                for (int i = 0; i < sub.VertexCount; i++) {
                    for (int j = 0; j < 4; j++) {
                        // Ignore zero weights
                        if (Math.Abs(sub.BoneWeights[4 * i + j]) < float.Epsilon) continue;
                        if (boneMap.TryGetValue(sourceMesh.Bones[sub.BoneIds[4 * i + j]].Type, out int newIdx))
                            sub.BoneIds[4 * i + j] = newIdx;
                        else
                            return false;
                    }
                }
            }

            res.Bones = new Bone[protoMesh.Bones.Length];
            Array.Copy(protoMesh.Bones, res.Bones, protoMesh.Bones.Length);
            return true;
        }

        /*
        /// <summary>
        /// Offset height values of animation based on skeletons
        /// </summary>
        public static void AnimRetargetHeight() {
            // TODO implement AnimRetargetHeight
            // Retarget vertical offsets of animation keys for new skeleton
            // Requires runtime evaluation of body
            // TODO test other functions to check behaviour of animation
        }
        */

        private static Dictionary<BoneType, int> MapBones(Bone[] bones) {
            var res = new Dictionary<BoneType, int>();
            for (int i = 0; i < bones.Length; i++)
                res[bones[i].Type] = i;
            return res;
        }

        private static float SqDist(Span<float> lhs, int lhsOffs, Span<float> rhs, int rhsOffs, int stride) {
            float res = 0.0f;
            for (int i = 0; i < stride; i++) {
                float tmp = lhs[stride * lhsOffs + i] - rhs[stride * rhsOffs + i];
                res += tmp * tmp;
            }

            return res;
        }

        private static void Add(Span<float> lhs, int lhsOffs, Span<float> rhs, int rhsOffs, Span<float> res,
            int resOffs,
            int stride) {
            for (int i = 0; i < stride; i++)
                res[stride * resOffs + i] = lhs[stride * lhsOffs + i] + rhs[stride * rhsOffs + i];
        }

        private static void Sub(Span<float> lhs, int lhsOffs, Span<float> rhs, int rhsOffs, Span<float> res,
            int resOffs,
            int stride) {
            for (int i = 0; i < stride; i++)
                res[stride * resOffs + i] = lhs[stride * lhsOffs + i] - rhs[stride * rhsOffs + i];
        }
    }
}
