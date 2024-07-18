using Roblox;
using static Roblox.Globals;

using Components;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var s = utf8.@char(16, 2, 24, 86);
            var n = ToUInt("0xE16D");
            Console.WriteLine(s);
            Console.WriteLine(n);
            // ComponentRunner.AttachTag("Lava", instance => new LavaComponent((Part)instance));
        }
    }
}