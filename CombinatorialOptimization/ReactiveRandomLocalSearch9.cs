using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // RRLS6のTargetRangeを適応的に変化させたもの
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class ReactiveRandomLocalSearch9 : IOptimizationAlgorithm
    {
        private int initialSizeOfSubset;
        private long limit;
        private int targetRange;
        private double targetHighAct;
        private double targetLowAct;

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
            int subLoop = 0;
            int sizeOfSubset = this.initialSizeOfSubset;
            int sizeOfNeighborhood = p.OperationSet().Count();
            int bestLoop = 0;
            double startRatio = targetHighAct;
            double targetAct = startRatio;
            int lastBest = 0;
            int bestDomValue = int.MaxValue;

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");

            List<ISolution> ss = new List<ISolution>();
            List<ISolution> doms = new List<ISolution>();

            for (int loop = 0; true; loop++)
            {
                ISolution dom = s;// new LocalSearch().Solve(p, s);
                bestDomValue = Math.Min(bestDomValue, dom.Value);

                doms.Add(dom);
                ss.Add(s.Clone());

                ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10, 0));
                ISolution s100 = ss.ElementAt(Math.Max(ss.Count() - 100, 0));

                int d10 = 0;
                for (int i = 0; i < Math.Min(ss.Count - 1, 10); i++)
                {
                    d10 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));
                }
                double r10 = (double)s.DistanceTo(s10) / d10;

                int d100 = 0;
                for (int i = 0; i < Math.Min(ss.Count - 1, 100); i++)
                {
                    d100 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));
                }
                double r100 = (double)s10.DistanceTo(s100) / d100;
                Diameter dm10 = Diameter.CalculateDiameter(ss, 10);
                Diameter dm100 = Diameter.CalculateDiameter(ss, 100);

                Console.WriteLine(loop + ":" + ret.Value + ":" + s.Value + ":" + dom.Value + ":" + bestDomValue + ":" + s.DistanceTo(p.Optimum).ToString("D4") + ":" + s.DistanceTo(ret).ToString("D4") + ":" + sizeOfSubset.ToString("D4") + ":" + dm100.DiameterValue + ":" + targetAct.ToString("F2") + ":" + r10.ToString("F2") + ":" + targetRange + ":" + endCount);

                if (3 * targetRange + 1000 < loop - lastBest)
                {
                    targetRange += 1000;
                    lastBest = loop;
                }

                if (100 < bestLoop)
                {
                    targetAct = targetHighAct;
                }
                else
                {
                    targetAct = targetLowAct;
                }

                if (0 <= subLoop)
                {

                    if (r10 < targetAct)
                    {
                        sizeOfSubset = Math.Max(sizeOfSubset - (int)(sizeOfNeighborhood * 0.01), 1);
                        subLoop = Math.Min(-10, sizeOfSubset);
                    }
                    else
                    {
                        sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.01), sizeOfNeighborhood);
                        subLoop = Math.Min(-10, sizeOfSubset);
                    }
                }
                if (!s.IsFeasible())
                {
                    sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
                }
                ++subLoop;
                ++bestLoop;
                w.WriteLine(Trace(loop, sizeOfSubset, 0, p.Optimum, dom, r10, r100, dm10.DiameterValue, dm100.DiameterValue, targetAct, s));

                IEnumerable<IOperation> ops = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset);
                IEnumerable<int> opsf = ops.Select((op) => p.OperationValue(op, s));
                IOperation tmpOp = ops.ArgMinStrict((op) => p.OperationValue(op, s));

                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
                int delta = p.OperationValue(bestOp, s);
                ISolution tmp = s.Apply(bestOp);

                if (s.Value < ss.Skip(ss.Count() - targetRange).Min((x) => x.Value))
                {
                    bestLoop = 0;
                }

                if (s.Value < ret.Value)
                {
                    lastBest = loop;
                    targetRange = 1000;
                    ret = s.Clone();
                }

                endCount -= sizeOfSubset;
                if (endCount <= 0)
                {
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
            return loop + ":" + sizeOfSubset + ":" + ratio.ToString("F3") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + dom.DistanceTo(opt) + ":" + r10.ToString("F3") + ":" + r100.ToString("F3") + ":" + targetAct.ToString("F3") + ":" + dm10 + ":" + dm100 + ":" + dom + ":" + x;
        }

        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, Diameter d1, Diameter d2, int d10, int d100, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + d1.DiameterValue + ":" + d2.DiameterValue + ":" + d10 + ":" + d100 + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",lact=" + targetLowAct + "]";
        }

        public ReactiveRandomLocalSearch9(long limit, double targetHighAct, double targetLowAct)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
            this.targetRange = 1000;
        }

    }
}