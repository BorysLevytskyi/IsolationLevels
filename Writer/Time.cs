using System;

namespace Writer
{
    public static class Time
    {
        public static TimeSpan Sec(this int n)
        {
            return TimeSpan.FromSeconds(n);
        }

        public static TimeSpan Ms(this int n)
        {
            return TimeSpan.FromMilliseconds(n);
        }
    }
}