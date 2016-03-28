using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class SimulatedAnnealing : IOptimizationAlgorithm
    {
        private double initialT;
        private double coolingRate;
        //private int maxAnnealing = -1;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            int coolingInterval = p.Size * 2;
            int maxAnnealing = coolingInterval * 160;
            int totalAnnealing = 0;
            ISolution s = sol.Clone();
            double t = initialT;
            List<ISolution> ss = new List<ISolution>();
            int n = p.OperationSet().Count();

            w.WriteLine("loop:t:vx:doptx:vdom:ddomx:r10:r100:r1000:dom:x");

            while(totalAnnealing < maxAnnealing)
            {
                for (int i = 0; i < coolingInterval; i++)
                {
                    IOperation op = p.OperationSet().RandomSubset(n, 1).First();
                    int delta = p.OperationValue(op, s);

                    if(delta < 0)
                    {
                        s = s.Apply(op);
                    }
                    else if(StrictRandom.Next() <= Math.Exp(-delta / t))
                    {
                        s = s.Apply(op);
                    }

                    ISolution dom = new LocalSearch().Solve(p, s);
                    ss.Add(s.Clone());
                    double r10 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 10, 0))) / Math.Min(2 * Math.Min(ss.Count(), 10), p.Size);
                    double r100 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 100, 0))) / Math.Min(2 * Math.Min(ss.Count(), 100), p.Size);
                    //double r1000 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 1000, 0))) / Math.Min(2 * Math.Min(ss.Count(), 1000), p.Size);

                    w.WriteLine(Trace(totalAnnealing, t, p.Optimum, dom, r10, r100, 0, s));

                    ++totalAnnealing;
                }


                t = t * coolingRate;
            }

            return s;
        }

        private string Trace(int totalAnnealing, double t, ISolution opt, ISolution x)
        {
            return totalAnnealing + ":" + t + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }
        private string Trace(int totalAnnealing, double t, ISolution opt, ISolution dom, ISolution x)
        {
            return totalAnnealing + ":" + t + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + x;
        }
        private string Trace(int totalAnnealing, double t, ISolution opt, ISolution dom, double r10, double r100, double r1000, ISolution x)
        {
            return totalAnnealing + ":" + t + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + r10.ToString("F4") + ":" + r100.ToString("F4") + ":" + r1000.ToString("F4") + ":" + dom + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[T=" + initialT + ",r=" + coolingRate + "]";// +",total=" + (this.p.Size * 20 * 160) + "]";
        }

        public SimulatedAnnealing(double initialT, double coolingRate)
        {
            this.initialT = initialT;
            this.coolingRate = coolingRate;
        }
    }
}
