using RobloxRuntime.Classes;

using Components;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            ComponentRunner.AttachTag("Lava", instance => new LavaComponent((Part)instance));
        }
    }
}