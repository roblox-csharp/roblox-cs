namespace Roblox
{
    public static partial class Globals
    {
        public static class debug
        {
            /// <summary>
            /// <para>Returns a traceback of the current function call stack as a string; in other words, a description of the functions that have been called up to this point. During debugging, this behaves like an error stack trace but does not stop execution of the script.</para>
            /// <para>The level parameter specifies what level of the call stack to consider, with 1 being the call of <see cref="traceback(string, int?)"/> itself, 2 being the call of the function calling <see cref="traceback(string, int?)"/>, and so on.</para>
            /// <para>Note that this function will often return inaccurate results (compared to the original source code) and that the format of the returned traceback may change at any time. You should not parse the return value for specific information such as script names or line numbers.</para>
            /// </summary>
            public static string traceback(string message, int? level = null)
            {
                return null!;
            }

            /// <summary>
            /// <para>Returns a traceback of the current function call stack as a string; in other words, a description of the functions that have been called up to this point. During debugging, this behaves like an error stack trace but does not stop execution of the script.</para>
            /// <para>The level parameter specifies what level of the call stack to consider, with 1 being the call of <see cref="traceback(string, int?)"/> itself, 2 being the call of the function calling <see cref="traceback(string, int?)"/>, and so on.</para>
            /// <para>Note that this function will often return inaccurate results (compared to the original source code) and that the format of the returned traceback may change at any time. You should not parse the return value for specific information such as script names or line numbers.</para>
            /// </summary>
            public static string traceback(Coroutine thread, string message, int? level = null)
            {
                return null!;
            }

            /// <summary>
            /// <para>Allows programmatic inspection of the call stack. This function differs from debug.traceback() in that it guarantees the format of the data it returns.</para>
            /// <para>This is useful for general logging and filtering purposes as well as for sending the data to systems expecting structured input, such as crash aggregation.</para>
            /// </summary>
            public static object[] info(int level, string options) // TODO: use LuaTuple type?
            {
                return null!;
            }

            /// <summary>Starts profiling for a <see href="https://create.roblox.com/docs/studio/microprofiler">MicroProfiler</see> label.</summary>
            /// <param name="label">The text that this <see href="https://create.roblox.com/docs/studio/microprofiler">MicroProfiler</see> label displays.</param>
            public static void profilebegin(string label)
            {
            }

            /// <summary>Stops profiling for the most recent <see href="https://create.roblox.com/docs/studio/microprofiler">MicroProfiler</see> label that <see cref="profilebegin()"/> opened.</summary>
            public static void profilebegin()
            {
            }

            /// <summary>Returns the name of the current thread's active memory category.</summary>
            public static string getmemorycategory()
            {
                return null!;
            }

            /// <summary>
            /// <para>Assigns a custom tag name to the current thread's memory category in the <see href="https://create.roblox.com/docs/studio/developer-console">Developer Console</see>.</para>
            /// <para>Useful for analyzing memory usage of multiple threads in the same script which would otherwise be grouped together under the same tag/name.</para>
            /// <para>Returns the name of the current thread's previous memory category.</para>
            /// </summary>
            public static string setmemorycategory(string tag)
            {
                return null!;
            }

            /// <summary>Resets the tag assigned by <see cref="setmemorycategory(string)"/> to the automatically assigned value (typically, the script name).</summary>
            public static void resetmemorycategory()
            { 
            }

            /// <summary>
            /// <para>Displays a table of native code size of individual functions and scripts. This function is only available in the Command Bar in Studio.</para>
            /// <para>More details can be found on the <see href="https://create.roblox.com/docs/luau/native-code-gen">Native Code Generation</see> page.</para>
            /// </summary>
            public static void dumpcodesize()
            {
            }
        }
    }
}