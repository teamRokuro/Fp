using System;

namespace Fp {
    /// <summary>
    /// Row-major raster position helper.
    /// </summary>
    public struct RasterIndexContext
    {
        /// <summary>
        /// Width.
        /// </summary>
        public readonly int W;
        /// <summary>
        /// Height.
        /// </summary>
        public readonly int H;
        /// <summary>
        /// X position.
        /// </summary>
        public int X;
        /// <summary>
        /// Y position.
        /// </summary>
        public int Y;

        /// <summary>
        /// Creates new instance of <see cref="RasterIndexContext"/>.
        /// </summary>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        /// <param name="x">Initial x position.</param>
        /// <param name="y">Initial y position.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when w &lt; 0 or h &lt; 0</exception>
        public RasterIndexContext(int w, int h, int x = 0, int y = 0)
        {
            if (w < 0) throw new ArgumentOutOfRangeException(nameof(w));
            if (h < 0) throw new ArgumentOutOfRangeException(nameof(h));
            W = w;
            H = h;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Checks if provided length is available.
        /// </summary>
        /// <param name="length">Length.</param>
        /// <returns>True if available.</returns>
        public bool IsAvailable(int length) => Y * W + X + length <= Y * H;

        /// <summary>
        /// Checks if specified state is valid.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="endIsOk">Allow end position to return true.</param>
        /// <returns>False if invalid.</returns>
        public bool IsValid(int x, int y, bool endIsOk = true) =>
            x >= 0 && y >= 0 && (x == 0 && y == H && endIsOk || x < W && y < H);

        /// <summary>
        /// Advance context.
        /// </summary>
        /// <param name="amount">Amount to advance by.</param>
        /// <param name="endIsOk">Allow end position to return true.</param>
        /// <param name="dontCommitInvalid">Do not commit state if invalid.</param>
        /// <returns>True if final state is valid.</returns>
        public bool Advance(int amount, bool endIsOk = true, bool dontCommitInvalid = false)
        {
            int y2;
            int x2;
            if (amount > 0)
            {
                y2 = Y + amount / W;
                x2 = X + amount % W;
                if (x2 >= W)
                {
                    y2++;
                    x2 -= W;
                }
            }
            else
            {
                y2 = Y - -amount / W;
                x2 = X - -amount % W;
                if (X < 0)
                {
                    y2--;
                    x2 += W;
                }
            }

            if (!dontCommitInvalid) return IsValid(X = x2, Y = y2, endIsOk);
            if (!IsValid(x2, y2, endIsOk)) return false;
            X = x2;
            Y = y2;
            return true;
        }

        /// <summary>
        /// Implicit cast to liner position in raster (row-major).
        /// </summary>
        /// <param name="value">Context.</param>
        /// <returns>Linear position.</returns>
        public static implicit operator int(RasterIndexContext value) => value.Y * value.W + value.X;
    }
}
