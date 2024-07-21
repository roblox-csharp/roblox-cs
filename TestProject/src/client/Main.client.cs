using Roblox;
using static Roblox.Globals;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var vector = new Vector4();
            Console.WriteLine(vector);
        }
    }

    public sealed class Vector4
    {
        public float X { get; } = 0;
        public float Y { get; } = 0;
        public float Z { get; } = 0;
        public float W { get; } = 0;

        public string ToString()
        {
            return $"{X}, {Y}, {Z}, {W}";
        }
    }
}