using Roblox;
using Components;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            ComponentRunner.AttachTag("Lava", instance =>
            {
                var lava = new LavaComponent((Part)instance);
                Console.WriteLine(lava);
                return lava;
            });
        }
    }
}