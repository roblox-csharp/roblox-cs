using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var players = Services.Players.GetPlayers();
            foreach (var player in players)
            {
                Console.WriteLine(player.Name);
            }
        }
    }
}