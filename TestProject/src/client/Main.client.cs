using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var player = Services.Players?.LocalPlayer;
            Console.WriteLine(player);
        }
    }
}