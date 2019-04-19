using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalRandomLocalSearch03 : IOptimizationAlgorithm
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

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution ret = sol.Clone();
            HierarchicalRandomLocalSearchElement root = new HierarchicalRandomLocalSearchElement(p, sol.Clone(), targetHighAct, 1, range);
            long endCount = limit;
            int bestLoop = 0;
            ISolution bestDom = null;
            string mode = "";
            sizeOfNeighborhood = p.OperationSet().Count();
            string lStatus = "";
            string mStatus = "";
            int lowIdCount = 0;
            int midIdCount = 0;
            int subLoop1 = 0;
            int subLoop2 = 0;
            //MetricSpace midPool = new MetricSpace();
            //MetricSpace lowPool = new MetricSpace();

            w.WriteLine("loop:vbest:vhigh:vmid:vlow:nhigh:nmid:nlow:lowin:lowex:midin:midex:x");
            
            for (int loop = 1; true; loop++, bestLoop++, subLoop1++, subLoop2++)
            {
                endCount -= root.Search();
                if (All(root).Min((n) => n.s.Value) < ret.Value)
                {
                    ret = All(root).ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, ret);

                    if (0 < Con(root).Count() && All(root).Min((n) => n.best.Value) == Con(root).Min((n) => n.best.Value)) mode = "C";
                    else if (0 < Low(root).Count() && All(root).Min((n) => n.best.Value) == Low(root).Min((n) => n.best.Value)) mode = "L";
                    else if (0 < Mid(root).Count() && All(root).Min((n) => n.best.Value) == Mid(root).Min((n) => n.best.Value)) mode = "M";
                }

                //コンソールへの出力
                lStatus = "";
                foreach (var e in Low(root))
                {
                    if (All(e).Min((n) => n.best.Value) == ret.Value)
                    {
                        lStatus += e.Id + "*";
                    }
                    else if (e.loop < 10)
                    {
                        lStatus += e.Id + "+";
                    }
                    else if (e.loop < 30)
                    {
                        lStatus += e.Id + "-";
                    }
                    else if (range / 10 < All(e).Min((n) => n.bestLoop))
                    {
                        lStatus += e.Id + "@";
                    }
                    else
                    {
                        lStatus += e.Id + " ";
                    }
                }

                mStatus = "";
                foreach (var e in Mid(root))
                {
                    if (All(e).Min((n) => n.best.Value) == ret.Value)
                    {
                        mStatus += e.Id + "*";
                    }
                    else if (e.loop < 1000)
                    {
                        mStatus += e.Id + "+";
                    }
                    else if (e.loop < 3000)
                    {
                        mStatus += e.Id + "-";
                    }
                    else if (10 * range < All(e).Min((n) => n.bestLoop))
                    {
                        mStatus += e.Id + "@";
                    }
                    else
                    {
                        mStatus += e.Id + " ";
                    }
                }

                Console.WriteLine(loop + ":" + ret.Value + ":" + bestDom.Value + ":" 
                    //+ (0 < Low(root).Count() ? Low(root).Min((n) => n.s.Value) : 0).ToString("D5") + ":" + (0 < Mid(root).Count() ? Mid(root).Min((n) => n.s.Value) : 0).ToString("D5") + ":" 
                    + root.s.Value + ":" + Con(root).Count() + ":" + Mid(root).Count() + ":" + endCount + ":" + mode);

                //プールの構成
                //////Midレベル
                if (1000 < root.loop && (root.child.Count() == 0 || root.bestLoop == 0))
                {
                    root.pool.Add(new HierarchicalRandomLocalSearchElement(root, root.best, targetMidAct, midIdCount++));
                }

                foreach (var e in Low(root).Where((n) => 100 < n.bestLoop))
                {
                    e.pool.Add(new HierarchicalRandomLocalSearchElement(e, targetMidAct, midIdCount++));
                }

                //////Lowレベル
                foreach (var e in Mid(root).Where((n) => 1000 < n.loop && n.bestLoop == 0))
                {
                    e.pool.Add(new HierarchicalRandomLocalSearchElement(e, targetLowAct, lowIdCount++));
                }
                
                foreach(var e in Con(root).Where((n) => 10 < n.bestLoop))
                {
                    e.pool.Add(new HierarchicalRandomLocalSearchElement(e, targetLowAct, lowIdCount++));
                }

                //ツリーの構成
                //////Midレベル
                root.child.Clear();
                foreach(var e in root.pool.SelectDiversity(trj))
                {
                    root.child.Add((HierarchicalRandomLocalSearchElement)e);
                }
                
                //////Lowレベル
                foreach(var m in Mid(root))
                {
                    m.child.Clear();
                    foreach(var e in m.pool.SelectDiversity(trj))
                    {
                        m.child.Add((HierarchicalRandomLocalSearchElement)e);
                    }
                }

                //////Conレベル
                if (0 < Low(root).Where((n) => n.bestLoop == 0).Count())
                {
                    var e = Low(root).Where((n) => n.bestLoop == 0).ArgMin((n) => n.s.Value);
                    e.child.Clear();
                    e.child.Add(new HierarchicalRandomLocalSearchElement(e, 0.0));
                }

                if (endCount <= 0) return ret;
            }
        }

        private HierarchicalRandomLocalSearchElement GetCenterPoint(IOptimizationProblem p, IEnumerable<ISolution> nodes)
        {
            int sizeOfSubset = p.OperationSet().Count();
            ISolution s = p.CreateRandomSolution();
            ISolution tmp = null;
            ISolution[] ss = nodes.ToArray();

            while (true)
            {
                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => s.OperationDistance(op, ss));
                tmp = s.Clone().Apply(bestOp);

                if (0 <= s.OperationDistance(bestOp, ss)) return new HierarchicalRandomLocalSearchElement(p, s, targetHighAct, 1, range);

                s = tmp;
            }
        }
        private void Remove(HierarchicalRandomLocalSearchElement root, HierarchicalRandomLocalSearchElement target)
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

        private HierarchicalRandomLocalSearchElement Parent(HierarchicalRandomLocalSearchElement root, HierarchicalRandomLocalSearchElement child)
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

        private IEnumerable<HierarchicalRandomLocalSearchElement> All(HierarchicalRandomLocalSearchElement root)
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

        private IEnumerable<HierarchicalRandomLocalSearchElement> Mid(HierarchicalRandomLocalSearchElement root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                yield return root.child[i];
            }
        }

        private IEnumerable<HierarchicalRandomLocalSearchElement> Low(HierarchicalRandomLocalSearchElement root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    yield return root.child[i].child[j];
                }
            }
        }

        private IEnumerable<HierarchicalRandomLocalSearchElement> Con(HierarchicalRandomLocalSearchElement root)
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

        /*
        private IEnumerable<HierarchicalRandomLocalSearchElement> Con(HierarchicalRandomLocalSearchElement root)
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
        */

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",trj=" + trj + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + ",range=" + range + "]";
        }

        public HierarchicalRandomLocalSearch03(long limit, int trj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
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
