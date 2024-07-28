using Roblox;

namespace TestGame.Client
{
    public static class Game
    {
        [Native]
        public static void Main()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
        }
    }
}