using RobloxRuntime;
using RobloxRuntime.Classes;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            int[] numbers = [1, 2, 3, 4, 5];
            foreach (var number in numbers)
            {
                Console.WriteLine(number);
            }
        }
    }
}