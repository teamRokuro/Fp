namespace Fp.Images.Png {
    /// <summary>
    /// Png palette data
    /// </summary>
    public class Palette {
        /// <summary>
        /// Palette data
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <param name="data">Palette data</param>
        public Palette(byte[] data) {
            Data = data;
        }

        /// <summary>
        /// Get color value for palette
        /// </summary>
        /// <param name="index">Position</param>
        /// <returns>Color value</returns>
        public Pixel GetPixel(int index) {
            var start = index * 3;

            return new Pixel(Data[start], Data[start + 1], Data[start + 2], 255, false);
        }
    }
}