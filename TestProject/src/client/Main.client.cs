using System;
using RobloxRuntime;
using RobloxRuntime.Classes;
using static RobloxRuntime.Globals;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var part = Instance.Create<Part>(game.Workspace);
            Console.WriteLine(part.IsA<Part>());
        }
    }
}