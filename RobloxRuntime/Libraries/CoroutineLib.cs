namespace Roblox
{
    public sealed class Coroutine
    { 
    }

    public static partial class Globals
    {
        public static class coroutine
        {
            /// <summary>Closes and puts the provided coroutine in a dead state.</summary>
            public static (bool, string?) close(Coroutine co)
            {
                return default;
            }

            /// <summary>Creates a new coroutine, with body f. f must be a Lua function.</summary>
            public static Coroutine create(Delegate f)
            {
                return null!;
            }

            /// <summary>Returns true if the coroutine this function is called within can safely yield.</summary>
            public static bool isyieldable(Coroutine co)
            {
                return default;
            }

            /// <summary>Starts or continues the execution of coroutine co.</summary>
            public static (bool, object[]) resume(Coroutine co, params object[] args)
            {
                return default;
            }

            /// <summary>Returns the running coroutine.</summary>
            public static Coroutine running()
            {
                return null!;
            }

            /// <summary>Returns the status of coroutine co as a string.</summary>
            public static string status(Coroutine co)
            {
                return null!;
            }

            /// <summary>Creates a new coroutine and returns a function that, when called, resumes the coroutine.</summary>
            public static TFunc wrap<TFunc>(TFunc f) where TFunc : Delegate
            {
                return null!;
            }

            /// <summary>Suspends execution of the coroutine.</summary>
            public static object[] yield(params object[] args)
            {
                return null!;
            }
        }
    }
}