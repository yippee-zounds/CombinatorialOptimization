using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class HierarchicalRandomLocalSearch071 : IOptimizationAlgorithm
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
            HierarchicalRandomLocalSearchNode root = new HierarchicalRandomLocalSearchNode(p, sol.Clone(), 1.00, 1, range);
            ISolution bestDom = sol.Clone();
            long endCount = limit;
            long halfCount = limit / 2;
            int subLoop = 0;
            int bestLoop = 0;
            sizeOfNeighborhood = p.OperationSet().Count();
            string mode = "";
            bool flag1 = false;
            bool flag2 = false;

            for (int i = 1; i < trj; i++)
            {
                root.child.Add(new HierarchicalRandomLocalSearchNode(p, p.CreateRandomSolution(), targetHighAct, root.sizeOfSubset, range));
            }

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:err:dom:x");

            for (int loop = 1; true; loop++)
            {
                //探索の実施および最良解の更新
                endCount -= root.Search();
                if (All(root).Min((n) => n.s.Value) < ret.Value)
                {
                    ret = All(root).ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, ret);

                    if (0 < High(root).Count() && All(root).Min((n) => n.best.Value) == High(root).Min((n) => n.best.Value)) mode = "H";
                    else if (0 < Mid(root).Count() && All(root).Min((n) => n.best.Value) == Mid(root).Min((n) => n.best.Value)) mode = "M";
                    else if (0 < Low(root).Count() && All(root).Min((n) => n.best.Value) == Low(root).Min((n) => n.best.Value)) mode = "L";
                    else if (0 < Con(root).Count() && All(root).Min((n) => n.best.Value) == Con(root).Min((n) => n.best.Value)) mode = "C";
                }

                //コンソールへの出力
                //Console.WriteLine(loop + ":" + bestLoop + ":" + ret.Value + ":" + Low(root).Min((n) => n.s.Value) + ":" + Mid(root).Min((n) => n.s.Value) + ":" + High(root).Min((n) => n.s.Value) + ":" + Low(root).Count() + ":" + (String.Join(",", Mid(root).Select((n) => n.child.Count()))) + ":" + High(root).Count() + ":" + endCount);
                Console.WriteLine(loop + ":" + ret.Value + ":" + bestDom.Value + ":" + (0 < Con(root).Count() ? Con(root).Min((n) => n.s.Value) : 0).ToString("D5") + ":" + (0 < Low(root).Count() ? Low(root).Min((n) => n.s.Value) : 0).ToString("D5") + ":"
                    + (0 < Mid(root).Count() ? Mid(root).Min((n) => n.s.Value) : 0).ToString("D5") + ":" + (0 < High(root).Count() ? High(root).Min((n) => n.s.Value) : 0).ToString("D5")
                    + ":" + Con(root).Count() + ":" + Low(root).Count() + ":" + (String.Join(",", High(root).Select((n) => n.child.Count()))) + ":" + High(root).Count() + ":" + endCount + ":" + mode);

                //ツリーの形成
                //High探索
                /*
                if (2 * range < bestLoop)
                {
                    root.child.Add(GetCenterPoint(p, High(root).Where((n) => 0 < n.child.Count()).Select((n) => n.child.Select((k) => k.best).ArgMinStrict((m) => m.Value))));
                    bestLoop = 0;
                }/**/
                
                while (trj < High(root).Count())
                {
                    if (0 < High(root).Where((t) => 0 < t.bestLoop && All(t).Min((n) => n.best.Value) != All(root).Min((n) => n.best.Value)).Count())
                    {
                        Remove(root, High(root).Where((t) => 0 < t.bestLoop && All(t).Min((n) => n.best.Value) != All(root).Min((n) => n.best.Value)).ArgMax((n) => n.bestLoop));
                    }
                    else if(0 < High(root).Where((t) => All(t).Min((n) => n.best.Value) != All(root).Min((n) => n.best.Value)).Count())
                    {
                        Remove(root, High(root).Where((t) => All(t).Min((n) => n.best.Value) != All(root).Min((n) => n.best.Value)).ArgMax((n) => n.bestLoop));
                    }
                    else
                    {
                        Remove(root, High(root).ArgMax((n) => n.bestLoop));
                    }
                }

                //Mid探索
                foreach (var v in High(root).Where((n) => n.isRangeBest))
                {
                    if (v.child.Count() < High(root).Count())
                    {
                        v.child.Add(new HierarchicalRandomLocalSearchNode(p, v.s, targetMidAct, v.sizeOfSubset, range));
                    }
                }

                while (High(root).Count() * 2 < Mid(root).Count())
                {
                    Remove(root, Mid(root).Where((t) => 0 < t.bestLoop && All(t).Min((n) => n.best.Value) != Mid(root).Min((n) => n.best.Value)).ArgMax((n) => n.bestLoop));
                }

                //Low探索
                foreach (var v in Mid(root).Where((n) => n.isRangeBest))
                {
                    if (v.child.Count() == 0)
                    {
                        v.child.Add(new HierarchicalRandomLocalSearchNode(p, v.s, targetMidAct, v.sizeOfSubset, range));
                    }
                }

                if (mode == "L" && bestLoop == 0)
                {
                    flag1 = true;
                }

                if (flag1 && bestLoop != 0)
                {
                    flag1 = false;
                    HierarchicalRandomLocalSearchNode h = new HierarchicalRandomLocalSearchNode(p, ret.Clone(), targetHighAct, 1, 1000);
                    HierarchicalRandomLocalSearchNode m = new HierarchicalRandomLocalSearchNode(p, ret.Clone(), targetMidAct, 1, 1000);

                    h.child.Add(m);
                    root.child.Add(h);
                }

                foreach (var v in Low(root).Where((n) => range / 10 < n.bestLoop))
                {
                    Remove(root, v);
                }

                while (High(root).Count() < Low(root).Count())
                {
                    Remove(root, Low(root).Where((t) => t != Low(root).ArgMin((n) => n.best.Value)).ArgMax((n) => n.bestLoop));
                }
                
                //Con探索
                if (0 < Low(root).Count() && mode == "L" && bestLoop == 0)
                {
                    foreach(var n in Low(root))
                    {
                        n.child.Clear();
                    }

                    var node = Low(root).ArgMin((n) => n.s.Value);
                    node.child.Add(new HierarchicalRandomLocalSearchNode(p, node.s, 0.0, p.OperationSet().Count(), range));
                }

                if (mode == "C" && bestLoop == 0)
                {
                    flag2 = true;
                }

                if (flag2 && bestLoop != 0)
                {
                    flag2 = false;
                    HierarchicalRandomLocalSearchNode h = new HierarchicalRandomLocalSearchNode(p, ret.Clone(), targetHighAct, 1, 1000);
                    HierarchicalRandomLocalSearchNode m = new HierarchicalRandomLocalSearchNode(p, ret.Clone(), targetMidAct, 1, 1000);
                    HierarchicalRandomLocalSearchNode l = new HierarchicalRandomLocalSearchNode(p, ret.Clone(), targetLowAct, 1, 1000);

                    m.child.Add(l);
                    h.child.Add(m);
                    root.child.Add(h);
                }

                foreach (var v in Con(root).Where((n) => range / 10 < n.bestLoop))
                {
                    Remove(root, v);
                }

                while (1 < Con(root).Count())
                {
                    Remove(root, Low(root).Where((t) => t != Con(root).ArgMin((n) => n.best.Value)).ArgMax((n) => n.bestLoop));
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

        private HierarchicalRandomLocalSearchNode GetCenterPoint(IOptimizationProblem p, IEnumerable<ISolution> nodes)
        {
            int sizeOfSubset = p.OperationSet().Count();
            ISolution s = p.CreateRandomSolution();
            ISolution tmp = null;
            ISolution[] ss = nodes.ToArray();

            while (true)
            {
                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => s.OperationDistance(op, ss));
                tmp = s.Clone().Apply(bestOp);

                if (0 <= s.OperationDistance(bestOp, ss)) return new HierarchicalRandomLocalSearchNode(p, s, targetHighAct, 1, range);

                s = tmp;
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
                        for (int l = 0; l < root.child[i].child[j].child[k].child.Count(); l++)
                        {
                            if (target == root.child[i].child[j].child[k].child[l]) root.child[i].child[j].child[k].child.RemoveAt(l);
                        }

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
                        for (int l = 0; l < root.child[i].child[j].child[k].child.Count(); l++)
                        {
                            if (child == root.child[i].child[j].child[k].child[l]) return root.child[i].child[j].child[k];
                        }

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

                        for (int l = 0; l < root.child[i].child[j].child[k].child.Count(); l++)
                        {
                            yield return root.child[i].child[j].child[k].child[l];
                        }
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


        private IEnumerable<HierarchicalRandomLocalSearchNode> Con(HierarchicalRandomLocalSearchNode root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    for (int k = 0; k < root.child[i].child[j].child.Count(); k++)
                    {
                        for (int l = 0; l < root.child[i].child[j].child[k].child.Count(); l++)
                        {
                            yield return root.child[i].child[j].child[k].child[l];
                        }
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

        public HierarchicalRandomLocalSearch071(long limit, int trj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
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
