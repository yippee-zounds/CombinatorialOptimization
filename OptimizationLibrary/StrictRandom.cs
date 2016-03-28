using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class StrictRandom
    {
        private static Random r = new Random();
        
        public static double Next()
        {
            return r.NextDouble();
        }

        public static int Next(int maxValue)
        {
            return r.Next(maxValue);
        }


        public static int Next(int minValue, int maxValue)
        {
            return r.Next(minValue, maxValue);
        }
    }
}
