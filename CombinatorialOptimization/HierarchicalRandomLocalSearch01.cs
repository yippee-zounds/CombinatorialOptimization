using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class HierarchicalRandomLocalSearch01 : IOptimizationAlgorithm
    {
        private long limit;
        private double targetHighAct;
        private double targetMidAct;
        private double targetLowAct;
        private int initialSizeOfSubset;
        private int sizeOfNeighborhood;


        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        private void Search(List<HierarchicalRandomLocalSearchTrajectory> xs)
        {
            for (int i = 0; i < xs.Count; i++)
            {
                xs[i].Search();
            }
        }

        private long CalcCount(List<HierarchicalRandomLocalSearchTrajectory> x)
        {
            return x.Sum((y) => y.sizeOfSubset);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution ret = sol.Clone();
            List<HierarchicalRandomLocalSearchTrajectory> xh = new List<HierarchicalRandomLocalSearchTrajectory>();
            List<HierarchicalRandomLocalSearchTrajectory> xm = new List<HierarchicalRandomLocalSearchTrajectory>();
            List<HierarchicalRandomLocalSearchTrajectory> xl = new List<HierarchicalRandomLocalSearchTrajectory>();
            ISolution bestDom = sol.Clone();
            ISolution s_old = sol.Clone();
            long endCount = limit;
            int subLoop = 0;
            sizeOfNeighborhood = p.OperationSet().Count();
            int hBestValue = int.MaxValue;
            int mBestValue = int.MaxValue;
            int worst = 0;

            for (int i = 0; i < 4; i++)
            {
                xh.Add(new HierarchicalRandomLocalSearchTrajectory(p, p.CreateRandomSolution(), targetHighAct, initialSizeOfSubset));
            }

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:err:dom:x");

            for (int loop = 0; true; loop++)
            {
                //探索モード、目標活性度の設定
                Search(xh);
                Search(xm);
                Search(xl);

                //コンソールへの出力
                Console.WriteLine(loop + ":" + ret.Value + ":" + xh.Min((x) => x.s.Value) + ":" + (0 < xm.Count ? xm.DefaultIfEmpty().Min((x) => x.s.Value) : 0) + ":" + (0 < xl.Count ? xl.DefaultIfEmpty().Min((x) => x.s.Value) : 0) + ":" + xh.Count() + ":" + xm.Count() + ":" + xl.Count() + ":" + endCount);

                //大域的な探索

                if (loop == 4000)
                {
                    for (int i = 0; i < xm.Count(); i++)
                    {
                        if (xm[i].best.Skip(xm[i].best.Count() - 1).First().Value < mBestValue)
                        {
                            mBestValue = xm[i].best.Skip(xm[i].best.Count() - 1).First().Value;
                            xl.Add(new HierarchicalRandomLocalSearchTrajectory(p, xm[i].best.Skip(xm[i].best.Count() - 1).First().Clone(), targetLowAct, xm[i].sizeOfSubset));
                        }
                    }
                }
                else if (4000 < loop)
                {
                    for (int i = 0; i < xm.Count(); i++)
                    {
                        if (xm[i].s.Value < mBestValue)
                        {
                            xl.Add(new HierarchicalRandomLocalSearchTrajectory(p, xm[i].s.Clone(), targetLowAct, xm[i].sizeOfSubset));

                            if (3 < xl.Count())
                            {
                                worst = xl.Max((x) => x.s.Value);
                                for (int j = 0; j < xl.Count; j++)
                                    if (xl[j].s.Value == worst) xl.RemoveAt(j);
                            }
                        }
                    }
                }
                else if (loop == 2000)
                {
                    for (int i = 0; i < xh.Count(); i++)
                    {
                        if (xh[i].best.Skip(xh[i].best.Count() - 1).First().Value < hBestValue)
                        {
                            hBestValue = xh[i].best.Skip(xh[i].best.Count() - 1).First().Value;
                            xm.Add(new HierarchicalRandomLocalSearchTrajectory(p, xh[i].best.Skip(xh[i].best.Count() - 1).First().Clone(), targetMidAct, xh[i].sizeOfSubset));
                        }
                    }
                }
                else if (2000 < loop)
                {
                    for (int i = 0; i < xh.Count(); i++)
                    {
                        if (xh[i].s.Value < hBestValue)
                        {
                            xm.Add(new HierarchicalRandomLocalSearchTrajectory(p, xh[i].s.Clone(), targetMidAct, xh[i].sizeOfSubset));

                            if (10 < xm.Count())
                            {
                                worst = xm.Max((x) => x.s.Value);
                                for (int j = 0; j < xm.Count; j++)
                                    if (xm[j].s.Value == worst) xm.RemoveAt(j);
                            }
                        }
                    }
                }

                if (0 < xh.Count() && xh.Min((x) => x.best.Min((y) => y.Value)) < ret.Value)
                {
                    ret = xh.Select((x) => x.best.Last()).ArgMinStrict((b) => b.Value).Clone();
                }
                if (0 < xm.Count() && xm.Min((x) => x.best.Min((y) => y.Value)) < ret.Value)
                {
                    ret = xm.Select((x) => x.best.Last()).ArgMinStrict((b) => b.Value).Clone();
                }
                if (0 < xl.Count() && xl.Min((x) => x.best.Min((y) => y.Value)) < ret.Value)
                {
                    ret = xl.Select((x) => x.best.Last()).ArgMinStrict((b) => b.Value).Clone();
                }

                //後処理
                endCount -= CalcCount(xh) + CalcCount(xm) + CalcCount(xl);
                if (endCount <= 0)
                {
                    w.WriteLine(ret.ToString());
                    return ret;
                }

                subLoop++;
            }
        }



        public ISolution Solve(IOptimizationProblem p, IOperationSet ops, ISolution sol, DataStoringWriter w)
        {
            ISolution s = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();

            w.WriteLine("loop:vx:doptx:x");

            for (int loop = 0; true; loop++)
            {

                IOperation bestOp = ops.ArgMinStrict((op) => p.OperationValue(op, s));
                ISolution tmp = s.Apply(bestOp);

                if (mk.IsNotMinimumStrict(tmp.Value))
                    return s;

                s = tmp;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + "]";
        }

        public HierarchicalRandomLocalSearch01(long limit, double targetHighAct, double targetMidAct, double targetLowAct)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetMidAct = targetMidAct;
            this.targetLowAct = targetLowAct;
            this.initialSizeOfSubset = 1;
        }

    }
}