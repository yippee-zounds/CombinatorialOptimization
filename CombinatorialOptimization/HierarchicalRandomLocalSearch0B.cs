using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class HierarchicalRandomLocalSearch0B : IOptimizationAlgorithm
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
            long endCount = limit;
            int subLoop = 0;
            int bestLoop = 0;
            sizeOfNeighborhood = p.OperationSet().Count();
            List<ISolution> rets = new List<ISolution>();
            Diameter dm = null;

            w.WriteLine("loop:vbest:vhigh:vmid:vlow:nhigh:nmid:nlow:lowin:lowex:midin:midex:x");
            Console.WriteLine(this.ToString());

            for (int loop = 1; true; loop++)
            {
                //探索の実施および最良解の更新
                endCount -= root.Search();
                if (All(root).Min((n) => n.s.Value) < ret.Value)
                {
                    ret = All(root).ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;

                    rets.Add(ret.Clone());
                    /*
                    if (10 <= rets.Count())
                    {
                        Diameter dm = Diameter.CalculateDiameter(rets.Skip(rets.Count() - 10));
                        Console.Write(dm.DiameterValue + ":");
                        Console.WriteLine(rets[rets.Count() - 2].DistanceTo(rets[rets.Count() - 1]));
                    }
                    /*
                    if (2 <= rets.Count())
                    {
                        int dd = 3;
                        int ltd = 0;
                        for (int i = 1; i < Math.Min(rets.Count() - 1, dd); i++)
                        {
                            ltd += rets.ElementAt(rets.Count() - i).DistanceTo(rets.ElementAt(rets.Count() - i - 1));
                        }
                        double lta = (double)rets[rets.Count() - 1].DistanceTo(rets[Math.Max(rets.Count() - dd, 0)]) / ltd;

                        if (rets.Count() == 2) Console.WriteLine("bestd:lta\n" + rets[0].DistanceTo(rets[1]) + ":" + lta);
                        else if (3 <= rets.Count()) Console.WriteLine(rets[rets.Count() - 2].DistanceTo(rets[rets.Count() - 1]) + ":" + lta);
                    }/**/
                }
                int midIn = (High(root).Count() < 2) ? 0 : High(root).SelectMany((x) => High(root), (x, y) => (x == y) ? int.MaxValue : x.s.DistanceTo(y.s)).Min((z) => z);
                int midEx = (High(root).Count() < 2) ? 0 : High(root).SelectMany((x) => High(root), (x, y) => (x == y) ? int.MinValue : x.s.DistanceTo(y.s)).Max((z) => z);
                int lowIn = (Mid (root).Count() < 2) ? 0 : Mid (root).SelectMany((x) => Mid (root), (x, y) => (x == y) ? int.MaxValue : x.s.DistanceTo(y.s)).Min((z) => z);
                int lowEx = (Mid (root).Count() < 2) ? 0 : Mid (root).SelectMany((x) => Mid (root), (x, y) => (x == y) ? int.MinValue : x.s.DistanceTo(y.s)).Max((z) => z);

                //出力
                Console.WriteLine(loop + ":" + ret.Value + ":" 
                    + (0 < Mid(root).Count() ? Mid(root).Min((n) => n.s.Value) : sol.Value) + ":" + (0 < High(root).Count() ? High(root).Min((n) => n.s.Value) : sol.Value)
                    + ":" + Mid(root).Count() + ":" + High(root).Count()
                    + ":" + lowIn + "," + lowEx + ":" + midIn + "," + midEx + ":" + endCount);
                
                w.WriteLine(loop + ":" + ret.Value + ":" + root.s.Value
                    + ":" + (0 < High(root).Count() ? High(root).Min((n) => n.s.Value) : root.s.Value)  + ":" + (0 < Mid(root).Count() ? Mid(root).Min((n) => n.s.Value) : root.s.Value)
                    + ":" + 1 + ":" + High(root).Count() + ":" + Mid(root).Count()
                    + ":" + lowIn + ":" + lowEx + ":" + midIn + ":" + midEx + ":" + ret);

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
                
                while (Math.Max(1, High(root).Count() / 3) < Mid(root).Count())
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
                    Console.WriteLine();
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

        public HierarchicalRandomLocalSearch0B(long limit, int trj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
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
