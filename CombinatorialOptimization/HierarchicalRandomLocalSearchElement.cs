using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalRandomLocalSearchElement : IMetric
    {
        private IOptimizationProblem p;
        public List<ISolution> ss;
        public List<HierarchicalRandomLocalSearchElement> child = new List<HierarchicalRandomLocalSearchElement>();
        public double targetAct;
        public int sizeOfSubset;
        private int targetRange;
        public int loop = 0;
        public int bestLoop = 0;
        public int rangeBestLoop = 0;
        public int highLoop = 0;
        public int highBestLoop = 0;
        private int subLoop = 0;
        private int sizeOfNeighborhood;
        public ISolution s;
        public ISolution best;
        public ISolution highBest;
        private List<int> hist = new List<int>();
        private int id = 0;
        public MetricSpace pool = new MetricSpace(100);
        public bool removeFlag = false;
        public bool checkedFlag = false;
        public long calc = 0;

        public int Id { get => id;}

        public int DistanceTo(IMetric m)
        {
            HierarchicalRandomLocalSearchElement e = (HierarchicalRandomLocalSearchElement)m;

            return this.best.DistanceTo(e.best);
        }

        public double CalcAct()
        {
            if (ss.Count() < 2) return 0.0;

            int dd = 20;
            ISolution s1 = ss.ElementAt(Math.Max(ss.Count() - 1, 0));
            ISolution s20 = ss.ElementAt(Math.Max(ss.Count() - dd, 0));
            int d20 = 0;

            for (int i = 1; i < Math.Min(ss.Count - 1, dd); i++)
            {
                d20 += ss.ElementAt(ss.Count() - i).DistanceTo(ss.ElementAt(ss.Count() - i - 1));
            }

            if (d20 == 0) return 0.0;
            else return (double)s1.DistanceTo(s20) / d20;
        }

        public long Search()
        {
            long ret = 0;

            foreach (var c in this.child)
            {
                ret += c.Search();
            }

            ss.Add(s.Clone());
            if (100 < ss.Count())
            {
                ss.RemoveAt(0);
            }

            hist.Add(s.Value);
            if (targetRange < hist.Count())
            {
                hist.RemoveAt(0);
            }

            int dd = 20;
            int d = 0;
            for (int i = 1; i < Math.Min(ss.Count - 1, dd); i++)
            {
                d += ss.ElementAt(ss.Count() - i).DistanceTo(ss.ElementAt(ss.Count() - i - 1));
            }
            double act = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - dd, 0))) / d;

            if (1000 < highLoop)
            {
                if (s.Value < highBest.Value)
                {
                    highBest = s.Clone();
                    highBestLoop = 0;
                }
            }

            if (10000 < highLoop) highBest = s.Clone();
            
            if (0 <= subLoop)
            {
                if (act < targetAct)
                {
                    sizeOfSubset = Math.Max(sizeOfSubset - (int)(sizeOfNeighborhood * 0.01), 1);
                    subLoop = Math.Min(-20, sizeOfSubset);
                }
                else
                {
                    sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.01), sizeOfNeighborhood);
                    subLoop = Math.Min(-20, sizeOfSubset);
                }
            }
            if (!s.IsFeasible())
            {
                sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
            }

            ++loop;
            ++subLoop;
            ++bestLoop;
            ++rangeBestLoop;
            ++highBestLoop;
            ++highLoop;

            IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
            int delta = p.OperationValue(bestOp, s);
            ISolution tmp = s.Apply(bestOp);

            if (s.Value < hist.Skip(hist.Count() - targetRange).Min((x) => x))
            {
                rangeBestLoop = 0;
            }

            if (0 <= loop && s.Value < best.Value)
            {
                best = s.Clone();
                bestLoop = 0;
            }

            s = tmp;

            calc += sizeOfSubset;

            return ret + sizeOfSubset;
        }

        public HierarchicalRandomLocalSearchElement(HierarchicalRandomLocalSearchElement e, ISolution s, double targetAct, int id)
        {
            this.id = id;
            this.p = e.p;
            this.s = s.Clone();
            this.best = p.CreateRandomSolution();
            this.highBest = this.best;
            this.targetAct = targetAct;
            this.sizeOfSubset = e.sizeOfSubset;
            this.targetRange = e.targetRange;
            this.sizeOfNeighborhood = e.sizeOfNeighborhood;
            this.ss = new List<ISolution>();
            foreach (var x in e.ss)
            {
                this.ss.Add(x.Clone());
            }
        }

        public HierarchicalRandomLocalSearchElement(HierarchicalRandomLocalSearchElement e, double targetAct, int id, int loop)
        {
            this.id = id;
            this.loop = loop;
            this.p = e.p;
            this.s = e.s.Clone();
            //this.s = e.best.Clone();
            this.best = p.CreateRandomSolution();
            this.highBest = this.best;
            this.targetAct = targetAct;
            this.sizeOfSubset = e.sizeOfSubset;
            this.targetRange = e.targetRange;
            this.sizeOfNeighborhood = e.sizeOfNeighborhood;
            this.ss = new List<ISolution>();
            foreach (var s in e.ss)
            {
                this.ss.Add(s.Clone());
            }
        }

        public HierarchicalRandomLocalSearchElement(HierarchicalRandomLocalSearchElement e, double targetAct, int id)
        {
            this.id = id;
            this.p = e.p;
            this.s = e.best.Clone();
            this.best = e.best.Clone();
            this.highBest = e.best.Clone();
            this.targetAct = targetAct;
            this.sizeOfSubset = e.sizeOfSubset;
            this.targetRange = e.targetRange;
            this.sizeOfNeighborhood = e.sizeOfNeighborhood;
            this.ss = new List<ISolution>();
            foreach (var s in e.ss)
            {
                this.ss.Add(s.Clone());
            }
        }

        public HierarchicalRandomLocalSearchElement(HierarchicalRandomLocalSearchElement e, double targetAct)
        {
            this.p = e.p;
            this.s = e.s.Clone();
            this.best = e.s.Clone();
            this.highBest = e.s.Clone();
            this.targetAct = targetAct;
            this.sizeOfSubset = e.sizeOfSubset;
            this.targetRange = e.targetRange;
            this.sizeOfNeighborhood = e.sizeOfNeighborhood;
            this.ss = new List<ISolution>();
            foreach (var x in e.ss)
            {
                this.ss.Add(s.Clone());
            }
        }

        public HierarchicalRandomLocalSearchElement(HierarchicalRandomLocalSearchElement e, ISolution s, double targetAct)
        {
            this.p = e.p;
            this.s = s.Clone();
            this.best = s.Clone();
            this.highBest = s.Clone();
            this.targetAct = targetAct;
            this.sizeOfSubset = e.sizeOfSubset;
            this.targetRange = e.targetRange;
            this.sizeOfNeighborhood = e.sizeOfNeighborhood;
            this.ss = new List<ISolution>();
            foreach (var x in e.ss)
            {
                this.ss.Add(s.Clone());
            }
        }

        public HierarchicalRandomLocalSearchElement(IOptimizationProblem p, ISolution s, double targetAct, int initialSizeOfSubset, int range)
        {
            this.p = p;
            this.s = s.Clone();
            this.best = s.Clone();
            this.highBest = s.Clone();
            this.targetAct = targetAct;
            this.sizeOfSubset = initialSizeOfSubset;
            this.targetRange = range;
            this.sizeOfNeighborhood = p.OperationSet().Count();
            this.ss = new List<ISolution>();
        }
    }
}
