using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var player = Services.Players.LocalPlayer;
            var character = player.Character ?? player.CharacterAdded.Wait();
            var part = Instance.Create<Part>(character);
            part.Anchored = true;
            part.Position = new Vector3(0, -1, 0);
        }
    }
}