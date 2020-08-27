using ConsoleApp26;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MonoCollision
{
    public class BallEntity : IEntity
    {
        public CircleF Bounds;
        public Vector2 Velocity;

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawCircle(Bounds, 8, Color.Red, 3f);
        }

        public void Update(GameTime gameTime)
        {
            Bounds.Position += Velocity * gameTime.GetElapsedSeconds() * 30;
        }

        public Collider GetCollider()
        {
            return new Collider(Bounds);
        }

        public void HandleCollision(Collision collision)
        {
            RandomizeVelocity();
            Bounds.Position -= collision.Penetration;
        }

        private void RandomizeVelocity()
        {
            Velocity.X = CollisionGame.Random.Next(-50, 50) / 10f;
            Velocity.Y = CollisionGame.Random.Next(-50, 50) / 10f;
        }

        public bool Equals(IEntity other)
        {
            return ReferenceEquals(this, other);
        }
    }
}