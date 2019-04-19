using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // RRLS4のTargetActivityをパラメータ化したもの
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class ReactiveRandomLocalSearch691 : IOptimizationAlgorithm
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
            double startAct = targetHighAct;
            double targetAct = startAct;
            ISolution highBest = sol.Clone();
            int highBestloop = 0;
            int highLoop = 0;

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");

            List<ISolution> ss = new List<ISolution>();
            List<int> hist = new List<int>();
            List<ISolution> doms = new List<ISolution>();

            for (int loop = 0; true; loop++)
            {
                ss.Add(s.Clone());
                if (100 < ss.Count())
                {
                    ss.RemoveAt(0);
                }
                hist.Add(s.Value);
                if (targetRange < hist.Count())
                {
                    hist.RemoveAt(0);
                }

                int dd = 20;
                int d = 0;
                for (int i = 1; i < Math.Min(ss.Count - 1, dd); i++)
                {
                    d += ss.ElementAt(ss.Count() - i).DistanceTo(ss.ElementAt(ss.Count() - i - 1));
                }
                double act = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - dd, 0))) / d;

                String add = "";
                if (1000 < highLoop && targetAct == targetHighAct && s.Value < highBest.Value)
                {
                    add = "*";
                }

                Console.WriteLine(loop + ":" + ret.Value + ":" + s.Value + ":" + highBest.Value + ":" + s.DistanceTo(p.Optimum).ToString("D4") + ":" + ":" + sizeOfSubset.ToString("D4") + ":" + act.ToString("F2") + ":" + targetAct.ToString("F2") + ":" + endCount + add);

                if (targetAct == targetHighAct)
                {
                    if (1000 < highLoop)
                    {
                        if (s.Value < highBest.Value)
                        {
                            highBest = s.Clone();
                            highBestloop = 0;
                        }
                    }

                    if (10000 < highLoop) highBest = s.Clone();
                }

                if (loop < 1000)
                {
                    if (targetAct == targetLowAct) highLoop = 0;

                    targetAct = targetHighAct;
                }
                else if (1000 < highBestloop)
                {
                    if(targetAct == targetLowAct) highLoop = 0;

                    targetAct = targetHighAct;
                }
                else if(1000 < bestLoop)
                {
                    if (targetAct == targetLowAct) highLoop = 0;

                    targetAct = targetHighAct;
                }
                else if (s.Value == highBest.Value)
                {
                    targetAct = targetLowAct;
                }

                if (0 <= subLoop)
                {
                    if (act < targetAct)
                    {
                        sizeOfSubset = Math.Max(sizeOfSubset - (int)(sizeOfNeighborhood * 0.01), 1);
                        subLoop = Math.Min(-20, sizeOfSubset);
                    }
                    else
                    {
                        sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.01), sizeOfNeighborhood);
                        subLoop = Math.Min(-20, sizeOfSubset);
                    }
                }
                if (!s.IsFeasible())
                {
                    sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
                }
                ++subLoop;
                ++bestLoop;
                ++highBestloop;
                ++highLoop;
                
                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
                int delta = p.OperationValue(bestOp, s);
                ISolution tmp = s.Apply(bestOp);

                if (s.Value < hist.Skip(hist.Count() - targetRange).Min((x) => x))
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
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",lact=" + targetLowAct + ",trange=" + targetRange + "]";
        }

        public ReactiveRandomLocalSearch691(long limit, double targetHighAct, double targetLowAct, int targetRange)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
            this.targetRange = targetRange;
        }

    }
}