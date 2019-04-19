using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalRandomLocalSearch0322 : IOptimizationAlgorithm
    {
        private long limit;
        private int htrj;
        private int mtrj;
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
            HierarchicalRandomLocalSearchElement root = new HierarchicalRandomLocalSearchElement(p, sol.Clone(), 1.0, 1, range);
            long endCount = limit;
            int bestLoop = 0;
            ISolution bestDom = null;
            string mode = "";
            sizeOfNeighborhood = p.OperationSet().Count();
            string lStatus = "";
            string mStatus = "";
            int lowIdCount = 0;
            int midIdCount = 0;
            int highIdCount = 0;
            int subLoop1 = 0;
            int subLoop2 = 0;
            int factor = 1;
            List<HierarchicalRandomLocalSearchElement> highList = new List<HierarchicalRandomLocalSearchElement>();
            List<HierarchicalRandomLocalSearchElement> midList = new List<HierarchicalRandomLocalSearchElement>();
            List<HierarchicalRandomLocalSearchElement> lowList = new List<HierarchicalRandomLocalSearchElement>();
            List<HierarchicalRandomLocalSearchElement> conList = new List<HierarchicalRandomLocalSearchElement>();

            w.WriteLine("loop:vbest:vhigh:vmid:vlow:nhigh:nmid:nlow:lowin:lowex:midin:midex:x");
            
            for (int loop = 0; true; loop++, bestLoop++, subLoop1++, subLoop2++)
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
                    else if (0 < High(root).Count() && All(root).Min((n) => n.best.Value) == High(root).Min((n) => n.best.Value)) mode = "H";
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

                Console.WriteLine(loop + ":" + ret.Value + ":" + bestDom.Value + ":" + All(root).Min((n) => n.s.Value)
                    //+ (0 < Low(root).Count() ? Low(root).Min((n) => n.s.Value) : 0).ToString("D5") + ":" + (0 < Mid(root).Count() ? Mid(root).Min((n) => n.s.Value) : 0).ToString("D5") + ":" 
                    + ":" + Con(root).Count() + ":" + Low(root).Count() + ":" + Mid(root).Count() + ":" + High(root).Count()
                    + ":" + conList.Sum((n) => n.calc) + ":" + lowList.Sum((n) => n.calc) + ":" + midList.Sum((n) => n.calc) + ":" + highList.Sum((n) => n.calc) + ":" + root.calc
                    + ":" + conList.Count((n) => n.calc != 0) + ":" + lowList.Count((n) => n.calc != 0) + ":" + midList.Count((n) => n.calc != 0) + ":" + highList.Count((n) => n.calc != 0) + ":" + 1
                    + ":" + endCount + ":" + mode);
                DrawTree(root, 1);

                //プールの構成
                //////Highレベル
                if (root.child.Count() == 0)
                {
                    for (int i = 0; i < htrj * 10; i++)
                    {
                        var x = new HierarchicalRandomLocalSearchElement(root, p.CreateRandomSolution(), targetHighAct, highIdCount++);
                        root.pool.Add(x);
                        highList.Add(x);
                    }
                }

                foreach (var e in Mid(root).Where((n) => 1000 < n.bestLoop))
                {
                    var x = new HierarchicalRandomLocalSearchElement(e, targetHighAct, highIdCount++);
                    root.pool.Add(x);
                    highList.Add(x);
                }


                //////Midレベル
                foreach (var e in High(root).Where((n) => 100 < n.loop && n.bestLoop == 0))
                {
                    var x = new HierarchicalRandomLocalSearchElement(e, targetMidAct, midIdCount++);
                    e.pool.Add(x);
                    midList.Add(x);
                }

                foreach (var e in Low(root).Where((n) => 0 < n.bestLoop))
                {
                    var x = new HierarchicalRandomLocalSearchElement(e, targetMidAct, midIdCount++);
                    Parent(root, Parent(root, e)).pool.Add(x);
                    midList.Add(x);
                }

                //////Lowレベル
                foreach (var e in Mid(root).Where((n) => 10 < n.loop && n.bestLoop== 0))
                {
                    var x = new HierarchicalRandomLocalSearchElement(e, targetLowAct, lowIdCount++);
                    e.pool.Add(x);
                    lowList.Add(x);
                }
                
                foreach(var e in Con(root).Where((n) => 0 < n.bestLoop))
                {
                    var x = new HierarchicalRandomLocalSearchElement(e, targetLowAct, lowIdCount++);
                    Parent(root, Parent(root, e)).pool.Add(x);
                    lowList.Add(x);
                }

                //プールの刈取り
                //////Conレベル
                var tmp = new List<HierarchicalRandomLocalSearchElement>();

                foreach (var e in Con(root).Where((n) => 0 < n.bestLoop))
                {
                    tmp.Add(e);
                }

                if(0 < Con(root).Count())
                {
                    //tmp.Add(Con(root).ArgMax((n) => n.s.Value));
                }

                foreach (var e in tmp)
                {
                    Parent(root, e).pool.Remove(e);
                }

                //////Lowレベル
                tmp = new List<HierarchicalRandomLocalSearchElement>();

                foreach (var e in Low(root).Where((n) => 100 < n.bestLoop))
                {
                    tmp.Add(e);
                }

                if (0 < Low(root).Count())
                {
                    //tmp.Add(Low(root).ArgMax((n) => n.s.Value));
                }

                foreach (var e in tmp)
                {
                    Parent(root, e).pool.Remove(e);
                }

                //////Midレベル
                tmp = new List<HierarchicalRandomLocalSearchElement>();

                foreach (var e in Mid(root).Where((n) => 1000 < n.bestLoop))
                {
                    tmp.Add(e);
                }

                if (0 < Mid(root).Count())
                {
                    tmp.Add(Mid(root).ArgMax((n) => n.s.Value));
                }

                foreach (var e in tmp)
                {
                    Parent(root, e).pool.Remove(e);
                }


                //ツリーの構成
                if (loop % 100 == 0)
                {
                    //////Highレベル
                    root.child.Clear();
                    foreach (var e in root.pool.SelectDiversity(htrj * factor).OrderBy((n) => ((HierarchicalRandomLocalSearchElement)n).s.Value).Take(htrj))
                    {
                        root.child.Add((HierarchicalRandomLocalSearchElement)e);
                    }

                    //////Midレベル
                    foreach (var h in High(root))
                    {
                        h.child.Clear();
                        if (0 < h.pool.Element().Count())
                            h.child.Add((HierarchicalRandomLocalSearchElement)h.pool.Center().Item1);
                        /*
                        foreach (var e in h.pool.SelectDiversity(mtrj * factor).OrderBy((n) => ((HierarchicalRandomLocalSearchElement)n).s.Value).Take(htrj))
                        {
                            h.child.Add((HierarchicalRandomLocalSearchElement)e);
                        }/**/
                    }

                    //////Lowレベル
                    foreach (var m in Mid(root))
                    {
                        m.child.Clear();
                        if(0 < m.pool.Element().Count())
                            m.child.Add((HierarchicalRandomLocalSearchElement)m.pool.Center().Item1);
                        /*
                        foreach (var e in m.pool.SelectDiversity(2 * factor).OrderBy((n) => ((HierarchicalRandomLocalSearchElement)n).s.Value).Take(htrj))
                        {
                            m.child.Add((HierarchicalRandomLocalSearchElement)e);
                        }/**/
                    }

                    //////Conレベル
                    if (0 < Low(root).Where((n) => n.bestLoop == 0).Count())
                    {
                        var e = Low(root).Where((n) => n.bestLoop == 0).ArgMin((n) => n.s.Value);
                        e.child.Clear();
                        var x = new HierarchicalRandomLocalSearchElement(e, 0.0);
                        e.child.Add(x);
                        conList.Add(x);
                    }
                }

                if (endCount <= 0) return ret;
            }
        }

        public void DrawTree(HierarchicalRandomLocalSearchElement origin, int nest)
        {
            string head = "?";

            if (nest == 1) head = "R";
            if (nest == 2) head = "H";
            if (nest == 3) head = "M";
            if (nest == 4) head = "L";
            if (nest == 5) head = "C";

            for (int i = 0; i < nest; i++) Console.Write("    ");
            //Console.WriteLine(head + "[" + origin.s.Value + "," + origin.best.Value + "," + origin.loop + "," + origin.pool.Element().Count() + "," + origin.pool.Variance().ToString("F2") + "]");
            Console.WriteLine(head + "[" + origin.s.Value + "," + origin.best.Value + "," + origin.loop + "," + origin.pool.Element().Count() + "," + origin.calc + "]");

            foreach (var c in origin.child)
            {
                DrawTree(c, nest + 1);
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

        private IEnumerable<HierarchicalRandomLocalSearchElement> High(HierarchicalRandomLocalSearchElement root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                yield return root.child[i];
            }
        }

        private IEnumerable<HierarchicalRandomLocalSearchElement> Mid(HierarchicalRandomLocalSearchElement root)
        {
            for (int i = 0; i < root.child.Count(); i++)
            {
                for (int j = 0; j < root.child[i].child.Count(); j++)
                {
                    yield return root.child[i].child[j];
                }
            }
        }

        private IEnumerable<HierarchicalRandomLocalSearchElement> Low(HierarchicalRandomLocalSearchElement root)
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
        
        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",htrj=" + htrj + ",mtrj=" + mtrj + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + ",range=" + range + "]";
        }

        public HierarchicalRandomLocalSearch0322(long limit, int htrj, int mtrj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
        {
            this.limit = limit;
            this.htrj = htrj;
            this.mtrj = mtrj;
            this.targetHighAct = targetHighAct;
            this.targetMidAct = targetMidAct;
            this.targetLowAct = targetLowAct;
            this.range = range;
        }
    }
}
