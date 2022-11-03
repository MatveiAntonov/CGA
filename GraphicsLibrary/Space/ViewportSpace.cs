using System.Numerics;

namespace GraphicsLibrary.Space {
    public class ViewportSpace {
        public Matrix4x4 TransposeMatrix { get; }
        public ViewportSpace(float width, float height, float xmin, float ymin) {
            TransposeMatrix = Matrix4x4.Transpose(new Matrix4x4 {
                M11 = width / 2,
                M22 = -height / 2,
                M33 = 1,
                M44 = 1,
                M14 = xmin + width / 2,
                M24 = ymin + height / 2
            });
        }
    }
}