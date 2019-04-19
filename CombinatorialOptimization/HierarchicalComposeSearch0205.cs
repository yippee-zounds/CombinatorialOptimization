using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalComposeSearch0205 : IOptimizationAlgorithm
    {
        private long limit;
        private int ctrj;
        private int dtrj;
        private double targetHighAct;
        private double targetMidAct;
        private double targetLowAct;
        private int sizeOfNeighborhood;
        private int range;
        private int choice = 3;
        private int trial = 1;
        private int initSize = 20;

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
            int setSize = initSize;

            //初期集合の構成
            Console.WriteLine("initialize");
            w.WriteLine("initialize");

            for (int i = 0; i < initSize; i++)
            {
                var x = new HierarchicalRandomLocalSearchElement(root, p.CreateRandomSolution(), targetHighAct);

                for (int loop = 0; loop < 1000; loop++)
                {
                    endCount -= x.Search();
                }
                Console.WriteLine("    search " + i + " : " + x.best.Value + " : " + endCount);
                w.WriteLine("    search " + i + " : " + x.best.Value + " : " + endCount);

                highSs.Add(x);
            }

            double v = new MetricSpace(highSs).Variance();
            Console.WriteLine("    base_variance : " + v.ToString("F1"));
            w.WriteLine("    base_variance : " + v.ToString("F1"));

            for (int loop = 0; true; loop++, bestLoop++)
            {
                Console.WriteLine(loop + " : "
                    + "( h=" + highSs.Count().ToString("D2") + "(" + (0 < highSs.Count() ? highSs.Average((n) => n.best.Value): 0).ToString("F1") + "," + (new MetricSpace(highSs).Variance() / v).ToString("F3") + ")"
                    + ", m=" + midSs.Count().ToString("D2") + "(" + (0 < midSs.Count() ? midSs.Average((n) => n.best.Value): 0).ToString("F1") + "," + (new MetricSpace(midSs).Variance() / v).ToString("F3") + ")"
                    + ", l=" + lowSs.Count().ToString("D2") + "(" + (0 < lowSs.Count() ? lowSs.Average((n) => n.best.Value): 0).ToString("F1") + "," + (new MetricSpace(lowSs).Variance() / v).ToString("F3") + ")" + " )");
                w.WriteLine(loop + " : "
                    + "( h=" + highSs.Count().ToString("D2") + "(" + (0 < highSs.Count() ? highSs.Average((n) => n.best.Value) : 0).ToString("F1") + "," + (0 < highSs.Count() ? highSs.StandardDeviation((n) => n.best.Value) : 0).ToString("F1") + "," + (new MetricSpace(highSs).Variance() / v).ToString("F3") + ")"
                    + ", m=" + midSs.Count().ToString("D2") + "(" + (0 < midSs.Count() ? midSs.Average((n) => n.best.Value) : 0).ToString("F1") + "," + (0 < midSs.Count() ? midSs.StandardDeviation((n) => n.best.Value) : 0).ToString("F1") + "," + (new MetricSpace(midSs).Variance() / v).ToString("F3") + ")"
                    + ", l=" + lowSs.Count().ToString("D2") + "(" + (0 < lowSs.Count() ? lowSs.Average((n) => n.best.Value) : 0).ToString("F1") + "," + (0 < lowSs.Count() ? lowSs.StandardDeviation((n) => n.best.Value) : 0).ToString("F1") + "," + (new MetricSpace(lowSs).Variance() / v).ToString("F3") + ")" + " )");

                //////////////////////////コンポーズ//////////////////////////
                var midCmp = new List<HierarchicalRandomLocalSearchElement>();
                var lowCmp = new List<HierarchicalRandomLocalSearchElement>();
                var conCmp = new List<HierarchicalRandomLocalSearchElement>();

                Console.Write("  : compose   ( ");
                w.Write("  : compose   ( ");

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
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetMidAct, sizeOfSubset, range);
                        midSs.Add(x);
                        midCmp.Add(x);
                        x.checkedFlag = true;
                    }
                }
                Console.Write("m=" + midCmp.Count().ToString("D2") + ", ");
                w.Write("m=" + midCmp.Count().ToString("D2") + ", ");
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
                        var x = new HierarchicalRandomLocalSearchElement(p, s, targetLowAct, sizeOfSubset, range);
                        lowSs.Add(x);
                        lowCmp.Add(x);
                        x.checkedFlag = true;
                    }
                }
                Console.Write("l=" + lowCmp.Count().ToString("D2") + "       ) ");
                w.Write("l=" + lowCmp.Count().ToString("D2") + "       ) ");
                endCount -= SearchLocalMinimum(lowCmp, range / 10);

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
                w.WriteLine(best.Value + ":" + bestDom.Value + ":" + endCount);

                //////////////////////////デコンポーズ//////////////////////////
                var highDec = new List<HierarchicalRandomLocalSearchElement>();
                var midDec  = new List<HierarchicalRandomLocalSearchElement>();
                var lowDec  = new List<HierarchicalRandomLocalSearchElement>();

                Console.Write("  : decompose ( ");
                w.Write("  : decompose ( ");
                Console.Write("l=" + lowDec.Count().ToString("D2") + ", ");
                w.Write("l=" + lowDec.Count().ToString("D2") + ", ");
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
                w.Write("m=" + midCmp.Count().ToString("D2") + ", ");
                endCount -= Search(midDec, range);

                //////Midレベル→Highレベル
                if (0 < midCmp.Where((n) => range <= n.loop).Count())
                {
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
                w.Write("h=" + highDec.Count().ToString("D2") + " ) ");
                endCount -= Search(highDec, range);

                //最良解の更新
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
                w.WriteLine(best.Value + ":" + bestDom.Value + ":" + endCount + "\n");

                //冗長な要素の削除
                Shrink(highSs, setSize);
                Shrink(midSs, setSize);
                Shrink(lowSs, setSize);
                Shrink(conSs, setSize);

                //後処理
                if (endCount <= 0) return best;
                Console.WriteLine();
                w.WriteLine("");
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

        public HierarchicalComposeSearch0205(long limit, int ctrj, int dtrj, double targetHighAct, double targetMidAct, double targetLowAct, int range)
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
