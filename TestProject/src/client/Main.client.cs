using Roblox;
using static Roblox.Globals;

using Components;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var x = 5;
            Console.WriteLine(x);
            task.wait(2);
            Console.WriteLine(math.pow(x, 2));
            ComponentRunner.AttachTag("Lava", instance => new LavaComponent((Part)instance));
        }
    }
}