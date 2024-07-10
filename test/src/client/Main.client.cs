using System;
using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var part = Instance.Create<Part>();
            Console.WriteLine(part.Size.ToString());
        }
    }
}