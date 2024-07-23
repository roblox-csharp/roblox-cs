using Components;
using Roblox;
using static Roblox.Globals;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var square = new Square(5);
            Console.WriteLine(square.GetArea());
            ComponentRunner.AttachTag("Lava", instance => new LavaComponent((Part)instance));
        }
    }

    public sealed class Square : Rectangle
    {
        public Square(float size)
            : base(size, size)
        {
        }
    }

    public class Rectangle
    {
        public float Height { get; } = 0;
        public float Width { get; } = 0;

        public Rectangle(float height, float width)
        {
            Height = height;
            Width = width;
        }

        public float GetArea()
        {
            return Height * Width;
        }
    }
}