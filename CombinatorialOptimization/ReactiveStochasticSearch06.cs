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
    class ReactiveStochasticSearch06 : IOptimizationAlgorithm
    {
        private int initialSizeOfSubset;
        private long limit;
        private double targetHighAct;
        private double targetLowAct;
        private int trajectories;

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
            ISolution[] s = new ISolution[trajectories];
            long endCount = limit;
            int[] sizeOfSubset = new int[trajectories];
            int sizeOfNeighborhood = p.OperationSet().Count();
            double[] startRatio = new double[trajectories];
            double[] targetAct = new double[trajectories];
            ISolution[] highBest = new ISolution[trajectories];
            int[] lowLoop = new int[trajectories];
            int[] highLoop = new int[trajectories];

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:tact:dom:x");

            List<ISolution>[] ss = new List<ISolution>[trajectories];
            List<int>[] sss = new List<int>[trajectories];
            List<ISolution>[] doms = new List<ISolution>[trajectories];

            for (int t = 0; t < trajectories; t++)
            {
                s[t] = sol.Clone();
                sizeOfSubset[t] = this.initialSizeOfSubset;
                highBest[t] = sol.Clone();
                startRatio[t] = targetHighAct;
                targetAct[t] = startRatio[t];
                ss[t] = new List<ISolution>();
                sss[t] = new List<int>();
                doms[t] = new List<ISolution>();
            }

            for (int loop = 0; true; loop++)
            {
                IEnumerable<int> q = s.SelectMany((x) => s.Where((y) => x != y).Select((y) => x.DistanceTo(y)));
                int distMin = q.Min();
                int distMax = q.Max();
                Console.WriteLine(loop + ":" + ret.Value + ":" + distMax + ":" + distMin);

                for (int t = 0; t < trajectories; t++)
                {
                    ISolution dom = s[t];// new LocalSearch().Solve(p, s[t]);
                    ss[t].Add(s[t].Clone());
                    if (10 < ss[t].Count()) ss[t].RemoveAt(0);

                    double currentAct = calculateActivity(ss[t]);

                    //Console.WriteLine(loop + ":" + ret.Value + ":" + s[t].Value + ":" + dom.Value + ":" + highBest[t].Value + ":" + s[t].DistanceTo(p.Optimum).ToString("D4") + ":" + ":" + sizeOfSubset[t].ToString("D4") + ":" + 0 + ":" + targetAct[t].ToString("F2") + ":" + currentAct.ToString("F2") + ":" + endCount);

                    if (targetAct[t] == targetHighAct)
                    {
                        ++highLoop[t];

                        if (1000 < highLoop[t])
                        {
                            if (s[t].Value < highBest[t].Value)
                            {
                                highBest[t] = s[t].Clone();
                                lowLoop[t] = 0;
                                targetAct[t] = targetLowAct;
                            }
                        }
                    }
                    else
                    {
                        ++lowLoop[t];

                        if (1000 < lowLoop[t])
                        {
                            highLoop[t] = 0;
                            targetAct[t] = targetHighAct;
                        }
                    }

                    if (currentAct < targetAct[t]) sizeOfSubset[t] = Math.Max(sizeOfSubset[t] - (int)(sizeOfNeighborhood * 0.001), 1);
                    else sizeOfSubset[t] = Math.Min(sizeOfSubset[t] + (int)(sizeOfNeighborhood * 0.001), sizeOfNeighborhood);

                    if (!s[t].IsFeasible()) sizeOfSubset[t] = Math.Min(sizeOfSubset[t] + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);

                    w.WriteLine(Trace(loop, sizeOfSubset[t], 0, p.Optimum, dom, currentAct, targetAct[t], s[t]));

                    s[t] = s[t].Apply(p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset[t]).ArgMinStrict((op) => p.OperationValue(op, s[t])));
                }

                if (s.ArgMin((x) => x.Value).Value < ret.Value) ret = s.ArgMin((x) => x.Value).Clone();

                endCount -= sizeOfSubset.Sum((x) => x);
                if (endCount <= 0) return ret;

            }
        }

        private double calculateActivity(List<ISolution> ss)
        {
            int d10 = 0;
            for (int i = 0; i < ss.Count - 1; i++) d10 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));

            return (double)ss.First().DistanceTo(ss.Last()) / d10;
        }

        private string Trace(int loop, int sizeOfSubset, double ratio, ISolution opt, ISolution dom, double r10, double targetAct, ISolution x)
        {
            return loop + ":" + sizeOfSubset + ":" + ratio.ToString("F3") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + dom.DistanceTo(opt) + ":" + r10.ToString("F3") + ":" + targetAct.ToString("F3") + ":" + dom + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",lact=" + targetLowAct + /*",trange=" + targetRange +*/ "]";
        }

        public ReactiveStochasticSearch06(long limit, double targetHighAct, double targetLowAct, int trajectories)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
            this.trajectories = trajectories;
        }

    }
}