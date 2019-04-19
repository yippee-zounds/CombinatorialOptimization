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
    class ReactiveRandomLocalSearch6A : IOptimizationAlgorithm
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
            ISolution highBest = sol.Clone();
            int bestLoop2 = 0;
            int highLoop = 0;

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");

            List<ISolution> ss = new List<ISolution>();
            List<int> sss = new List<int>();
            List<ISolution> doms = new List<ISolution>();

            for (int loop = 0; true; loop++)
            {
                ISolution dom = new LocalSearch().Solve(p, s);
                doms.Add(dom);

                ss.Add(s.Clone());
                if (Math.Max(20, targetRange) < ss.Count())
                {
                    ss.RemoveAt(0);
                }

                int d10 = 0;
                for (int i = 0; i < Math.Min(ss.Count - 1, 10); i++)
                {
                    d10 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));
                }
                double r10 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 10, 0))) / d10;

                int dd = 20;
                int d20 = 0;
                for (int i = 1; i < Math.Min(ss.Count - 1, dd); i++)
                {
                    d20 += ss.ElementAt(ss.Count() - i).DistanceTo(ss.ElementAt(ss.Count() - i - 1));
                }
                double r20 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - dd, 0))) / d20;
                
                String add = "";
                if (1000 < highLoop && targetAct == targetHighAct && s.Value < highBest.Value)
                {
                    highBest = s.Clone();
                    bestLoop2 = 0;
                    add = "*";
                }

                if (10000 < highLoop && targetAct == targetHighAct) highBest = s.Clone();

                Console.WriteLine(loop + ":" + ret.Value + ":" + s.Value + ":" + dom.Value + ":" + highBest.Value + ":" + s.DistanceTo(p.Optimum).ToString("D4") + ":" + ":" + sizeOfSubset.ToString("D4") + ":" + r20.ToString("F2") + ":" + targetAct.ToString("F2") + ":" + r10.ToString("F2") + ":" + endCount + add);

                if (1000 < bestLoop2 || 1000 < bestLoop || loop < 1000)
                {
                    if(targetAct == targetLowAct) highLoop = 0;
                    targetAct = targetHighAct;
                }
                else if(s.Value == highBest.Value)
                {
                    targetAct = targetLowAct;
                }

                if (0 <= subLoop)
                {

                    if (r20 < targetAct)
                    {
                        sizeOfSubset = Math.Max(sizeOfSubset - (int)(sizeOfNeighborhood * 0.020), 1);
                        subLoop = Math.Min(-20, sizeOfSubset);
                    }
                    else
                    {
                        sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.020), sizeOfNeighborhood);
                        subLoop = Math.Min(-20, sizeOfSubset);
                    }
                }
                if (!s.IsFeasible())
                {
                    sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
                }
                ++subLoop;
                ++bestLoop;
                ++bestLoop2;
                ++highLoop;
                w.WriteLine(Trace(loop, sizeOfSubset, 0, p.Optimum, dom, r10, r20, 0, 0, targetAct, s));

                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
                int delta = p.OperationValue(bestOp, s);
                ISolution tmp = s.Apply(bestOp);

                if (s.Value < ss.Skip(Math.Max(0, ss.Count() - targetRange)).Min((x) => x.Value))
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

        private string Trace(int loop, int sizeOfSubset, double ratio, ISolution opt, ISolution dom, double r10, double r100, double dm10, double dm100, double targetAct, ISolution x)
        {
            return loop + ":" + sizeOfSubset + ":" + ratio.ToString("F3") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + dom.DistanceTo(opt) + ":" + r10.ToString("F3") + ":" + r100.ToString("F3") + ":" + targetAct.ToString("F3") + ":" + dm10 + ":" + dm100 + ":" + dom + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",lact=" + targetLowAct + ",trange=" + targetRange + "]";
        }

        public ReactiveRandomLocalSearch6A(long limit, double targetHighAct, double targetLowAct, int targetRange)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
            this.targetRange = targetRange;
        }

    }
}