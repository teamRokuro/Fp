using System;

namespace Fp.Intermediate.Model {
    /// <summary>
    /// Stores animation data for one bone
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields",
        Justification = "<Pending>")]
    public class MdlSubAnim : ICloneable {
        /// <summary>
        /// Bone to be modified
        /// </summary>
        public int BoneId;

        /// <summary>
        /// Number of position entries
        /// </summary>
        public int PositionCount;

        /// <summary>
        /// Position t values
        /// </summary>
        public float[] PositionTimes;

        /// <summary>
        /// Position x values
        /// </summary>
        public float[] PositionX;

        /// <summary>
        /// Position y values
        /// </summary>
        public float[] PositionY;

        /// <summary>
        /// Position z values
        /// </summary>
        public float[] PositionZ;

        /// <summary>
        /// Number of rotation entries
        /// </summary>
        public int RotationCount;

        /// <summary>
        /// Rotation t values
        /// </summary>
        public float[] RotationTimes;

        /// <summary>
        /// Rotation w values
        /// </summary>
        public float[] RotationW;

        /// <summary>
        /// Rotation x values
        /// </summary>
        public float[] RotationX;

        /// <summary>
        /// Rotation y values
        /// </summary>
        public float[] RotationY;

        /// <summary>
        /// Rotation z values
        /// </summary>
        public float[] RotationZ;

        /// <summary>
        /// Number of scaling entries
        /// </summary>
        public int ScalingCount;

        /// <summary>
        /// Scaling t values
        /// </summary>
        public float[] ScalingTimes;

        /// <summary>
        /// Scaling x values
        /// </summary>
        public float[] ScalingX;

        /// <summary>
        /// Scaling y values
        /// </summary>
        public float[] ScalingY;

        /// <summary>
        /// Scaling z values
        /// </summary>
        public float[] ScalingZ;

        /// <inheritdoc />
        public object Clone()
        {
            MdlSubAnim res = new MdlSubAnim
            {
                BoneId = BoneId,
                PositionCount = PositionCount,
                PositionTimes = (float[])PositionTimes?.Clone(),
                PositionX = (float[])PositionX?.Clone(),
                PositionY = (float[])PositionY?.Clone(),
                PositionZ = (float[])PositionZ?.Clone(),
                RotationCount = RotationCount,
                RotationW = (float[])RotationW?.Clone(),
                RotationX = (float[])RotationX?.Clone(),
                RotationY = (float[])RotationY?.Clone(),
                RotationZ = (float[])RotationZ?.Clone(),
                ScalingCount = ScalingCount,
                ScalingTimes = (float[])ScalingTimes?.Clone(),
                ScalingX = (float[])ScalingX?.Clone(),
                ScalingY = (float[])ScalingY?.Clone(),
                ScalingZ = (float[])ScalingZ?.Clone()
            };
            return res;
        }
    }
}
