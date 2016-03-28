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
    class ReactiveRandomLocalSearch63 : IOptimizationAlgorithm
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
            //int actLoop = targetRange;
            int sizeOfSubset = this.initialSizeOfSubset;

            int bestLoop = 0;
            double startRatio = targetHighAct;
            double targetAct = startRatio;

            Dictionary<int, int> d = new Dictionary<int, int>();

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");

            List<ISolution> ss = new List<ISolution>();
            List<int> sss = new List<int>();
            List<ISolution> doms = new List<ISolution>();

            for (int loop = 0; true; loop++)
            {
                ISolution dom = s;// new LocalSearch().Solve(p, s);
                //doms.Add(dom);
                ss.Add(s.Clone());
                if (100 < ss.Count()) ss.RemoveAt(0);
                
                sss.Add(s.Value);
                if (targetRange < sss.Count()) sss.RemoveAt(0);

                double currentAct = calcActivity(ss);

                Console.WriteLine(loop + ":" + ret.Value + ":" + s.Value + ":" + dom.Value + ":" + s.DistanceTo(p.Optimum).ToString("D4") + ":" + s.DistanceTo(ret).ToString("D4") + ":" + sizeOfSubset.ToString("D4") + ":" + Diameter.CalculateDiameter(ss).DiameterValue + ":" + targetAct.ToString("F2") + ":" + currentAct.ToString("F2") + ":" + endCount + ":" + bestLoop);
                //Console.WriteLine(loop + ":" + ret.Value + ":" + s.DistanceTo(p.Optimum) + ":" + s.ToString());
                if ((loop + 1) % 1000 == 0) Console.WriteLine(string.Join(", ", d.Select((x) => "{" + x.Key + ", " + x.Value + "}")));

                int nrange = 3;
                if (nrange < ss.Count() && 2 <= ss.Skip(ss.Count() - nrange).Count((x) => x.DistanceTo(ss.Last()) == 0))
                {
                    targetAct = Math.Min(targetAct + 0.1, targetHighAct);
                }
                else if(1000 < loop && bestLoop == 0)
                {
                    targetAct = Math.Max(targetAct - 0.1, targetLowAct);
                }

                ISolution tmp = moveByActivity(p, s, currentAct, targetAct, ref sizeOfSubset);

                //w.WriteLine(Trace(loop, sizeOfSubset, 0, p.Optimum, dom, currentAct, 0, 0, 0, targetAct, s));

                ++bestLoop;

                if (s.Value < sss.Skip(sss.Count() - targetRange).Min((x) => x))
                {
                    bestLoop = 0;
                }

                if (s.Value < ret.Value)
                {
                    int dist = s.DistanceTo(ret);
                    int disto = s.DistanceTo(p.Optimum);
                    if (2 < dist)
                    {
                        if (!d.ContainsKey(dist)) d.Add(dist, disto);
                        d[dist] = disto;
                    }

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

        private double calcActivity(List<ISolution> ss)
        {
            ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10, 0));

            int d10 = 0;
            for (int i = 0; i < Math.Min(ss.Count - 1, 10); i++)
            {
                d10 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));
            }
            return (double)ss.Last().DistanceTo(s10) / d10;
        }

        private ISolution moveByActivity(IOptimizationProblem p, ISolution s, double currentAct, double targetAct, ref int sizeOfSubset)
        {
            int sizeOfNeighborhood = p.OperationSet().Count();

            if (Double.IsNaN(currentAct) || currentAct < targetAct)
            {
                sizeOfSubset = Math.Max(sizeOfSubset - Math.Max(1, (int)(sizeOfNeighborhood * 0.001)), 1);

            }
            else
            {
                sizeOfSubset = Math.Min(sizeOfSubset + Math.Max(1, (int)(sizeOfNeighborhood * 0.001)), sizeOfNeighborhood);
            }
            if (!s.IsFeasible())
            {
                sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
            }

            return moveBySizeOfNeighborhood(p, s, sizeOfNeighborhood, sizeOfSubset);
        }

        private ISolution moveBySizeOfNeighborhood(IOptimizationProblem p, ISolution s, int sizeOfNeighborhood, int sizeOfSubset)
        {
            IEnumerable<IOperation> ops = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset);
            IEnumerable<int> opsf = ops.Select((op) => p.OperationValue(op, s));
            IOperation tmpOp = ops.ArgMinStrict((op) => p.OperationValue(op, s));

            IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
            int delta = p.OperationValue(bestOp, s);
            return s.Apply(bestOp);
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

        public ReactiveRandomLocalSearch63(long limit, double targetHighAct, double targetLowAct, int targetRange)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
            this.targetRange = targetRange;
        }

    }
}