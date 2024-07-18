namespace Roblox
{
    public sealed class Buffer
    { 
    }

    public static partial class Globals
    {
        public static class buffer
        {
            /// <summary>Creates a buffer.</summary>
            public static Buffer create(uint size)
            {
                return null!;
            }

            /// <summary>Creates a buffer from a string.</summary>
            public static Buffer fromstring(string str)
            {
                return null!;
            }

            /// <summary>Converts a buffer to a string.</summary>
            public static string tostring(Buffer b)
            {
                return null!;
            }

            /// <summary>SReturns the size of the buffer in bytes.</summary>
            public static uint len(Buffer b)
            {
                return default;
            }

            /// <summary>Reads the data from the buffer by reinterpreting bytes at the offset as an 8-bit signed integer and converting it into a number.</summary>
            public static sbyte readi8(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>Reads the data from the buffer by reinterpreting bytes at the offset as an 8-bit unsigned integer and converting it into a number.</summary>
            public static byte readu8(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>Reads the data from the buffer by reinterpreting bytes at the offset as a 16-bit signed integer and converting it into a number.</summary>
            public static short readi16(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>Reads the data from the buffer by reinterpreting bytes at the offset as a 16-bit unsigned integer and converting it into a number.</summary>
            public static ushort readu16(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>Reads the data from the buffer by reinterpreting bytes at the offset as a 32-bit signed integer and converting it into a number.</summary>
            public static int readi32(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>Reads the data from the buffer by reinterpreting bytes at the offset as a 32-bit unsigned integer and converting it into a number.</summary>
            public static uint readu32(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>
            /// <para>Reads the data from the buffer by reinterpreting bytes at the offset as a 32-bit floating-point value and converting it into a number.</para>
            /// <para>If the floating-point value matches any bit patterns that represent NaN (not a number), the returned value may be converted to a different quiet NaN representation.</para>
            /// </summary>
            public static float readf32(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>
            /// <para>Reads the data from the buffer by reinterpreting bytes at the offset as a 64-bit floating-point value and converting it into a number.</para>
            /// <para>If the floating-point value matches any bit patterns that represent NaN (not a number), the returned value may be converted to a different quiet NaN representation.</para>
            /// </summary>
            public static double readf64(Buffer b, uint offset)
            {
                return default;
            }

            /// <summary>Writes data to the buffer by converting the number into an 8-bit signed integer and writing a single byte.</summary>
            public static void writei8(Buffer b, uint offset, sbyte value)
            { 
            }

            /// <summary>Writes data to the buffer by converting the number into an 8-bit unsigned integer and writing a single byte.</summary>
            public static void writeu8(Buffer b, uint offset, byte value)
            {
            }

            /// <summary>Writes data to the buffer by converting the number into a 16-bit signed integer and reinterpreting it as individual bytes.</summary>
            public static void writei16(Buffer b, uint offset, short value)
            {
            }

            /// <summary>Writes data to the buffer by converting the number into a 16-bit unsigned integer and reinterpreting it as individual bytes.</summary>
            public static void writeu16(Buffer b, uint offset, ushort value)
            {
            }

            /// <summary>Writes data to the buffer by converting the number into a 32-bit signed integer and reinterpreting it as individual bytes.</summary>
            public static void writei32(Buffer b, uint offset, int value)
            {
            }

            /// <summary>Writes data to the buffer by converting the number into a 32-bit unsigned integer and reinterpreting it as individual bytes.</summary>
            public static void writeu32(Buffer b, uint offset, uint value)
            {
            }

            /// <summary>Writes data to the buffer by converting the number into a 32-bit floating-point value and reinterpreting it as individual bytes.</summary>
            public static void writef32(Buffer b, uint offset, float value)
            {
            }

            /// <summary>Writes data to the buffer by converting the number into a 64-bit floating-point value and reinterpreting it as individual bytes.</summary>
            public static void writef64(Buffer b, uint offset, double value)
            {
            }

            /// <summary>Reads a string of length count from the buffer at the specified offset.</summary>
            public static string readstring(Buffer b, uint offset, uint count)
            {
                return null!;
            }

            /// <summary>Writes data from a string into the buffer at the specified offset. If an optional count is specified, only count bytes are taken from the string.</summary>
            public static void writestring(Buffer b, uint offset, string value, uint count)
            {
            }

            /// <summary>
            /// <para>Copies count bytes from source starting at offset sourceOffset into the target at targetOffset.</para>
            /// <para>It's possible for source and target to be the same. Copying an overlapping region inside the same buffer acts as if the source region is copied into a temporary buffer and then that buffer is copied over to the target.</para>
            /// </summary>
            public static void copy(Buffer target, uint targetOffset, Buffer source, uint sourceOffset, uint count)
            { 
            }

            /// <summary>Sets count bytes in the buffer starting at the specified offset to value.</summary>
            public static void fill(Buffer b, uint offset, byte value, uint count)
            { 
            }
        }
    }
}