using System.Runtime.InteropServices;
using MonoGame.Extended;

namespace MonoCollision
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ShapeUnion
    {
        [FieldOffset(0)]
        public RectangleF Rectangle;
        [FieldOffset(0)]
        public CircleF Circle;
    }
}