using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class HigherLevelSearch2 : IOptimizationAlgorithm
    {
        private IOptimizationProblem p;
        private int loopMax;
        private int regionSize;
        private List<ISolution> localMinimum;
        private bool isLocalMode;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
           return this.Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s, DataStoringWriter w) {
            this.p = p;
            ISolution x = (ISolution)s.Clone();
            localMinimum = new List<ISolution>();
            isLocalMode = true;
            MinimumKeeper mk = new MinimumKeeper();
            ISolution ret = x.Clone();
            ISolution lopt = new LocalSearch().Solve(p, s);
            IOptimizationAlgorithm ls = new LocalSearch();

            //w.WriteLine("loop:vx:doptx:dloptx:doptlopt:x");
            w.WriteLine("loop:vbest:vx:vdom:doptx:dloptx:doptlopt:ddomx:doptdom:dloptdom:doptbest:dloptbest:ddombest:dbestx:x");

            for (int loop = 0; loop < loopMax; loop++)
            {
                w.WriteLine(Trace(loop, p.Optimum, lopt, ls.Solve(p, x), ret, x));
                if (isLocalMode) {
                    int oldValue = x.Value;
                    ISolution tmp = BestImprovement(x);
                    if (oldValue <= tmp.Value) {
                        //解が改悪されていたら、モードを切替える
                        localMinimum.Add((ISolution)x.Clone());
                        isLocalMode = false;
                    }
                    else {
                        x = tmp;
                    }
                }
                else {
                    ISolution tmp = Move2(x);

                    if (tmp.Value < x.Value) {
                        isLocalMode = true;
                    }
                    x = tmp;
                }

                if (mk.IsMinimumStrict(x.Value))
                    ret = x.Clone();
            }

            w.WriteLine(Trace(loopMax, p.Optimum, lopt, ls.Solve(p, ret), ret, ret));
            return ret;
        }

        private string Trace(int loop, ISolution opt, ISolution lopt, ISolution dom, ISolution best, ISolution x)
        {
            return loop + ":" + best.Value + ":" + x.Value + ":" + dom.Value + ":" + opt.DistanceTo(x) + ":" + lopt.DistanceTo(x) + ":" + opt.DistanceTo(lopt) + ":" + dom.DistanceTo(x) + ":" + opt.DistanceTo(dom) + ":" + lopt.DistanceTo(dom) + ":" + opt.DistanceTo(best) + ":" + lopt.DistanceTo(best) + ":" + dom.DistanceTo(best) + ":" + x.DistanceTo(best) + ":" + x;
        }

        private string Trace(int loop, ISolution opt, ISolution lopt, ISolution x)
        {
            return loop + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + lopt.DistanceTo(x) + ":" + opt.DistanceTo(lopt) + ":" + x;
        }

        private bool IsNull(Diameter diam) {
            return diam.Solution1 == null || diam.Solution2 == null;
        }

        private bool IsFarther(ISolution common, ISolution older, ISolution newer){
            return common.DistanceTo(older) < common.DistanceTo(newer);
        }

        private ISolution Move2(ISolution x)
        {
            int stockValue = int.MaxValue;
            IOperation stockOperation = null;
            int bestValue = int.MaxValue;
            int bestOperationValue = int.MaxValue;
            IOperation bestOperation = null;
            int bestValueBackup = int.MaxValue;
            IOperation bestOperationBackup = null;
            ISolution last = localMinimum.Last();
            int lastDistance = last.DistanceTo(x);
            Diameter diam = Diameter.CalculateDiameter(localMinimum, regionSize);
            int count = 0;
            int bestCount = 0;
            int bestBackupCount = 0;

            foreach (var op in p.OperationSet().Where((o) => p.OperationValue(o, x) <= bestOperationValue))
            {
                //System.Console.Write(p.OperationValue(op, x) + "\t" + bestOperationValue);

                int diamDistanceSumX = 0;
                if(!IsNull(diam)) diamDistanceSumX = diam.DistanceSum(x);
                int xValue = x.Value;
                ISolution tmp = x.Apply(op);
                int tmpValue = tmp.Value;
                int tmpDistance = tmp.DistanceTo(last);

                stockValue = tmpValue;
                stockOperation = op;
                count++;

                     
                if (tmpValue < bestValue && lastDistance < tmpDistance)
                {
                    if (IsNull(diam) || diamDistanceSumX < diam.DistanceSum(tmp))
                    {
                        bestOperationValue = tmpValue - xValue;
                        bestValue = tmpValue;
                        bestOperation = op;
                        bestCount++;
                    }
                }

                if (tmpValue < bestValueBackup && lastDistance <= tmpDistance)
                {
                    bestValueBackup = tmpValue;
                    bestOperationBackup = op;
                    bestBackupCount++;
                }

                x.ReverseApply(op);
                //System.Console.WriteLine("\t->\t" + bestOperationValue + "\t" + count + "\t" + bestCount + "\t" + bestBackupCount);

            }
            if (bestOperation == null)
            {
                bestOperation = bestOperationBackup;
            }
            return ((ISolution)x.Clone()).Apply(bestOperation);
        }

        private ISolution Move(ISolution x, int loop) {
            ISolution s = x.Clone();
            int bestValue = int.MaxValue;
            IOperation bestOperation = null;

            int bestValueBackup = int.MaxValue;
            IOperation bestOperationBackup = null;

            ISolution last = localMinimum.Last();
            int lastDistance = last.DistanceTo(x);
            Diameter diam = Diameter.CalculateDiameter(localMinimum, regionSize);

            foreach (var op in p.OperationSet())
            {
                ISolution tmp = x.CloneApply(op);
                int tmpValue = tmp.Value;
                int tmpDistance = tmp.DistanceTo(last);

                if (tmpValue < bestValue && lastDistance < tmpDistance)
                {
                    if (IsNull(diam) || diam.DistanceSum(x) < diam.DistanceSum(tmp))
                    {
                        bestValue = tmpValue;
                        bestOperation = op;
                    }
                }

                if (tmpValue < bestValueBackup && lastDistance <= tmpDistance)
                {
                    bestValueBackup = tmpValue;
                    bestOperationBackup = op;
                }
            }
            if (bestOperation == null) {
                bestOperation = bestOperationBackup;
            }
            ISolution next = ((ISolution)x.Clone()).Apply(bestOperation);

            return next;
        }

        private ISolution BestImprovement(ISolution s) {
            IOperation bestOp = p.OperationSet().ArgMinStrict((op) => p.OperationValue(op, s));
            return s.Apply(bestOp);
        }

        public override string ToString() {
            return GetType().Name + "[loop=" + loopMax + ",rs=" + regionSize + "]";
        }

        public HigherLevelSearch2(int loopMax, int regionSize) {
            this.loopMax = loopMax;
            this.regionSize = regionSize;
        }
    }
}
