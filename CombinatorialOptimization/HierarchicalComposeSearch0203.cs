using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalComposeSearch0203 : IOptimizationAlgorithm
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
        private int trial = 1;
        private int initSize = 10;

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

            for (int i = 0; i < initSize; i++)
            {
                var x = new HierarchicalRandomLocalSearchElement(root, p.CreateRandomSolution(), targetHighAct);

                for (int loop = 0; loop < 1000; loop++)
                {
                    endCount -= x.Search();
                }
                Console.WriteLine("    search " + i + " : " + x.best.Value + " : " + endCount);

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
                        ISolution s = null;
                        int sizeOfSubset = 0;

                        for (int j = 0; j < trial; j++)
                        {
                            var subset = highSs.Where((n) => range <= n.loop).RandomSubset(choice);
                            ISolution tmp = GetCenterSolution(p, subset.Select((n) => n.best));
                            sizeOfSubset = Math.Max(sizeOfSubset, (subset.Max((n) => n.sizeOfSubset)));
                            if(s == null || tmp.Value < s.Value)
                            {
                                s = tmp;
                            }
                        }
                        //Console.Write("[" + s.Value + "]");
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetMidAct, sizeOfSubset, range);
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
                        ISolution s = null;
                        int sizeOfSubset = 0;

                        for (int j = 0; j < trial; j++)
                        {
                            var subset = midSs.Where((n) => range <= n.loop).RandomSubset(choice);
                            ISolution tmp = GetCenterSolution(p, subset.Select((n) => n.best));
                            sizeOfSubset = Math.Max(sizeOfSubset, (subset.Max((n) => n.sizeOfSubset)));
                            if (s == null || tmp.Value < s.Value)
                            {
                                s = tmp;
                            }
                        }
                        //Console.Write("[" + s.Value + "]");
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetLowAct, sizeOfSubset, range);
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
                        ISolution s = null;
                        int sizeOfSubset = 0;

                        for (int j = 0; j < trial; j++)
                        {
                            var subset = lowSs.Where((n) => range / 10 <= n.loop).RandomSubset(choice);
                            ISolution tmp = GetCenterSolution(p, subset.Select((n) => n.best));
                            sizeOfSubset = Math.Max(sizeOfSubset, (subset.Max((n) => n.sizeOfSubset)));
                            if (s == null || tmp.Value < s.Value)
                            {
                                s = tmp;
                            }
                        }
                        //Console.Write("[" + s.Value + "]");
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetConAct, sizeOfSubset, range);
                        conSs.Add(x);
                        conCmp.Add(x);
                        x.checkedFlag = true;
                    }
                }
                Console.Write("c=" + conCmp.Count().ToString("D2") + " ) ");
                //endCount -= SearchLocalMinimum(conCmp, 1);/**/

                //最良解の更新
                /*if (0 < conSs.Count() && conSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = conSs.ArgMin((n) => n.best.Value).best.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }*/
                if (0 < lowSs.Count() && lowSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = lowSs.ArgMin((n) => n.best.Value).best.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < midSs.Count() && midSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = midSs.ArgMin((n) => n.best.Value).best.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < highSs.Count() && highSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = highSs.ArgMin((n) => n.best.Value).best.Clone();
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
                    best = conSs.ArgMin((n) => n.best.Value).best.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < lowSs.Count() && lowSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = lowSs.ArgMin((n) => n.best.Value).best.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < midSs.Count() && midSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = midSs.ArgMin((n) => n.best.Value).best.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }
                if (0 < highSs.Count() && highSs.Min((n) => n.best.Value) < best.Value)
                {
                    best = highSs.ArgMin((n) => n.best.Value).best.Clone();
                    bestLoop = 0;
                    bestDom = new LocalSearch().Solve(p, best);
                }

                Console.WriteLine(best.Value + ":" + bestDom.Value + ":" + endCount + "\n");

                //不要な要素の削除
                /*
                highSs.RemoveAll((n) => n.removeFlag);
                midSs.RemoveAll((n) => n.removeFlag);
                lowSs.RemoveAll((n) => n.removeFlag);
                conSs.RemoveAll((n) => n.removeFlag);/**/

                Shrink(highSs, initSize);
                Shrink(midSs, initSize);
                Shrink(lowSs, initSize);
                Shrink(conSs, initSize);

                //後処理
                if (endCount <= 0) return best;
                Console.WriteLine();
            }
        }

        public void Shrink(List<HierarchicalRandomLocalSearchElement> ss, int targetSize)
        {
            while(targetSize < ss.Count())
            {
                var e = ss.ArgMax((n) => n.best.Value);
                ss.Remove(e);
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

        private ISolution GetCenterSolution2(IOptimizationProblem p, IEnumerable<ISolution> nodes)
        {
            return (ISolution)new MetricSpace(nodes).Center().Item1;
        }

        private ISolution GetCenterSolution(IOptimizationProblem p, IEnumerable<ISolution> nodes)
        {
            int sizeOfSubset = p.OperationSet().Count();
            ISolution s = p.CreateRandomSolution();
            ISolution tmp = null;
            ISolution[] ss = nodes.ToArray();

            for (int loop = 0; loop < 1000; loop++)
            {
                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => s.OperationDistance(op, ss));
                tmp = s.Clone().Apply(bestOp);

                if (0 <= s.OperationDistance(bestOp, ss)) return s;

                s = tmp;
            }

            return s;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",ctrj=" + ctrj + ",dtrj=" + dtrj + ",hact=" + targetHighAct + ",mact=" + targetMidAct + ",lact=" + targetLowAct + ",range=" + range + "]";
        }

        public HierarchicalComposeSearch0203(long limit, int ctrj, int dtrj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
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
