using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.OptimizationLibrary
{
    public static class Extend
    {
        public static IEnumerable<T> Shuffle<T>(this System.Collections.Generic.IEnumerable<T> e)
        {
            return e.OrderBy((x) => Guid.NewGuid());
        }

        public static IEnumerable<T> RandomSubset<T>(this System.Collections.Generic.IEnumerable<T> e, int n, int k)
        {
            int t = 0;
            int m = 0;

            foreach (var v in e)
            {
                if (StrictRandom.Next() * (n - t) < k - m)
                {
                    yield return v;
                    ++m;
                }
                ++t;
            }
            //return e.Shuffle().Take(k);
        }

        public static IEnumerable<T> RandomSubset<T>(this System.Collections.Generic.IEnumerable<T> e, int k)
        {
            //throw (new NotImplementedException());
            return e.Shuffle().Take(k);
        }

        public static IEnumerable<T> ArgMinStrictSequence<T>(this System.Collections.Generic.IEnumerable<T> e, Func<T, int> f)
        {
            var m = new MinimumKeeper();
            return e.Where((x) => m.IsMinimumStrict(f(x)));
        }

        public static T ArgMinStrictWhere<T>(this System.Collections.Generic.IEnumerable<T> e, Func<T, int> f, Predicate<T> p)
        {
            var m = new MinimumKeeper();
            return e.Where((x) => m.IsMinimumStrict(f(x))).Last((x) => p(x));
        }

        public static T ArgMinStrict<T>(this System.Collections.Generic.IEnumerable<T> e, Func<T, int> f)
        {
            var m = new MinimumKeeper();
            return e.Last((x) => m.IsMinimumStrict(f(x)));
        }

        public static T ArgMin<T>(this System.Collections.Generic.IEnumerable<T> e, Func<T, int> f)
        {
            var m = new MinimumKeeper();
            return e.Last((x) => m.IsMinimum(f(x)));
        }

        public static double StandardDeviation(this System.Collections.Generic.IEnumerable<int> e)
        {
            return Math.Sqrt(e.Select((x) => Math.Pow(x, 2)).Average() - Math.Pow(e.Average(), 2));
        }

        public static double StandardDeviation<T>(this System.Collections.Generic.IEnumerable<T> e, Func<T, int> f)
        {
            return Math.Sqrt(e.Select((x) => Math.Pow(f(x), 2)).Average() - Math.Pow(e.Select((x) => f(x)).Average(), 2));
        }
    }
}
