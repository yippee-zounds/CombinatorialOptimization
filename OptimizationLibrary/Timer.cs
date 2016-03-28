using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class Timer
    {
        private static DateTime st;

        public static void Start()
        {
            st = DateTime.Now;
        }

        public static TimeSpan Time()
        {
            return DateTime.Now - st;
        }
    }
}
