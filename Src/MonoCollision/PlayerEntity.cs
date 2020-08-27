using ConsoleApp26;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace MonoCollision
{
    public class PlayerEntity : CubeEntity
    {
        private const float Speed = 150;

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle(Bounds, Color.Blue, 3);
        }

        public override void HandleCollision(Collision collision)
        {
            //Do nothing
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W))
            {
                Bounds.Y -= Speed * gameTime.GetElapsedSeconds();
            }

            if (state.IsKeyDown(Keys.S))
            {
                Bounds.Y += Speed * gameTime.GetElapsedSeconds();
            }

            if (state.IsKeyDown(Keys.D))
            {
                Bounds.X += Speed * gameTime.GetElapsedSeconds();
            }

            if (state.IsKeyDown(Keys.A))
            {
                Bounds.X -= Speed * gameTime.GetElapsedSeconds();
            }
        }
    }
}