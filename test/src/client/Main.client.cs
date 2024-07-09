using RobloxRuntime;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var rect = new Rectangle(4.0f, 3.0f);
            Console.Print(rect.Width.ToString());
        }
    }

    public class Rectangle
    {
        public readonly float Width;
        public readonly float Height;

        public Rectangle(float width, float height)
        {
            Width = width;
            Height = height;
        }
    }
}