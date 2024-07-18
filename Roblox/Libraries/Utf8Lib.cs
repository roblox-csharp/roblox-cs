namespace Roblox
{
    public static partial class Globals
    {
        public static class utf8
        {
            /// <summary>The pattern "[%z\x01-\x7F\xC2-\xF4][\x80-\xBF]*", which matches exactly zero or more UTF-8 byte sequence, assuming that the subject is a valid UTF-8 string.</summary>
            public static string charpattern { get; set; } = null!;

            /// <summary>Receives zero or more codepoints as integers, converts each one to its corresponding UTF-8 byte sequence and returns a string with the concatenation of all these sequences.</summary>
            public static string @char(params uint[] codepoints)
            {
                return null!;

            }

            /// <summary>
            /// <para>Returns an iterator function so that the construction:</para>
            /// <code>
            /// for position, codepoint in utf8.codes(str) do
            /// 	-- body
            /// end
            /// </code>
            /// <para>will iterate over all codepoints in string str. It raises an error if it meets any invalid byte sequence.</para>
            /// </summary>
            public static IEnumerable<(uint, uint)> codes(string str)
            {
                return null!;
            }

            /// <summary>
            /// <para>Returns the codepoints (as integers) from all codepoints in the provided string (str) that start between byte positions i and j (both included).</para>
            /// <para>The default for i is 1 and for j is i. It raises an error if it meets any invalid byte sequence.</para>
            /// </summary>
            public static uint[] codepoint(string s, int? i = null, int? j = null) // TODO: use LuaTuple type?
            {
                return null!;
            }

            /// <summary>
            /// <para>Returns the number of UTF-8 codepoints in the string str that start between positions i and j (both inclusive).</para>
            /// <para>The default for i is 1 and for j is -1. If it finds any invalid byte sequence, returns a nil value plus the position of the first invalid byte.</para>
            /// </summary>
            public static uint len(string s, int? i = null, int? j = null)
            {
                return default;
            }

            /// <summary>
            /// <para>Returns the position (in bytes) where the encoding of the n‑th codepoint of s (counting from byte position i) starts. A negative n gets characters before position i.</para>
            /// <para>The default for i is 1 when n is non-negative and #s + 1 otherwise, so that utf8.offset(s, -n) gets the offset of the n‑th character from the end of the string.</para>
            /// <para>If the specified character is neither in the subject nor right after its end, the function returns nil.</para>
            /// </summary>
            public static uint offset(string s, int n, int? i = null)
            {
                return default;
            }

            /// <summary>
            /// <para>Returns an iterator function so that:</para>
            /// <code>
            /// for first, last in utf8.graphemes(str) do
            ///     local grapheme = s:sub(first, last)
            ///     -- body
            /// end
            /// </code>
            /// <para>will iterate the grapheme clusters of the string.</para>
            /// </summary>

            public static IEnumerable<(string, string)> graphemes(string sr, int i, int j)
            {
                return null!;
            }

            /// <summary>Converts the input string to Normal Form C, which tries to convert decomposed characters into composed characters.</summary>
            public static string nfcnormalize(string str)
            {
                return null!;
            }

            /// <summary>Converts the input string to Normal Form D, which tries to break up composed characters into decomposed characters.</summary>
            public static string nfdnormalize(string str)
            {
                return null!;
            }
        }
    }
}