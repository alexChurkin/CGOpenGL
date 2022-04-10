using System;
using System.Collections.Generic;
using System.Text;

namespace OpenGLHeart
{
    public static class Time
    {
        public static long CurrentMillis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
