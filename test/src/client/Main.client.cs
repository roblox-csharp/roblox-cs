using RobloxRuntime;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            Console.Print("hello world");
        }
    }

    public class Abc
    {
        private readonly int _xyz = 5;
        private readonly int _blah;

        public Abc(int blah)
        {
            _blah = blah;
        }
    }
}