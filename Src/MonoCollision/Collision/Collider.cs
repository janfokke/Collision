using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using MonoCollision;
using MonoGame.Extended;

namespace ConsoleApp26
{
    public readonly struct Collider
    {
        public Collider(CircleF circle, byte collisionMask = 1, byte collisionLayers = 1)
        {
            CollisionLayers = collisionLayers;
            CollisionMask = collisionMask;
            Shape = new ShapeUnion {Circle = circle};
            ShapeType = ShapeType.Circle;
        }

        public Collider(RectangleF rectangle, byte collisionMask = 1, byte collisionLayers = 1)
        {
            CollisionLayers = collisionLayers;
            CollisionMask = collisionMask;
            Shape = new ShapeUnion {Rectangle = rectangle};
            ShapeType = ShapeType.Rectangle;
        }

        internal readonly byte CollisionMask;
        internal readonly byte CollisionLayers;
        internal readonly ShapeType ShapeType;
        internal readonly ShapeUnion Shape;

        public Vector2 CalculatePenetrationVector(in Collider other)
        {
            return ShapeType switch
            {
                ShapeType.Rectangle when other.ShapeType == ShapeType.Rectangle => PenetrationVector(Shape.Rectangle,
                    other.Shape.Rectangle),
                ShapeType.Circle when other.ShapeType == ShapeType.Circle => PenetrationVector(Shape.Circle,
                    other.Shape.Circle),
                ShapeType.Circle when other.ShapeType == ShapeType.Rectangle => PenetrationVector(Shape.Circle,
                    other.Shape.Rectangle),
                ShapeType.Rectangle when other.ShapeType == ShapeType.Circle => PenetrationVector(Shape.Rectangle,
                    other.Shape.Circle),
                _ => throw new NotSupportedException("Shapes must be either a CircleF or RectangleF")
            };
        }

        public bool Intersects(in Collider other)
        {
            return ShapeType switch
            {
                ShapeType.Rectangle when other.ShapeType == ShapeType.Rectangle => Shape.Rectangle.Intersects(
                    other.Shape.Rectangle),
                ShapeType.Circle when other.ShapeType == ShapeType.Circle =>
                Shape.Circle.Intersects(other.Shape.Circle),
                ShapeType.Circle when other.ShapeType == ShapeType.Rectangle => Intersects(Shape.Circle,
                    other.Shape.Rectangle),
                ShapeType.Rectangle when other.ShapeType == ShapeType.Circle => Intersects(Shape.Rectangle,
                    other.Shape.Circle),
                _ => throw new NotSupportedException("Shapes must be either a CircleF or RectangleF")
            };
        }

        public bool Intersects(RectangleF rectangle)
        {
            return ShapeType switch
            {
                ShapeType.Rectangle => rectangle.Intersects(Shape.Rectangle),
                ShapeType.Circle => Intersects(Shape.Circle, rectangle),
                _ => throw new NotSupportedException("Shapes must be either a CircleF or RectangleF")
            };
        }

        public bool Intersects(CircleF circle)
        {
            return ShapeType switch
            {
                ShapeType.Circle => circle.Intersects(Shape.Circle),
                ShapeType.Rectangle => Intersects(Shape.Rectangle, circle),
                _ => throw new NotSupportedException("Shapes must be either a CircleF or RectangleF")
            };
        }

        private static bool Intersects(CircleF circle, RectangleF rectangle)
        {
            Point2 closestPoint = rectangle.ClosestPointTo(circle.Center);
            return circle.Contains(closestPoint);
        }

        private static Vector2 PenetrationVector(RectangleF rect1, RectangleF rect2)
        {
            RectangleF intersectingRectangle = RectangleF.Intersection(rect1, rect2);
            Debug.Assert(!intersectingRectangle.IsEmpty,
                "Violation of: !intersect.IsEmpty; Rectangles must intersect to calculate a penetration vector.");

            Vector2 penetration;
            if (intersectingRectangle.Width < intersectingRectangle.Height)
            {
                var d = rect1.Center.X < rect2.Center.X
                    ? intersectingRectangle.Width
                    : -intersectingRectangle.Width;
                penetration = new Vector2(d, 0);
            }
            else
            {
                var d = rect1.Center.Y < rect2.Center.Y
                    ? intersectingRectangle.Height
                    : -intersectingRectangle.Height;
                penetration = new Vector2(0, d);
            }

            return penetration;
        }

        private static Vector2 PenetrationVector(CircleF circ1, CircleF circ2)
        {
            if (!circ1.Intersects(circ2))
            {
                return Vector2.Zero;
            }

            Vector2 displacement = Point2.Displacement(circ1.Center, circ2.Center);

            Vector2 desiredDisplacement;
            if (displacement != Vector2.Zero)
            {
                desiredDisplacement = displacement.NormalizedCopy() * (circ1.Radius + circ2.Radius);
            }
            else
            {
                desiredDisplacement = -Vector2.UnitY * (circ1.Radius + circ2.Radius);
            }

            Vector2 penetration = displacement - desiredDisplacement;
            return penetration;
        }

        private static Vector2 PenetrationVector(CircleF circ, RectangleF rect)
        {
            Point2 collisionPoint = rect.ClosestPointTo(circ.Center);
            Vector2 cToCollPoint = collisionPoint - circ.Center;

            if (rect.Contains(circ.Center) || cToCollPoint.Equals(Vector2.Zero))
            {
                Vector2 displacement = Point2.Displacement(circ.Center, rect.Center);

                Vector2 desiredDisplacement;
                if (displacement != Vector2.Zero)
                {
                    // Calculate penetration as only in X or Y direction.
                    // Whichever is lower.
                    var dispx = new Vector2(displacement.X, 0);
                    var dispy = new Vector2(0, displacement.Y);
                    dispx.Normalize();
                    dispy.Normalize();

                    dispx *= circ.Radius + rect.Width / 2;
                    dispy *= circ.Radius + rect.Height / 2;

                    if (dispx.LengthSquared() < dispy.LengthSquared())
                    {
                        desiredDisplacement = dispx;
                        displacement.Y = 0;
                    }
                    else
                    {
                        desiredDisplacement = dispy;
                        displacement.X = 0;
                    }
                }
                else
                {
                    desiredDisplacement = -Vector2.UnitY * (circ.Radius + rect.Height / 2);
                }

                Vector2 penetration = displacement - desiredDisplacement;
                return penetration;
            }
            else
            {
                Vector2 penetration = circ.Radius * cToCollPoint.NormalizedCopy() - cToCollPoint;
                return penetration;
            }
        }

        private static Vector2 PenetrationVector(RectangleF rect, CircleF circ)
        {
            return -PenetrationVector(circ, rect);
        }
    }
}