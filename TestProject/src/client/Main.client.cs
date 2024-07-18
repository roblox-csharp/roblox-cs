using Roblox;
using static Roblox.Globals;

namespace TestGame.Client
{
    public static class Game
    {
        public static void Main()
        {
            var @abc = 5;
            Console.WriteLine(@abc);
            // var vec1 = new Vector4(3, 0, 6, 0);
            // var vec2 = new Vector4(0, 2, 0, 9);
            // var vec3 = vec1 + vec2;
            // Console.WriteLine(vec3);
        }
    }

    // just some test code for future metamethod stuff

    // class Vector4
    // {
    //     public float X { get; }
    //     public float Y { get; }
    //     public float Z { get; }
    //     public float W { get; }
    // 
    //     public Vector4(float x = 0, float y = 0, float z = 0, float w = 0)
    //     {
    //         X = x;
    //         Y = y;
    //         Z = z;
    //         W = w;
    //     }
    // 
    //     public static Vector4 operator +(Vector4 a, Vector4 b)
    //     {
    //         return new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    //     }
    // 
    //     public override string ToString()
    //     {
    //         return $"{X}, {Y}, {Z}, {W}";
    //     }
    // }
}