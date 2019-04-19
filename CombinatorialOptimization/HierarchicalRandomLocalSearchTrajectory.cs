using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HierarchicalRandomLocalSearchTrajectory
    {
        public IOptimizationProblem p;
        public ISolution s;
        public double targetAct;
        public int actLoop;
        public int bestLoop;
        public int sizeOfSubset;
        public List<ISolution> ss;
        public List<ISolution> best;

        public void AddList()
        {
            ss.Add(s.Clone());
            if (10 < ss.Count()) ss.RemoveAt(0);

            if (s.Value < best.Min((y) => y.Value))
            {
                best.Add(s.Clone());
                bestLoop = 0;
            }
        }

        public double CalcAct()
        {
            if (ss.Count() < 2) return 0.0;

            ISolution s1 = ss.ElementAt(Math.Max(ss.Count() - 1, 0));
            ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10, 0));
            int d10 = 0;

            for (int i = 1; i < Math.Min(ss.Count - 1, 10); i++)
            {
                d10 += ss.ElementAt(ss.Count() - i).DistanceTo(ss.ElementAt(ss.Count() - i - 1));
            }
            return (double)s1.DistanceTo(s10) / d10;
        }

        public ISolution Search()
        {
            int sizeOfNeighborhood = p.OperationSet().Count();

            //活性度の調整
            if (10 <= actLoop)
            {
                if (CalcAct() < targetAct)
                {
                    actLoop = 0;
                    sizeOfSubset = Math.Max(sizeOfSubset - Math.Max((int)(sizeOfNeighborhood * 0.01), 1), 1);
                }
                else
                {
                    sizeOfSubset = Math.Min(sizeOfSubset + Math.Max((int)(sizeOfNeighborhood * 0.01), 1), sizeOfNeighborhood);
                    actLoop = 0;
                }
            }

            //局所的な探索
            IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
            int delta = p.OperationValue(bestOp, s);
            AddList();
            actLoop++;
            bestLoop++;
            return s.Apply(bestOp);
        }

        public HierarchicalRandomLocalSearchTrajectory(IOptimizationProblem p, ISolution s, double targetAct, int initialSizeOfSubset)
        {
            this.p = p;
            this.s = s.Clone();
            this.targetAct = targetAct;
            this.actLoop = 0;
            this.bestLoop = 0;
            this.sizeOfSubset = initialSizeOfSubset;
            this.ss = new List<ISolution>();
            this.best = new List<ISolution>();
            this.best.Add(s.Clone());
        }
    }
}
