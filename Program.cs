//using var game = new EscapeRoom.Game1();
//game.Run();

using System;

namespace EscapeRoom
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new Game1();
            game.Run();
        }
    }
}