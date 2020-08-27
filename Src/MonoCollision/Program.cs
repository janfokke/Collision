using System;

namespace MonoCollision
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using var game = new CollisionGame();
            game.Run();
        }
    }
}