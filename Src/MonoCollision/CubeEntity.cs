using ConsoleApp26;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MonoCollision
{
    public class CubeEntity : IEntity
    {
        public RectangleF Bounds;
        public Vector2 Velocity;

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(Bounds, Color.Red, 3);
        }

        public virtual void Update(GameTime gameTime)
        {
            Bounds.Position += Velocity * gameTime.GetElapsedSeconds() * 50;
        }

        public Collider GetCollider()
        {
            return new Collider(Bounds);
        }

        public virtual void HandleCollision(Collision collision)
        {
            RandomizeVelocity();
            Bounds.Position -= collision.Penetration;
        }

        private void RandomizeVelocity()
        {
            Velocity.X = CollisionGame.Random.Next(-50, 50) / 10f;
            Velocity.Y = CollisionGame.Random.Next(-50, 50) / 10f;
        }
    }
}