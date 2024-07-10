using System;
using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var key = Enum.KeyCode.ButtonA;
            Console.WriteLine(key.ToString());
        }
    }
}