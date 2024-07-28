using Roblox;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            [Native]
            void test()
            {
                Console.WriteLine("abc");
            }
            test();
        }
    }
}