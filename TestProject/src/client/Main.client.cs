namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var config = new Config();
            config.DoSomeCoolStuff = true;
            config.AnAwesomeProgrammer = "CharSiewGuy";

            Console.WriteLine($"the awesomest programmer: {config.AnAwesomeProgrammer}");
        }
    }

    public class Config
    {
        public bool DoSomeCoolStuff { get; set; }
        public string? AnAwesomeProgrammer { get; set; }
    }
}