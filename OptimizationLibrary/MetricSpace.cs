using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class MetricSpace : IMetric
    {
        private int maxSize = int.MaxValue - 1;
        private List<IMetric> set = new List<IMetric>();

        public IEnumerable<IMetric> Element()
        {
            return this.set;
        }

        public int DistanceTo(IMetric m)
        {
            MetricSpace ms = (MetricSpace)m;

            return this.Center().Item1.DistanceTo(ms.Center().Item1);
        }

        public IEnumerable<IMetric> SelectDiversity(int maxSize)
        {
            if (set.Count() <= maxSize)
            {
                return set;
            }
            else
            {
                IEnumerable<IMetric> ret = null;
                double variance = 0.0;

                for (int i = 0; i < 10; i++)
                {
                    MetricSpace tmp = new MetricSpace(set.RandomSubset(set.Count(), maxSize));
                    if(variance < tmp.Variance())
                    {
                        variance = tmp.Variance();
                        ret = tmp.set;
                    }
                }

                return ret;
            }
        }

        public Tuple<IMetric, IMetric, int> InsideDiameter()
        {
            int dm = int.MaxValue;
            IMetric m1 = null;
            IMetric m2 = null;

            for (int i = 0; i < set.Count() - 1; i++)
            {
                for (int j = i + 1; j < set.Count(); j++)
                {
                    if (set[i].DistanceTo(set[j]) < dm)
                    {
                        dm = set[i].DistanceTo(set[j]);
                        m1 = set[i];
                        m2 = set[j];
                    }
                }
            }

            return new Tuple<IMetric, IMetric, int>(m1, m2, dm);
        }

        public IEnumerable<IMetric> Periphery()
        {
            var c = this.Center().Item1;
            int rd = this.Center().Item2;

            for (int i = 0; i < set.Count(); i++)
            {
                if (c.DistanceTo(set[i]) == rd)
                {
                    yield return set[i];
                }
            }
        }

        public double Variance()
        {
            if (set.Count() == 0) return 0.0;

            int ret = 0;
            IMetric c = this.Center().Item1;

            for (int i = 0; i < set.Count(); i++)
            {
                ret += c.DistanceTo(set[i]) * c.DistanceTo(set[i]);
            }
            
            return (double)ret / set.Count();
        }

        public Tuple<IMetric, int> Center()
        {
            int rd = int.MaxValue;
            IMetric m = null;

            if (set.Count() == 1) return new Tuple<IMetric, int>(set[0], 0);

            for (int i = 0; i < set.Count(); i++)
            {
                int tmp = int.MinValue;

                for (int j = 0; j < set.Count(); j++)
                {
                    if (tmp < set[i].DistanceTo(set[j]))
                    {
                        tmp = set[i].DistanceTo(set[j]);
                    }
                }

                if (tmp < rd)
                {
                    rd = tmp;
                    m = set[i];
                }
            }

            return new Tuple<IMetric, int>(m, rd);
        }

        public Tuple<IMetric, IMetric, int> SimilarPair()
        {
            int dm = int.MaxValue;
            IMetric m1 = null;
            IMetric m2 = null;

            for (int i = 0; i < set.Count() - 1; i++)
            {
                for (int j = i + 1; j < set.Count(); j++)
                {
                    if (set[i].DistanceTo(set[j]) < dm)
                    {
                        dm = set[i].DistanceTo(set[j]);
                        m1 = set[i];
                        m2 = set[j];
                    }
                }
            }

            return new Tuple<IMetric, IMetric, int>(m1, m2, dm);
        }

        public Tuple<IMetric, IMetric, int> Diameter()
        {
            int dm = int.MinValue;
            IMetric m1 = null;
            IMetric m2 = null;

            for(int i = 0; i < set.Count() - 1; i++)
            {
                for(int j = i + 1; j < set.Count(); j++)
                {
                    if (dm < set[i].DistanceTo(set[j]))
                    {
                        dm = set[i].DistanceTo(set[j]);
                        m1 = set[i];
                        m2 = set[j];
                    }
                }
            }

            return new Tuple<IMetric, IMetric, int>(m1, m2, dm);
        }

        public void Add(IMetric m)
        {
            set.Add(m);
            if (this.maxSize < set.Count())
            {
                set.RemoveAt(0);
            }
        }

        public void Remove(IMetric m)
        {
            for (int i = 0; i < set.Count(); i++)
            {
                if (m == set[i]) set.RemoveAt(i);
            }
        }

        public MetricSpace()
        {
            this.set = new List<IMetric>();
        }

        public MetricSpace(int maxSize)
        {
            this.maxSize = maxSize;
        }

        public MetricSpace(IEnumerable<IMetric> set)
        {
            foreach(var m in set)
            {
                this.Add(m);
            }
        }

        public MetricSpace(IEnumerable<IMetric> set, int maxSize)
        {
            this.maxSize = maxSize;

            foreach (var m in set)
            {
                this.Add(m);
            }
        }
    }
}
