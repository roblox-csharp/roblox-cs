namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var result = TestBrah.HelloNiga();
            Console.WriteLine($"result: {result}");
        }
    }
}

namespace TestGame.Client
{
    public static class TestBrah
    {
        public static uint HelloNiga()
        {
            return 69;
        }
    }
}