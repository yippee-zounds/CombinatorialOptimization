using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class HierarchicalRandomLocalSearch04 : IOptimizationAlgorithm
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

        private void Search(List<HierarchicalRandomLocalSearchTree> xs)
        {
            for (int i = 0; i < xs.Count; i++)
            {
                xs[i].Search();
            }
        }

        private long CalcCount(List<HierarchicalRandomLocalSearchTree> x)
        {
            return x.Sum((y) => y.sizeOfSubset);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution ret = sol.Clone();
            HierarchicalRandomLocalSearchTree root = new HierarchicalRandomLocalSearchTree(p, sol.Clone(), 1.00, 1);
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

            for (int i = 0; i < 1; i++)
                root.child.Add(new HierarchicalRandomLocalSearchTree(p, root.s, targetHighAct, initialSizeOfSubset));

            for (int loop = 1; true; loop++)
            {
                //コンソールへの出力
                Console.WriteLine(loop + ":" + bestLoop + ":" + ret.Value + ":" + highBest.Value + ":" + root.child.Count() + ":" + root.child.Sum((n) => n.child.Count()) + ":" + endCount);

                //ツリーの形成

                //High探索
                if (bestLoop % 1000 == 0)
                {
                    root.child.Add(new HierarchicalRandomLocalSearchTree(p, root.s, targetHighAct, root.sizeOfSubset));
                    /*
                    if (endCount <= halfCount)
                    {
                        int wt = 0;
                        int worstT = int.MinValue;

                        for (int i = 0; i < root.child.Count; i++)
                        {
                            if (worstT < root.child[i].GetBestDeep().Value)
                            {
                                wt = i;
                                worstT = root.child[i].GetBestDeep().Value;
                            }
                        }

                        root.child.RemoveAt(wt);
                    }*/
                }

                for (int i = 0; i < root.child.Count; i++)
                {
                    if (1 < root.child.Count && 10000 <= root.child[i].GetBestLoopDeep())
                    {
                        root.child.RemoveAt(i);
                    }
                }

                //Mid探索
                if (bestLoop % 1000 == 0)
                {
                    HierarchicalRandomLocalSearchTree node = root.child.ArgMin((n) => n.s.Value);
                    node.child.Add(new HierarchicalRandomLocalSearchTree(p, node.s, targetMidAct, node.sizeOfSubset));
                }

                if (1000 < loop && root.child.Min((n) => n.s.Value) < highBest.Value)
                {
                    HierarchicalRandomLocalSearchTree node = root.child.ArgMin((n) => n.s.Value);
                    node.child.Add(new HierarchicalRandomLocalSearchTree(p, node.s, targetMidAct, node.sizeOfSubset));
                    highBest = node.s.Clone();
                }

                for (int i = 0; i < root.child.Count; i++)
                {
                    for (int j = 0; j < root.child[i].child.Count; j++)
                    {
                        if (1000 <= root.child[i].child[j].GetBestLoopDeep())
                        {
                            root.child[i].child.RemoveAt(j);
                        }
                    }
                }

                //Low探索
                if (root.child.Min((n) => 0 < n.child.Count() ? n.child.Min((m) => m.s.Value) : int.MaxValue) < midBest.Value)
                {
                    List<HierarchicalRandomLocalSearchTree> nodes = new List<HierarchicalRandomLocalSearchTree>();

                    for (int i = 0; i < root.child.Count(); i++)
                    {
                        for (int j = 0; j < root.child[i].child.Count(); j++)
                        {
                            if (root.child[i].child[j].s.Value < midBest.Value)
                            {
                                HierarchicalRandomLocalSearchTree node = root.child[i].child.ArgMin((n) => n.s.Value);
                                node.child.Clear();
                                nodes.Add(node);
                                midBest = node.s.Clone();
                            }
                        }
                    }

                    HierarchicalRandomLocalSearchTree bestNode = nodes.ArgMin((n) => n.s.Value);
                    bestNode.child.Add(new HierarchicalRandomLocalSearchTree(p, bestNode.s, targetLowAct, bestNode.sizeOfSubset));
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
                /*
                if (bestLoop % 200 == 0)
                {
                    int bt = 0;
                    int bestT = int.MaxValue;

                    for (int i = 0; i < root.child.Count; i++)
                    {
                        if (root.child[i].best.Last().Value < bestT)
                        {
                            bt = i;
                            bestT = root.child[i].best.Last().Value;
                        }
                    }
                    root.child[bt].child.Add(new HierarchicalRandomLocalSearchTree(p, root.child[bt].best.Last(), targetMidAct, root.child[bt].sizeOfSubset));

                    if (endCount <= halfCount)
                    {
                        int wt1 = 0;
                        int wt2 = 0;
                        int worstT = int.MinValue;

                        for (int i = 0; i < root.child.Count(); i++)
                        {
                            for (int j = 0; j < root.child[i].child.Count(); j++)
                            {
                                if (worstT < root.child[i].child[j].GetBestDeep().Value)
                                {
                                    wt1 = i;
                                    wt2 = j;
                                    worstT = root.child[i].child[j].GetBestDeep().Value;
                                }
                            }
                        }

                        root.child[wt1].child.RemoveAt(wt2);
                    }
                }
                */

                /*
                if (bestLoop % 400 == 0)
                {
                    HierarchicalRandomLocalSearchTree bestNode = null;
                    int bestNodeValue = int.MaxValue;

                    for (int i = 0; i < root.child.Count; i++)
                    {
                        for (int j = 0; j < root.child[i].child.Count(); j++)
                        {
                            root.child[i].child[j].child.Clear();
                            if (root.child[i].child[j].GetBest().Value < bestNodeValue)
                            {
                                bestNode = root.child[i].child[j];
                                bestNodeValue = bestNode.GetBest().Value;
                            }
                        }
                    }

                    bestNode.child.Add(new HierarchicalRandomLocalSearchTree(p, bestNode.GetBest(), targetLowAct, bestNode.sizeOfSubset));
                }
                */

                //探索の実施および最良解の更新
                endCount -= root.Search();
                if (root.GetBestDeep().Value < ret.Value)
                {
                    ret = root.GetBestDeep().Clone();
                    bestLoop = 0;
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
            return this.GetType().Name + "[lim=" + limit + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + "]";
        }

        public HierarchicalRandomLocalSearch04(long limit, double targetHighAct, double targetMidAct, double targetLowAct)
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