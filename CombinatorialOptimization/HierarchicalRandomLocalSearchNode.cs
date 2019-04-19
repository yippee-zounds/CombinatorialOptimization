using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalRandomLocalSearchNode
    {
        public IOptimizationProblem p;
        public ISolution s;
        public double targetAct;
        public int actLoop;
        public int bestLoop;
        public int loop;
        public int sizeOfSubset;
        public List<ISolution> ss;
        public List<int> sv;
        public ISolution best;
        public ISolution worst;
        public List<HierarchicalRandomLocalSearchNode> child;
        public bool isRangeBest = false;
        public bool isRangeBestOld = false;
        public int range;

        public int GetBestLoopDeep()
        {
            int ret = bestLoop;

            for (int i = 0; i < child.Count; i++)
            {
                if (child[i].GetBestLoopDeep() < ret)
                {
                    ret = child[i].GetBestLoopDeep();
                }
            }

            return ret;
        }
        public ISolution GetBest()
        {
            ISolution ret = best.Clone();

            for (int i = 0; i < child.Count; i++)
            {
                if (child[i].GetBest().Value < ret.Value)
                {
                    ret = child[i].best.Clone();
                }
            }

            return ret;
        }
        public ISolution GetBestDeep()
        {
            ISolution ret = best.Clone();

            for (int i = 0; i < child.Count; i++)
            {
                if (child[i].GetBest().Value < ret.Value)
                {
                    ret = child[i].GetBest();
                }
            }

            return ret;
        }

        public void AddList()
        {
            ss.Add(s.Clone());
            if (20 < ss.Count()) ss.RemoveAt(0);

            if (s.Value < best.Value)
            {
                best = s.Clone();
                bestLoop = 0;
            }

            if (worst.Value < s.Value)
            {
                worst = s.Clone();
            }

            sv.Add(s.Value);
            if (range < sv.Count()) sv.RemoveAt(0);
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

            for (int i = 0; i < child.Count(); i++)
            {
                ret += child[i].Search();
            }

            int sizeOfNeighborhood = p.OperationSet().Count();

            //活性度の調整
            if (20 <= actLoop)
            {
                if (CalcAct() < targetAct)
                {
                    actLoop = 0;
                    sizeOfSubset = Math.Max(sizeOfSubset - Math.Max((int)(sizeOfNeighborhood * 0.005), 1), 1);
                }
                else
                {
                    actLoop = 0;
                    sizeOfSubset = Math.Min(sizeOfSubset + Math.Max((int)(sizeOfNeighborhood * 0.005), 1), sizeOfNeighborhood);
                }
            }

            //局所的な探索
            IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
            int delta = p.OperationValue(bestOp, s);
            AddList();
            isRangeBestOld = isRangeBest;
            isRangeBest = (range <= sv.Count() && sv.Last() == sv.Min());
            ret += sizeOfSubset;
            loop++;
            actLoop++;
            bestLoop++;
            s = s.Clone().Apply(bestOp);

            return ret;
        }

        public HierarchicalRandomLocalSearchNode(IOptimizationProblem p, ISolution s, double targetAct, int initialSizeOfSubset, int range)
        {
            this.p = p;
            this.s = s.Clone();
            this.targetAct = targetAct;
            this.loop = 0;
            this.actLoop = 0;
            this.bestLoop = 0;
            this.sizeOfSubset = initialSizeOfSubset;
            this.ss = new List<ISolution>();
            this.best = s.Clone();
            this.worst = s.Clone();
            this.child = new List<HierarchicalRandomLocalSearchNode>();
            this.sv = new List<int>();
            this.range = range;
        }
    }
}
