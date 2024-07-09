using RobloxRuntime.Types;
using System;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var rect = new Rectangle(4.0f, 3.0f);
            Console.WriteLine(Math.Pow(4, 2));
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

        public float Area()
        {
            return Width * Height;
        }
    }
}