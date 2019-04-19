using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class HierarchicalRandomLocalSearch08 : IOptimizationAlgorithm
    {
        private long limit;
        private int trj;
        private double targetHighAct;
        private double targetMidAct;
        private double targetLowAct;
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
            HierarchicalRandomLocalSearchNode root = new HierarchicalRandomLocalSearchNode(p, sol.Clone(), targetHighAct, 1, range);
            ISolution bestDom = sol.Clone();
            long endCount = limit;
            long halfCount = limit / 2;
            int subLoop = 0;
            int bestLoop = 0;
            sizeOfNeighborhood = p.OperationSet().Count();

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:err:dom:x");

            for (int loop = 1; true; loop++)
            {
                //探索の実施および最良解の更新
                endCount -= root.Search();
                if (All(root).Min((n) => n.s.Value) < ret.Value)
                {
                    ret = All(root).ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                }

                //コンソールへの出力
                //Console.WriteLine(loop + ":" + bestLoop + ":" + ret.Value + ":" + Low(root).Min((n) => n.s.Value) + ":" + Mid(root).Min((n) => n.s.Value) + ":" + High(root).Min((n) => n.s.Value) + ":" + Low(root).Count() + ":" + (String.Join(",", Mid(root).Select((n) => n.child.Count()))) + ":" + High(root).Count() + ":" + endCount);
                Console.WriteLine(loop + ":" + bestLoop + ":" + ret.Value + ":" + (0 < Low(root).Count() ? Low(root).Min((n) => n.s.Value) : sol.Value) + ":"
                    + (0 < Mid(root).Count() ? Mid(root).Min((n) => n.s.Value) : sol.Value) + ":" + (0 < High(root).Count() ? High(root).Min((n) => n.s.Value) : sol.Value)
                    + ":" + Low(root).Count() + ":" + (String.Join(",", High(root).Select((n) => n.child.Count()))) + ":" + High(root).Count() + ":" + endCount);

                //ツリーの形成
                //Mid探索
                if (root.isRangeBest)
                {
                    root.child.Add(new HierarchicalRandomLocalSearchNode(p, root.s, targetMidAct, root.sizeOfSubset, range));
                }

                while (trj < High(root).Count())
                {
                    Remove(root, High(root).Where((t) => t != High(root).ArgMin((n) => n.best.Value)).ArgMax((n) => n.bestLoop));
                }

                //Low探索
                if (0 < High(root).Count() && High(root).Min((n) => n.s.Value) == High(root).Min((n) => n.best.Value))
                {
                    var node = High(root).ArgMin((n) => n.s.Value);
                    node.child.Add(new HierarchicalRandomLocalSearchNode(p, node.s, targetLowAct, node.sizeOfSubset, range));
                }

                while (High(root).Count() < Mid(root).Count())
                {
                    Remove(root, Mid(root).Where((t) => t != Mid(root).ArgMin((n) => n.best.Value)).ArgMax((n) => n.bestLoop));
                }

                //Low探索
                if (0 < Mid(root).Count() && Mid(root).Min((n) => n.s.Value) == Mid(root).Min((n) => n.best.Value))
                {
                    var node = Mid(root).ArgMin((n) => n.s.Value);
                    Parent(root, node).child.Add(new HierarchicalRandomLocalSearchNode(p, node.s, targetMidAct, node.sizeOfSubset, range));
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

        private void Remove(HierarchicalRandomLocalSearchNode root, HierarchicalRandomLocalSearchNode target)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    for (int k = 0; k < root.child[i].child[j].child.Count(); k++)
                    {
                        if (target == root.child[i].child[j].child[k]) root.child[i].child[j].child.RemoveAt(k);
                    }

                    if (target == root.child[i].child[j]) root.child[i].child.RemoveAt(j);
                }

                if (target == root.child[i]) root.child.RemoveAt(i);
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
            return this.GetType().Name + "[lim=" + limit + ",trj=" + trj + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + ",range=" + range + "]";
        }

        public HierarchicalRandomLocalSearch08(long limit, int trj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
        {
            this.limit = limit;
            this.trj = trj;
            this.targetHighAct = targetHighAct;
            this.targetMidAct = targetMidAct;
            this.targetLowAct = targetLowAct;
            this.range = range;
        }

    }
}
