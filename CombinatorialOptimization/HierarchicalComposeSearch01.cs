using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalComposeSearch01 : IOptimizationAlgorithm
    {
        private long limit;
        private int ctrj;
        private int dtrj;
        private double targetHighAct;
        private double targetMidAct;
        private double targetLowAct;
        private double targetConAct = 0.0;
        private int sizeOfNeighborhood;
        private int range;
        private int choice = 5;

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
            ISolution best = sol.Clone();
            HierarchicalRandomLocalSearchElement root = new HierarchicalRandomLocalSearchElement(p, sol.Clone(), 1.0, 1, range);
            long endCount = limit;
            int bestLoop = 0;
            ISolution bestDom = null;
            sizeOfNeighborhood = p.OperationSet().Count();
            List<HierarchicalRandomLocalSearchElement> highSs = new List<HierarchicalRandomLocalSearchElement>();
            List<HierarchicalRandomLocalSearchElement> midSs = new List<HierarchicalRandomLocalSearchElement>();
            List<HierarchicalRandomLocalSearchElement> lowSs = new List<HierarchicalRandomLocalSearchElement>();
            List<HierarchicalRandomLocalSearchElement> conSs = new List<HierarchicalRandomLocalSearchElement>();
            
            w.WriteLine("loop:vbest:vhigh:vmid:vlow:nhigh:nmid:nlow:lowin:lowex:midin:midex:x");

            //初期集合の構成
            Console.WriteLine("initialize");

            for (int i = 0; i < 30; i++)
            {
                var x = new HierarchicalRandomLocalSearchElement(root, p.CreateRandomSolution(), targetHighAct);

                Console.WriteLine("    search " + i + " : " + endCount);
                for (int loop = 0; loop < range; loop++)
                {
                    endCount -= x.Search();
                }

                highSs.Add(x);
            }


            for (int loop = 0; true; loop++, bestLoop++)
            {
                Console.WriteLine(loop + " : "
                    + "( h=" + highSs.Count().ToString("D2") + "(" + new MetricSpace(highSs).Variance().ToString("F1") + ")"
                    + ", m=" + midSs.Count().ToString("D2") + "(" + new MetricSpace(midSs).Variance().ToString("F1") + ")"
                    + ", l=" + lowSs.Count().ToString("D2") + "(" + new MetricSpace(lowSs).Variance().ToString("F1") + ")"
                    + ", c=" + conSs.Count().ToString("D2") + "(" + new MetricSpace(conSs).Variance().ToString("F1") + ")" + " )");

                //////////////////////////コンポーズ//////////////////////////
                var midCmp = new List<HierarchicalRandomLocalSearchElement>();
                var lowCmp = new List<HierarchicalRandomLocalSearchElement>();
                var conCmp = new List<HierarchicalRandomLocalSearchElement>();

                Console.Write("  : compose   ( ");

                //////Highレベル→Midレベル
                if (choice <= highSs.Where((n) => range <= n.loop).Count())
                {
                    for(int i = 0; i < ctrj; i++)
                    {
                        var subset = highSs.Where((n) => range <= n.loop).RandomSubset(choice);
                        ISolution s = GetCenterSolution(p, subset.Select((n) => n.s));
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetMidAct, subset.Max((n) => n.sizeOfSubset), range);
                        midSs.Add(x);
                        midCmp.Add(x);
                        x.checkedFlag = true;
                    }
                }
                Console.Write("m=" + midCmp.Count().ToString("D2") + ", ");
                endCount -= Search(midCmp, range);

                //////Midレベル→Lowレベル
                if (choice <= midSs.Where((n) => range <= n.loop).Count())
                {
                    for (int i = 0; i < ctrj; i++)
                    {
                        var subset = midSs.Where((n) => range <= n.loop).RandomSubset(choice);
                        ISolution s = GetCenterSolution(p, subset.Select((n) => n.s));
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetLowAct, subset.Max((n) => n.sizeOfSubset), range);
                        lowSs.Add(x);
                        lowCmp.Add(x);
                        x.checkedFlag = true;
                    }
                }
                Console.Write("l=" + lowCmp.Count().ToString("D2") + ", ");
                endCount -= SearchLocalMinimum(lowCmp, range / 10);

                //////Lowレベル→Conレベル
                if (choice <= lowSs.Where((n) => range / 10 <= n.loop).Count())
                {
                    for (int i = 0; i < ctrj; i++)
                    {
                        var subset = lowSs.Where((n) => range / 10 <= n.loop).RandomSubset(choice);
                        ISolution s = GetCenterSolution(p, subset.Select((n) => n.s));
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetConAct, subset.Max((n) => n.sizeOfSubset), range);
                        conSs.Add(x);
                        conCmp.Add(x);
                        x.checkedFlag = true;
                    }
                }
                Console.Write("c=" + conCmp.Count().ToString("D2") + " ) ");
                endCount -= SearchLocalMinimum(conCmp, 1);

                //最良解の更新
                if (0 < conSs.Count() && conSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = conSs.ArgMin((n) => n.best.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < lowSs.Count() && lowSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = lowSs.ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < midSs.Count() && midSs.Min((n) => n.s.Value) < best.Value)
                {
                    best = midSs.ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < highSs.Count() && highSs.Min((n) => n.s.Value) < best.Value)
                {
                    best = highSs.ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }

                Console.WriteLine(best.Value + ":" + bestDom.Value + ":" + endCount);

                //////////////////////////デコンポーズ//////////////////////////
                var highDec = new List<HierarchicalRandomLocalSearchElement>();
                var midDec  = new List<HierarchicalRandomLocalSearchElement>();
                var lowDec  = new List<HierarchicalRandomLocalSearchElement>();

                Console.Write("  : decompose ( ");

                //////Conレベル→Lowレベル
                if (0 < conCmp.Where((n) => 0 <= n.bestLoop).Count())
                {
                    foreach (var e in conCmp.Where((n) => 0 <= n.bestLoop))
                    {
                        for (int i = 0; i < dtrj; i++)
                        {
                            var x = new HierarchicalRandomLocalSearchElement(e, e.best, targetLowAct);
                            lowDec.Add(x);
                            lowSs.Add(x);
                            x.checkedFlag = true;
                        }
                        e.removeFlag = true;
                    }
                }
                Console.Write("l=" + lowDec.Count().ToString("D2") + ", ");
                endCount -= Search(lowDec, 100);

                //////Lowレベル→Midレベル
                if (0 < lowCmp.Where((n) => range / 10 <= n.loop).Count())
                {
                    foreach (var e in lowCmp.Where((n) => range / 10 <= n.loop))
                    {
                        for (int i = 0; i < dtrj; i++)
                        {
                            var x = new HierarchicalRandomLocalSearchElement(e, e.best, targetMidAct);
                            midDec.Add(x);
                            midSs.Add(x);
                            x.checkedFlag = true;
                        }
                        e.removeFlag = true;
                    }
                }
                Console.Write("m=" + midCmp.Count().ToString("D2") + ", ");
                endCount -= Search(midDec, range);

                //////Midレベル→Highレベル
                if (0 < midCmp.Where((n) => range <= n.loop).Count())
                {
                    //Console.WriteLine("!");
                    foreach (var e in midCmp.Where((n) => range <= n.loop))
                    {
                        for (int i = 0; i < dtrj; i++)
                        {
                            var x = new HierarchicalRandomLocalSearchElement(e, e.best, targetHighAct);
                            highDec.Add(x);
                            highSs.Add(x);
                            x.checkedFlag = true;
                        }
                        e.removeFlag = true;
                    }
                }
                Console.Write("h=" + highDec.Count().ToString("D2") + " ) ");
                endCount -= Search(highDec, range);

                //最良解の更新
                if (0 < conSs.Count() && conSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = conSs.ArgMin((n) => n.best.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < lowSs.Count() && lowSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = lowSs.ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < midSs.Count() && midSs.Min((n) => n.s.Value) < best.Value)
                {
                    best = midSs.ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < highSs.Count() && highSs.Min((n) => n.s.Value) < best.Value)
                {
                    best = highSs.ArgMin((n) => n.s.Value).s.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }

                Console.WriteLine(best.Value + ":" + bestDom.Value + ":" + endCount + "\n");

                //不要な要素の削除
                RemoveElement(highSs);
                RemoveElement(midSs);
                RemoveElement(lowSs);
                RemoveElement(conSs);

                //後処理
                if (endCount <= 0) return best;
                Console.WriteLine();
            }
        }

        public long Search(List<HierarchicalRandomLocalSearchElement> ss, int loopMax)
        {
            long ret = 0;

            //Console.WriteLine("    search");

            foreach (var e in ss)
            {
                for (int loop = 0; loop < loopMax; loop++)
                {
                    ret += e.Search();
                }
            }

            return ret;
        }

        public long SearchLocalMinimum(List<HierarchicalRandomLocalSearchElement> ss, int bestLoopMax)
        {
            long ret = 0;

            //Console.WriteLine("    search_local_minimum");

            foreach (var e in ss)
            {
                for (int loop = 0; e.bestLoop < bestLoopMax; loop++)
                {
                    ret += e.Search();
                }
            }

            return ret;
        }

        public void RemoveElement(List<HierarchicalRandomLocalSearchElement> ss)
        {
            ss.RemoveAll((n) => n.removeFlag);
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

        private ISolution GetCenterSolution(IOptimizationProblem p, IEnumerable<ISolution> nodes)
        {
            int sizeOfSubset = p.OperationSet().Count();
            ISolution s = p.CreateRandomSolution();
            ISolution tmp = null;
            ISolution[] ss = nodes.ToArray();

            while (true)
            {
                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => s.OperationDistance(op, ss));
                tmp = s.Clone().Apply(bestOp);

                if (0 <= s.OperationDistance(bestOp, ss)) return s;

                s = tmp;
            }
        }

        /*
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
        }/**/
        
        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",ctrj=" + ctrj + ",dtrj=" + dtrj + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + ",range=" + range + "]";
        }

        public HierarchicalComposeSearch01(long limit, int ctrj, int dtrj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
        {
            this.limit = limit;
            this.ctrj = ctrj;
            this.dtrj = dtrj;
            this.targetHighAct = targetHighAct;
            this.targetMidAct = targetMidAct;
            this.targetLowAct = targetLowAct;
            this.range = range;
        }
    }
}
