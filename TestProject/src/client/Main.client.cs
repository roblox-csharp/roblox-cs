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
            part.CanCollide = false;
            part.Position = character.PrimaryPart!.Position;

            var runtime = Services.RunService;
            var goalColor = Color3.fromRGB(255, 0, 0);
            var alpha = 0.005f;
            runtime.RenderStepped.Connect(dt => part.Color = part.Color.Lerp(goalColor, alpha));
        }
    }
}