using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class HierarchicalRandomLocalSearch05 : IOptimizationAlgorithm
    {
        private long limit;
        private double targetHighAct;
        private double targetMidAct;
        private double targetLowAct;
        private int initialSizeOfSubset;
        private int sizeOfNeighborhood;
        private int range;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        private void Search(List<HierarchicalRandomLocalSearchNode> xs)
        {
            for (int i = 0; i < xs.Count; i++)
            {
                xs[i].Search();
            }
        }

        private long CalcCount(List<HierarchicalRandomLocalSearchNode> x)
        {
            return x.Sum((y) => y.sizeOfSubset);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution ret = sol.Clone();
            HierarchicalRandomLocalSearchNode root = new HierarchicalRandomLocalSearchNode(p, sol.Clone(), 1.00, 1, range);
            ISolution bestDom = sol.Clone();
            ISolution s_old = sol.Clone();
            ISolution highBest = sol.Clone();
            ISolution midBest = sol.Clone();
            long endCount = limit;
            long halfCount = limit / 2;
            int subLoop = 0;
            int bestLoop = 0;
            sizeOfNeighborhood = p.OperationSet().Count();

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:err:dom:x");

            //for (int i = 0; i < 1; i++)
            //    root.child.Add(new HierarchicalRandomLocalSearchTree05(p, root.s, targetHighAct, initialSizeOfSubset, range));

            for (int loop = 1; true; loop++)
            {
                //探索の実施および最良解の更新
                endCount -= root.Search();
                if (root.GetBestDeep().Value < ret.Value)
                {
                    ret = root.GetBestDeep().Clone();
                    bestLoop = 0;
                }

                //コンソールへの出力
                Console.WriteLine(loop + ":" + bestLoop + ":" + ret.Value + ":" + highBest.Value + ":" + root.child.Count() + ":" + root.child.Sum((n) => n.child.Count()) + ":" + endCount);

                //ツリーの形成
                //High探索
                if (root.isRangeBest)
                {
                    root.child.Add(new HierarchicalRandomLocalSearchNode(p, root.s, targetHighAct, root.sizeOfSubset, range));
                }

                while (10 <= root.child.Count())
                {
                    //Console.Write(root.child.Count() + ", ");
                    for (int i = 0; i < root.child.Count; i++)
                    {
                        if (root.child[i].GetBestLoopDeep() == root.child.Max((c) => c.GetBestLoopDeep()))
                        {
                            root.child.RemoveAt(i);
                        }
                    }
                }

                //Mid探索
                for (int i = 0; i < root.child.Count; i++)
                {
                    if (root.child[i].isRangeBest)
                    {
                        HierarchicalRandomLocalSearchNode node = root.child.ArgMin((n) => n.s.Value);
                        node.child.Add(new HierarchicalRandomLocalSearchNode(p, node.s, targetMidAct, node.sizeOfSubset, range));
                    }
                }

                while (1 < root.child.Count() && root.child.Count() * 3 <= root.child.Sum((c) => c.child.Count()))
                {
                    //Console.Write(root.child.Sum((c) => c.child.Count()) + ", ");
                    for (int i = 0; i < root.child.Count; i++)
                    {
                        for (int j = 0; j < root.child[i].child.Count; j++)
                        {
                            if (root.child[i].child[j].GetBestLoopDeep() == root.child[i].child.Max((c) => c.GetBestLoopDeep()))
                            {
                                root.child[i].child.RemoveAt(j);
                            }
                        }
                    }
                }

                //Low探索
                if (0 < root.child.Count() && 0 < root.child.Min((c) => c.child.Count()) && root.child.Min((n) => 0 < n.child.Count() ? n.child.Min((m) => m.s.Value) : int.MaxValue) < midBest.Value)
                {
                    List<HierarchicalRandomLocalSearchNode> nodes = new List<HierarchicalRandomLocalSearchNode>();

                    for (int i = 0; i < root.child.Count(); i++)
                    {
                        for (int j = 0; j < root.child[i].child.Count(); j++)
                        {
                            if (root.child[i].child[j].s.Value < midBest.Value)
                            {
                                HierarchicalRandomLocalSearchNode node = root.child[i].child.ArgMin((n) => n.s.Value);
                                node.child.Clear();
                                nodes.Add(node);
                                midBest = node.s.Clone();
                            }
                        }
                    }

                    HierarchicalRandomLocalSearchNode bestNode = nodes.ArgMin((n) => n.s.Value);
                    bestNode.child.Add(new HierarchicalRandomLocalSearchNode(p, bestNode.s, targetLowAct, bestNode.sizeOfSubset, range));
                }

                for (int i = 0; i < root.child.Count(); i++)
                {
                    for (int j = 0; j < root.child[i].child.Count(); j++)
                    {
                        for (int k = 0; k < root.child[i].child[j].child.Count(); k++)
                        {
                            if (100 <= root.child[i].child[j].child[k].GetBestLoopDeep())
                            {
                                root.child[i].child[j].child.RemoveAt(k);
                            }
                        }
                    }
                }

                //後処理
                if (endCount <= 0)
                {
                    w.WriteLine(ret.ToString());
                    return ret;
                }

                subLoop++;
                bestLoop++;
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
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + ",range=" + range + "]";
        }

        public HierarchicalRandomLocalSearch05(long limit, double targetHighAct, double targetMidAct, double targetLowAct, int range)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetMidAct = targetMidAct;
            this.targetLowAct = targetLowAct;
            this.initialSizeOfSubset = 1;
            this.range = range;
        }

    }
}
