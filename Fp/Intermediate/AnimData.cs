using System;
using System.Collections.Generic;
using System.IO;
using Fp.Intermediate.Model;

namespace Fp.Intermediate
{
    /// <summary>
    /// 3D model animation data
    /// </summary>
    public class AnimData : Data
    {
        /// <summary>
        /// Animation
        /// </summary>
        public MdlAnim Anim { get; }

        /// <summary>
        /// Create new instance of <see cref="AnimData"/>
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="anim"></param>
        public AnimData(string basePath, MdlAnim anim) : base(basePath)
        {
            Anim = anim;
        }

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.ExportUnsupported;

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void Dispose() {
        }

        /// <inheritdoc />
        public override object Clone() => new AnimData(BasePath, (MdlAnim)Anim.Clone());
    }
}
