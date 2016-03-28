using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class RandomWalk : IOptimizationAlgorithm
    {
        private int loop;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        private double calcRatio(ISolution s, List<ISolution> ss, int n)
        {
            return (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 2, 0)));
        }

        private double calcCorrelation(ISolution origin, ISolution sa, ISolution sb)
        {
            double dist_oa = origin.DistanceTo(sa);
            double dist_ob = origin.DistanceTo(sb);
            double dist_ab = sa.DistanceTo(sb);
            double ret = (dist_oa * dist_oa + dist_ob * dist_ob - dist_ab * dist_ab) / (2.0 * dist_oa * dist_ob);

            if (double.IsNaN(ret))
            {
                return 0.0;
            }
            else
            {
                return ret;
            }
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            int sizeOfNeighborhood = p.OperationSet().Count();
            //sol = p.Optimum;
            ISolution ret = sol.Clone();
            ISolution s = sol.Clone();
            List<ISolution> doms = new List<ISolution>();
            List<ISolution> ss = new List<ISolution>();
            Dictionary<String, int> dic = new Dictionary<String, int>();

            w.WriteLine("loop:vx:doptx:vdom:ddomx:doptdom:dom:x");

            for (int loop = 0; loop < this.loop; loop++)
            {
                ISolution dom = new LocalSearch().Solve(p, s);

                doms.Add(dom);
                ss.Add(s.Clone());

                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, 512).ArgMinStrict((op) => p.OperationValue(op, s));
                s = s.Apply(bestOp);

                w.WriteLine(loop + ":" + s.Value + ":" + s.DistanceTo(p.Optimum) + ":" + dom.Value + ":" + s.DistanceTo(dom) + ":" + dom.DistanceTo(p.Optimum) + ":" + dom + ":" + s);

                double ssAverage = ss.Skip(Math.Max(ss.Count() - 1000, 0)).Select(t => (double)t.Value).Average();
                IEnumerable<ISolution> e = doms.Skip(Math.Max(ss.Count() - 1000, 0));
                double domsAverage = e.Select(t => (double)t.Value).Average();
                int domsMin = e.Select(t => t.Value).Min();
                int domsCount = e.Select(t => t.ToString()).Distinct().Count();
                Console.WriteLine(loop + ":" + s.Value + ":" + dom.Value + ":" + domsMin + ":" + domsCount + ":" + domsAverage.ToString() + ":" + (ssAverage - domsAverage) + ":" + s.DistanceTo(p.Optimum) + ":" + s.DistanceTo(dom) + ":" + dom.DistanceTo(p.Optimum) + ":" + s.DistanceTo(sol) + ":" + (s.Value - dom.Value));
            }
            Console.Write(doms.Last().DistanceTo(sol) + "  ");
               
            return s;
        }

        public ISolution Solve(IOptimizationProblem p, IOperationSet ops, ISolution sol, DataStoringWriter w)
        {
            ISolution s = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();

            w.WriteLine("loop:vx:doptx:x");

            for (int loop = 0; true; loop++)
            {
                w.WriteLine(Trace(loop, p.Optimum, s));

                IOperation bestOp = ops.ArgMinStrict((op) => p.OperationValue(op, s));
                ISolution tmp = s.Apply(bestOp);

                if (mk.IsNotMinimumStrict(tmp.Value))
                    return s;

                s = tmp;
            }
        }

        private string Trace(int loop, ISolution opt, ISolution x)
        {
            return loop + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, Diameter d1, Diameter d2, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + d1.DiameterValue + ":" + d1.RadiusValue + ":" + d2.DiameterValue + ":" + d2.RadiusValue + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, double r10, double r100, double rr10, double rr100, ISolution x)
        {
            return loop + ":" + ratio.ToString("F5") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + r10.ToString("F5") + ":" + r100.ToString("F5") + ":" + rr10.ToString("F5") + ":" + rr100.ToString("F5") + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, double r10, double r100, double r1000, double rr10, double rr100, double rr1000, int dm10, int dm100, int dm1000, ISolution x)
        {
            return loop + ":" + ratio.ToString("F5") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + r10.ToString("F5") + ":" + r100.ToString("F5") + ":" + r1000.ToString("F5") + ":" + rr10.ToString("F5") + ":" + rr100.ToString("F5") + ":" + rr1000.ToString("F5") + ":" + dm10 + ":" + dm100 + ":" + dm1000 + ":" + x;
        }

        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, Diameter d1, Diameter d2, int d10, int d100, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + d1.DiameterValue + ":" + d2.DiameterValue + ":" + d10 + ":" + d100 + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[loop=" + loop + "]";
        }

        public RandomWalk(int loop)
        {
            this.loop = loop;
        }
    }
}