using ConsoleApp26;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoCollision
{
    public interface IEntity : ICollisionActor
    {
        void Draw(SpriteBatch spriteBatch);
        void Update(GameTime gameTime);
    }
}