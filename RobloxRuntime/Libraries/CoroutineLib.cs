namespace RobloxRuntime
{
    public sealed class Coroutine
    { 
    }

    public sealed class CoroutineLib
    {
        /// <summary>Closes and puts the provided coroutine in a dead state.</summary>
        public (bool, string?) close(Coroutine co)
        {
            return default;
        }

        /// <summary>Creates a new coroutine, with body f. f must be a Lua function.</summary>
        public Coroutine create(Delegate f)
        {
            return null!;
        }

        /// <summary>Returns true if the coroutine this function is called within can safely yield.</summary>
        public bool isyieldable(Coroutine co)
        {
            return default;
        }

        /// <summary>Starts or continues the execution of coroutine co.</summary>
        public (bool, object[]) resume(Coroutine co, params object[] args)
        {
            return default;
        }

        /// <summary>Returns the running coroutine.</summary>
        public Coroutine running()
        {
            return null!;
        }

        /// <summary>Returns the status of coroutine co as a string.</summary>
        public string status(Coroutine co)
        {
            return null!;
        }

        /// <summary>Creates a new coroutine and returns a function that, when called, resumes the coroutine.</summary>
        public TFunc wrap<TFunc>(TFunc f) where TFunc : Delegate
        {
            return null!;
        }

        /// <summary>Suspends execution of the coroutine.</summary>
        public object[] yield(params object[] args)
        {
            return null!;
        }
    }
}