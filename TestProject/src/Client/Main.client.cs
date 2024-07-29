namespace TestGame.Client
{
    public enum Shape
    {
        Circle = 1,
        Quadrilateral,
        Triangle
    }

    public static class Game
    {
        public static void Main()
        {
            Console.WriteLine(Shape.Quadrilateral + 5);
        }
    }
}