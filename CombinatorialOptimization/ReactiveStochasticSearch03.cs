using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization {
    // RRLS4のTargetActivityをパラメータ化したもの
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class ReactiveStochasticSearch03 : IOptimizationAlgorithm {
        private int initialSizeOfSubset;
        private long limit;
        private double targetHighAct;
        private double targetLowAct;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w) {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s) {
            return Solve(p, s, new NullWriter());
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w) {
            ISolution best = sol.Clone();
            ISolution s = sol.Clone();
            long endCount = limit;
            int sizeOfSubset = this.initialSizeOfSubset;
            int sizeOfNeighborhood = p.OperationSet().Count();
            double startRatio = targetHighAct;
            double targetAct = startRatio;
            ISolution highBest = sol.Clone();
            int lowLoop = 0;
            int highLoop = 0;
            int subLoop = 0;

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");

            List<ISolution> ss = new List<ISolution>();
            List<int> sss = new List<int>();
            List<ISolution> doms = new List<ISolution>();

            for (int loop = 0; true; loop++) {
                ISolution dom = new LocalSearch().Solve(p, s);
                doms.Add(dom);
                if (100 < doms.Count()) doms.RemoveAt(0);

                ss.Add(s.Clone());
                if (10 < ss.Count()) ss.RemoveAt(0);

                double currentAct = calculateActivity(ss);

                Console.WriteLine(loop + ":" + s.Value + ":" + best.Value + ":" + dom.Value + ":" + highBest.Value + ":" + Diameter.CalculateDiameter(doms).DiameterValue.ToString("D3") + ":" + s.DistanceTo(p.Optimum).ToString("D4") + ":" + ":" + sizeOfSubset.ToString("D4") + ":" + 0 + ":" + targetAct.ToString("F2") + ":" + currentAct.ToString("F2") + ":" + endCount);

                if (10000 < highLoop) {
                    targetAct = targetHighAct + 0.05;
                    highLoop = 0;
                    lowLoop = 0;
                    highBest = p.CreateRandomSolution();
                }
                else if (targetAct == targetHighAct) {
                    ++highLoop;

                    if (1000 < highLoop && s.Value < highBest.Value) {
                        highBest = s.Clone();
                        lowLoop = 0;
                        targetAct = targetLowAct;
                    }
                }
                else {
                    ++lowLoop;

                    if (1000 < lowLoop) {
                        highLoop = 0;
                        targetAct = targetHighAct;
                    }
                }

                if (0 <= subLoop) {
                    if (currentAct < targetAct) {
                        sizeOfSubset = Math.Max(sizeOfSubset - (int)(sizeOfNeighborhood * 0.01), 1);
                        subLoop = Math.Min(-10, sizeOfSubset);
                    }
                    else {
                        sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.01), sizeOfNeighborhood);
                        subLoop = Math.Min(-10, sizeOfSubset);
                    }
                }
                ++subLoop;

                if (!s.IsFeasible()) sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);

                w.WriteLine(Trace(loop, sizeOfSubset, 0, p.Optimum, dom, currentAct, 0, 0, 0, targetAct, s));

                ISolution tmp = s.Apply(p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s)));

                if (s.Value < best.Value) best = s.Clone();

                endCount -= sizeOfSubset;
                if (endCount <= 0) return best;

                s = tmp;
            }
        }

        private double calculateActivity(List<ISolution> ss) {
            int d10 = 0;
            for (int i = 0; i < ss.Count - 1; i++) d10 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));

            return (double)ss.First().DistanceTo(ss.Last()) / d10;
        }

        private string Trace(int loop, int sizeOfSubset, double ratio, ISolution opt, ISolution dom, double r10, double r100, double dm10, double dm100, double targetAct, ISolution x) {
            return loop + ":" + sizeOfSubset + ":" + ratio.ToString("F3") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + dom.DistanceTo(opt) + ":" + r10.ToString("F3") + ":" + r100.ToString("F3") + ":" + targetAct.ToString("F3") + ":" + dm10 + ":" + dm100 + ":" + dom + ":" + x;
        }

        public override string ToString() {
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",lact=" + targetLowAct + /*",trange=" + targetRange +*/ "]";
        }

        public ReactiveStochasticSearch03(long limit, double targetHighAct, double targetLowAct) {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
        }

    }
}