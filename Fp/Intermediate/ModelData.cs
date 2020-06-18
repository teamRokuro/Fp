using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fp.Intermediate.Model;

namespace Fp.Intermediate
{
    /// <summary>
    /// 3D model data
    /// </summary>
    public class ModelData : Data
    {
        /// <summary>
        /// Mesh of model
        /// </summary>
        public MdlMesh Mesh { get; }

        /// <summary>
        /// Materials used by mesh (ordered for base mesh)
        /// </summary>
        public List<Material> Materials { get; }

        /// <summary>
        /// Create new instance of <see cref="ModelData"/>
        /// </summary>
        /// <param name="basePath">Base path</param>
        /// <param name="mesh">Mesh</param>
        /// <param name="materials">Materials</param>
        public ModelData(string basePath, MdlMesh mesh, List<Material> materials) : base(basePath)
        {
            Mesh = mesh;
            Materials = materials;
        }

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.ExportUnsupported;

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null) => throw new NotSupportedException();

        /// <inheritdoc />
        public override void Dispose()
        {
        }

        /// <inheritdoc />
        public override object Clone() => new ModelData(BasePath, (MdlMesh)Mesh.Clone(),
            new List<Material>(Materials.Select(material => (Material)material.Clone())));
    }
}
