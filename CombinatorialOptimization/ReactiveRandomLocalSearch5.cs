using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // RRLS4の高速版
    class ReactiveRandomLocalSearch5 : IOptimizationAlgorithm
    {
        private int initialSizeOfSubset;
        private long limit;
        private int targetRange;

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
            ISolution ret = sol.Clone();
            ISolution s = sol.Clone();
            long endCount = limit;
            int subLoop = -800;
            int sizeOfSubset = this.initialSizeOfSubset;
            int bestLoop = 0;
            double startRatio = 0.65;
            double targetAct = startRatio;

            List<ISolution> ss = new List<ISolution>();
            List<ISolution> doms = new List<ISolution>();

            for (int loop = 0; true; loop++)
            {
                double plus = p.OperationSet().Where((op) => p.OperationValue(op, s) >= 0).Count();
                double minus = p.OperationSet().Where((op) => p.OperationValue(op, s) < 0).Count();

                ss.Add(s.Clone());

                ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10, 0));
                double r10 = (double)s.DistanceTo(s10) / Math.Min(2 * Math.Min(Math.Max(ss.Count(), 1), 10), p.Size);
                
                //Console.WriteLine(loop + ":" + ret.Value + ":" + s.Value + ":" + s.DistanceTo(p.Optimum) + ":" + sizeOfSubset + ":" + targetAct.ToString("F2") + ":" + r10.ToString("F2") + ":" + endCount);
                if (20 < bestLoop)
                {
                    targetAct = 0.65;
                }
                else
                {
                    targetAct = 0.2;
                }

                if (0 <= subLoop)
                {


                    if (r10 < targetAct)
                    {
                        sizeOfSubset = Math.Max(sizeOfSubset - (int)(p.OperationSet().Count() * 0.01), 1);
                        subLoop = Math.Min(-10, sizeOfSubset);
                    }
                    else
                    {
                        sizeOfSubset = Math.Min(sizeOfSubset + (int)(p.OperationSet().Count() * 0.01), p.OperationSet().Count());
                        subLoop = Math.Min(-10, sizeOfSubset);
                    }
                }

                ++subLoop;
                ++bestLoop;
            
                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
                ISolution tmp = s.Apply(bestOp);

                if (s.Value < ss.Skip(ss.Count() - targetRange).Min((x) => x.Value))
                {
                    bestLoop = 0;
                }

                if (s.Value < ret.Value)
                {
                    ret = s.Clone();
                }

                endCount -= sizeOfSubset;
                if (endCount <= 0)
                {
                    //Analizer.createDomainMap(ss, doms, p);
                    return ret;
                }


                s = tmp;
            }
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
        private string Trace(int loop, int sizeOfSubset, double ratio, ISolution opt, ISolution dom, double r10, double r100, double dm10, double dm100, double targetAct, ISolution x)
        {
            return loop + ":" + sizeOfSubset + ":" + ratio.ToString("F3") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + dom.DistanceTo(opt) + ":" + r10.ToString("F3") + ":" + r100.ToString("F3") + ":" + targetAct.ToString("F3") + ":" + dm10 + ":" + dm100 + ":" + x;
        }

        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, Diameter d1, Diameter d2, int d10, int d100, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + d1.DiameterValue + ":" + d2.DiameterValue + ":" + d10 + ":" + d100 + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",subset=" + initialSizeOfSubset + ",trange" + targetRange + "]";
        }

        public ReactiveRandomLocalSearch5(long limit, int sizeOfSubset, int targetRange)
        {
            this.limit = limit;
            this.initialSizeOfSubset = sizeOfSubset;
            this.targetRange = targetRange;
        }

    }
}