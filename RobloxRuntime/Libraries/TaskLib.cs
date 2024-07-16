namespace RobloxRuntime
{
    public sealed class TaskLib
    {
        /// <summary>Calls/resumes a function/coroutine immediately through the engine scheduler.</summary>
        public Coroutine spawn(Delegate f, params object[] args)
        {
            return null!;
        }

        /// <summary>Calls/resumes a function/coroutine immediately through the engine scheduler.</summary>
        public Coroutine spawn(Coroutine thread, params object[] args)
        {
            return null!;
        }

        /// <summary>Calls/resumes a function/coroutine on the next resumption cycle.</summary>
        public Coroutine defer(Delegate f, params object[] args)
        {
            return null!;
        }

        /// <summary>Calls/resumes a function/coroutine on the next resumption cycle.</summary>
        public Coroutine defer(Coroutine thread, params object[] args)
        {
            return null!;
        }

        /// <summary>Schedules a function/coroutine to be called/resumed on the next Heartbeat after the given duration (in seconds) has passed, without throttling.</summary>
        public Coroutine delay(float duration, Delegate f, params object[] args)
        {
            return null!;
        }

        /// <summary>Schedules a function/coroutine to be called/resumed on the next Heartbeat after the given duration (in seconds) has passed, without throttling.</summary>
        public Coroutine delay(float duration, Coroutine thread, params object[] args)
        {
            return null!;
        }

        /// <summary>Causes the following code to be run in parallel.</summary>
        public void desynchronize()
        {
        }

        /// <summary>Causes the following code to be run in serial.</summary>
        public void synchronize()
        { 
        }

        /// <summary>Yields the current thread until the next Heartbeat in which the given duration (in seconds) has passed, without throttling.</summary>
        public float wait(float time)
        {
            return default;
        }

        /// <summary>Cancels a thread, preventing it from being resumed.</summary>
        public void cancel(Coroutine thread)
        {
        }
    }
}