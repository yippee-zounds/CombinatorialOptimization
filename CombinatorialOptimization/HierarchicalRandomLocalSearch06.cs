using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class HierarchicalRandomLocalSearch06 : IOptimizationAlgorithm
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
            ISolution lowBest = sol.Clone();
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
                //初期化
                bool renewMidBest = false;

                //探索の実施および最良解の更新
                endCount -= root.Search();
                if (root.GetBestDeep().Value < ret.Value)
                {
                    ret = root.GetBestDeep().Clone();
                    bestLoop = 0;
                }
                int lowCount = 0;

                for (int i = 0; i < root.child.Count; i++)
                {
                    if (root.child[i].s.Value < highBest.Value) highBest = root.child[i].s.Clone();

                    for (int j = 0; j < root.child[i].child.Count; j++)
                    {
                        if (root.child[i].child[j].s.Value < midBest.Value)
                        {
                            midBest = root.child[i].child[j].s.Clone();
                            renewMidBest = true;
                        }
                        for (int k = 0; k < root.child[i].child[j].child.Count; k++)
                        {
                            if (root.child[i].child[j].child[k].s.Value < lowBest.Value) lowBest = root.child[i].child[j].child[k].s.Clone();
                            ++lowCount;
                        }
                    }
                }

                //コンソールへの出力
                Console.WriteLine(loop + ":" + bestLoop + ":" + ret.Value + ":" + lowBest.Value + ":" + midBest.Value + ":" + highBest.Value + ":" + root.child.Count() + ":" + (String.Join(",", root.child.Select((n) => n.child.Count()))) + ":" + lowCount + ":" + endCount);

                //ツリーの形成
                //High探索
                if (root.isRangeBest)
                {
                    root.child.Add(new HierarchicalRandomLocalSearchNode(p, root.s, targetHighAct, root.sizeOfSubset, range));
                }

                while (10 < root.child.Count())
                {
                    int worstI = 0;
                    int worstV = int.MinValue;
                    int bestV = int.MinValue;

                    for (int i = 0; i < root.child.Count; i++)
                    {
                        if (root.child[i].best.Value < bestV) bestV = root.child[i].best.Value;
                    }

                    for (int i = 0; i < root.child.Count; i++)
                    {
                        if (root.child[i].best.Value != bestV && worstV < root.child[i].bestLoop)
                        {
                            worstV = root.child[i].bestLoop;
                            worstI = i;
                        }
                    }
                    root.child.RemoveAt(worstI);
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

                if (1 < root.child.Count())
                {
                    while (root.child.Count() * 3 < root.child.Sum((c) => c.child.Count()))
                    {
                        int worstI = 0;
                        int worstJ = 0;
                        int worstV = int.MinValue;
                        int bestV = int.MinValue;

                        for (int i = 0; i < root.child.Count; i++)
                        {
                            for (int j = 0; j < root.child[i].child.Count; j++)
                            {
                                if (root.child[i].child[j].best.Value < bestV) bestV = root.child[i].child[j].best.Value;
                            }
                        }
                                //Console.Write(root.child.Sum((c) => c.child.Count()) + ", ");
                        for (int i = 0; i < root.child.Count; i++)
                        {
                            for (int j = 0; j < root.child[i].child.Count; j++)
                            {
                                if (root.child[i].child[j].best.Value != bestV && worstV < root.child[i].child[j].bestLoop)
                                {
                                    worstV = root.child[i].child[j].bestLoop;
                                    worstI = i;
                                    worstJ = j;
                                }
                            }
                        }
                        root.child[worstI].child.RemoveAt(worstJ);
                    }
                }

                //Low探索
                if (0 < root.child.Count() && 0 < root.child.Sum((c) => c.child.Count()))
                {
                    int bestI = 0;
                    int bestJ = 0;
                    int bestM = int.MaxValue;
                    for (int i = 0; i < root.child.Count(); i++)
                    {
                        for (int j = 0; j < root.child[i].child.Count(); j++)
                        {
                            if (root.child[i].child[j].s.Value < bestM)
                            {
                                bestM = root.child[i].child[j].s.Value;
                                bestI = i;
                                bestJ = j;
                            }
                        }
                    }

                    if (bestM == midBest.Value)
                    {
                        //midBest = root.child[bestI].child[bestJ].best.Clone();
                        root.child[bestI].child[bestJ].child.Clear();
                        root.child[bestI].child[bestJ].child.Add(new HierarchicalRandomLocalSearchNode(p, root.child[bestI].child[bestJ].s, targetLowAct, root.child[bestI].child[bestJ].sizeOfSubset, range));
                    }

                    //while (10 < root.child.Sum((c) => c.child.Sum((cc) => cc.child.Count())))
                    {
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
                    }
                }

                //Low探索
                if (0 < root.child.Count() && 0 < root.child.Sum((c) => c.child.Count()))
                {
                    int bestI = 0;
                    int bestJ = 0;
                    int bestK = 0;
                    int bestL = int.MaxValue;
                    for (int i = 0; i < root.child.Count(); i++)
                    {
                        for (int j = 0; j < root.child[i].child.Count(); j++)
                        {
                            for (int k = 0; k < root.child[i].child[j].child.Count(); k++)
                            {
                                if (root.child[i].child[j].child[k].best.Value < bestL)
                                {
                                    bestL = root.child[i].child[j].child[k].best.Value;
                                    bestI = i;
                                    bestJ = j;
                                    bestK = k;
                                }
                            }
                        }
                    }

                    if (bestL <= lowBest.Value)
                    {
                        root.child[bestI].child.Add(new HierarchicalRandomLocalSearchNode(p, root.child[bestI].child[bestJ].child[bestK].s, targetMidAct, root.child[bestI].child[bestJ].child[bestK].sizeOfSubset, range));
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

        private HierarchicalRandomLocalSearchNode Parent(HierarchicalRandomLocalSearchNode root, HierarchicalRandomLocalSearchNode child)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                if (child == root.child[i]) return root;

                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    if (child == root.child[i].child[j]) return root.child[i];

                    for (int k = 0; k < root.child[i].child[j].child.Count(); k++)
                    {
                        if (child == root.child[i].child[j].child[k]) return root.child[i].child[j];
                    }
                }
            }

            return null;
        }

        private IEnumerable<HierarchicalRandomLocalSearchNode> All(HierarchicalRandomLocalSearchNode root)
        {
            yield return root;

            for (int i = 0; i < root.child.Count(); i++)
            {
                yield return root.child[i];

                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    yield return root.child[i].child[j];

                    for (int k = 0; k < root.child[i].child[j].child.Count(); k++)
                    {
                        yield return root.child[i].child[j].child[k];
                    }
                }
            }
        }

        private IEnumerable<HierarchicalRandomLocalSearchNode> High(HierarchicalRandomLocalSearchNode root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                yield return root.child[i];
            }
        }

        private IEnumerable<HierarchicalRandomLocalSearchNode> Mid(HierarchicalRandomLocalSearchNode root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    yield return root.child[i].child[j];
                }
            }
        }

        private IEnumerable<HierarchicalRandomLocalSearchNode> Low(HierarchicalRandomLocalSearchNode root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    for (int k = 0; k < root.child[i].child[j].child.Count(); k++)
                    {
                        yield return root.child[i].child[j].child[k];
                    }
                }
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

        public HierarchicalRandomLocalSearch06(long limit, double targetHighAct, double targetMidAct, double targetLowAct, int range)
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
