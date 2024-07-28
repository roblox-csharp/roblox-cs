using Roblox;

namespace TestGame.Client
{
    public static class Game
    {
        [Native]
        public static void Main()
        {
            DoShit();
        }

        [Native]
        public static object DoShit()
        {
            var name = "billy";
            switch (name)
            {
                case "joanna":
                case "milly":
                case "mary" when name.Length == 4:
                    var msg = "wtf";
                    Console.WriteLine(msg);
                    break;
                case "bob":
                    Console.WriteLine("yay!");
                    break;
                default:
                    Console.WriteLine("who are you?!");
                    break;
            }
            return null!;
        }
    }
}