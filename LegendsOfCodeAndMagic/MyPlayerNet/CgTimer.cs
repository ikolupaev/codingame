using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsOfCodeAndMagic
{
    public static class CgTimer
    {
        static int timeoutMs = 0;

        static bool timeout = false;
        static public Stopwatch turnTimer = new Stopwatch();
        static int ticks;

        public static void Reset(int timeoutMs)
        {
            CgTimer.timeoutMs = timeoutMs;
            ticks = 0;
            timeout = false;
            turnTimer.Restart();
        }

        public static void Tick()
        {
            ticks++;
        }

        public static bool IsTimeout()
        {
            if (!timeout)
            {
                timeout = turnTimer.ElapsedMilliseconds > timeoutMs;
                if( timeout )
                {
                    Log("timeout");
                }
            }

            return timeout;
        }

        public static void Log(string message)
        {
            CgPlayer.D($"{message}. {ticks} ticks in {turnTimer.ElapsedMilliseconds} ms");
        }
    }
}
