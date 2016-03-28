using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    class ReactiveRandomLocalSearch2 : IOptimizationAlgorithm
    {
        private int loop;
        private int sizeOfSubset;
        private int limit;

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
            MinimumKeeper mk = new MinimumKeeper();
            int endCount;
            List<ISolution> ss = new List<ISolution>();
            List<ISolution> doms = new List<ISolution>();
            int subLoop = -200;
            int sizeOfN = p.OperationSet().Count();
            int upper = (int)(sizeOfN * 0.8);
            int lower = 1;//(int)(sizeOfN * 0.02);
            int bestLoop = 0;
            int threLoop1 = (int)(this.loop * 0.2);
            int threLoop2 = (int)(this.loop * 0.4);
            int threLoop3 = (int)(this.loop * 0.5);
            int threLoop4 = (int)(this.loop * 0.8);
            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:x");
            double startRatio = 0.65;
            double middleRatio1 = 0.15;
            double middleRatio2 = 0.45;
            double middleRatio3 = 0.25;
            double endRatio = 0.10;
            double targetAct = startRatio;

            for (int loop = 0; true; loop++)
            {
                double plus = p.OperationSet().Where((op) => p.OperationValue(op, s) >= 0).Count();
                double minus = p.OperationSet().Where((op) => p.OperationValue(op, s) < 0).Count();
                endCount = limit;

                ISolution dom = new LocalSearch().Solve(p, s);

                doms.Add(dom);
                ss.Add(s.Clone());

                ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10, 0));
                ISolution s100 = ss.ElementAt(Math.Max(ss.Count() - 100, 0));
                double r10 = (double)s.DistanceTo(s10) / Math.Min(2 * Math.Min(Math.Max(ss.Count(), 1), 10), p.Size);
                double r100 = (double)s10.DistanceTo(s100) / Math.Min(2 * Math.Min(Math.Max(ss.Count(), 1), 90), p.Size);
                Diameter dm10 = Diameter.CalculateDiameter(ss, 10);
                Diameter dm100 = Diameter.CalculateDiameter(ss, 100);

                if(bestLoop == 0)
                {
                    Console.WriteLine(loop + ":" + ret.Value + ":" + dom.Value + ":" + ret.DistanceTo(p.Optimum) + ":" + dom.DistanceTo(p.Optimum));
                }

                if (0 <= subLoop)
                {
                    if(loop < threLoop1)
                    {
                        targetAct = startRatio;
                    }
                    else if(loop < threLoop2)
                    {
                        targetAct = (middleRatio1 - startRatio) * (loop - threLoop1) / (this.loop - threLoop1) + startRatio;
                    }
                    else if(loop < threLoop3)
                    {
                        targetAct = (middleRatio2 - middleRatio1) * (loop - threLoop2) / (this.loop - threLoop2) + middleRatio1;
                    }
                    else if(loop < threLoop4)
                    {
                        targetAct = (middleRatio3 - middleRatio2) * (loop - threLoop3) / (this.loop - threLoop3) + middleRatio2;
                    }
                    else
                    {
                        targetAct = (endRatio - middleRatio3) * (loop - threLoop4) / (this.loop - threLoop4) + middleRatio3;
                    }

                    if (r10 < targetAct)
                    {
                        sizeOfSubset = Math.Max(sizeOfSubset - (int)(sizeOfN * 0.01), lower);
                        subLoop = -10;
                    }
                    else
                    {
                        sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfN * 0.01), upper);
                        subLoop = -10;
                    }
                }
                ++subLoop;
                ++bestLoop;

                w.WriteLine(Trace(loop, sizeOfSubset, minus / (plus + minus), p.Optimum, dom, r10, r100, dm10.DiameterValue, dm100.DiameterValue, targetAct, s));

                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
                ISolution tmp = s.Apply(bestOp);

                if (s.Value < ret.Value)
                {
                    bestLoop = 0;
                    ret = s.Clone();
                }

                if (this.loop == 0 && endCount <= 0)
                {
                    Analizer.createDomainMap(ss, doms, p);
                    return ret;
                }

                if (limit == 0 && this.loop <= loop)
                {
                    Analizer.createDomainMap(ss, doms, p);
                    return ret;
                }
                if (this.loop == 0 && mk.IsNotMinimumStrict(tmp.Value))
                {
                    --endCount;
                    --loop;
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
            return this.GetType().Name + "[loop=" + loop + ",subset=" + sizeOfSubset + ",lim=" + limit + "]";
        }

        public ReactiveRandomLocalSearch2(int loop, int sizeOfSubset, int limit)
        {
            this.loop = loop;
            this.sizeOfSubset = sizeOfSubset;
            this.limit = limit;
        }

        public ReactiveRandomLocalSearch2(int sizeOfSubset, int limit)
        {
            this.loop = 0;
            this.sizeOfSubset = sizeOfSubset;
            this.limit = limit;
        }
    }
}