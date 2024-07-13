namespace TestGame.Client
{
    using MyOtherNamespace;

    public static class Game
    {
        public static void Main()
        {
            var result = TestBrah.HelloNiga();
            System.Console.WriteLine($"result: {result}");
        }
    }
}

namespace MyOtherNamespace
{
    public static class TestBrah
    {
        public static uint HelloNiga()
        {
            return 69;
        }
    }
}