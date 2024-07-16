namespace RobloxRuntime
{
    public sealed class OsLib
    {
        /// <summary>Returns a high-precision amount of CPU time used by Lua in seconds, intended for use in benchmarking.</summary>
        public double clock()
        {
            return default;

        }

        /// <summary>Returns how many seconds have passed since the Unix epoch (1 January 1970, 00:00:00) under current UTC time.</summary>
        public uint time(time_t? time = null)
        {
            return default;
        }

        /// <summary>Returns the number of seconds from t1 to t2, assuming the arguments are correctly casted to the time_t format.</summary>
        public uint difftime(float t2, float t1)
        {
            return default;
        }

        /// <summary>Formats the given string with date/time information based on the given time (or if not provided, the value returned by os.time).</summary>
        public string date(string? formatString = null, float? time = null)
        {
            return null!;
        }

        /// <summary>Formats the given string with date/time information based on the given time (or if not provided, the value returned by os.time).</summary>
        public time_t date(string formatString, float time)
        {
            return null!;
        }
    }
}