using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP {
    class HigherLevelSearch : IOptimizationMethod{
        private FlowShopSchedulingProblem fssp;
        private int loopMax;
        private int regionSize;
        private List<FsspSolution> localMinimum;
        private bool isLocalMode;

        public ISolution Solve(FlowShopSchedulingProblem fssp, DataStoringWriter w)
        {
           return this.Solve(fssp, FsspSolution.Random(fssp, fssp.NumberOfItems), w);
        }

        public ISolution Solve(FlowShopSchedulingProblem fssp, ISolution s, DataStoringWriter w) {
            this.fssp = fssp;
            FsspSolution x = (FsspSolution)s;
            localMinimum = new List<FsspSolution>();
            isLocalMode = true;

            for (int loop = 0; loop < loopMax; loop++) {
                w.WriteLine(loop + ":" + fssp.Evaluate(x) + ":" + x.ToString());
                if (isLocalMode) {
                    FsspSolution tmp = BestImprovement(x);
                    if (fssp.Evaluate(x) <= fssp.Evaluate(tmp)) {
                        //解が改悪されていたら、モードを切替える
                        localMinimum.Add((FsspSolution)x.Clone());
                        isLocalMode = false;
                    }
                    else {
                        x = tmp;
                    }
                }
                else {
                    FsspSolution tmp = Move(x, loop);

                    if (fssp.Evaluate(tmp) < fssp.Evaluate(x)) {
                        isLocalMode = true;
                    }
                    x = tmp;
                }
            }

            w.WriteLine(loopMax + ":" + fssp.Evaluate(x) + ":" + x.ToString());
            return x;
        }

        private bool IsNull(Diameter diam) {
            return diam.Solution1 == null || diam.Solution2 == null;
        }

        private bool IsFarther(FsspSolution common, FsspSolution older, FsspSolution newer){
            return common.DistanceTo(older) < common.DistanceTo(newer);
        }
 
        private FsspSolution Move(FsspSolution x, int loop) {
            int op1 = -1;
            int op2 = -1;
            int bestValue = int.MaxValue;

            int op1Backup = -1;
            int op2Backup = -1;
            int bestValueBackup = int.MaxValue;

            FsspSolution last = localMinimum.Last();
            int lastDistance = last.DistanceTo(x);
            Diameter diam = Diameter.CalculateDiameter(localMinimum, regionSize);

            for (int i = 0; i < x.Size - 1; i++) {
                for (int j = i + 1; j < x.Size; j++) {
                    FsspSolution tmp = ((FsspSolution)x.Clone()).Swap(i, j);
                    int tmpValue = fssp.Evaluate(tmp);
                    int tmpDistance = tmp.DistanceTo(last);

                    if (tmpValue < bestValue) {
                        if (lastDistance < tmpDistance) {
                            if (IsNull(diam) || diam.DistanceSum(x) < diam.DistanceSum(tmp)) {
                                bestValue = tmpValue;
                                op1 = i;
                                op2 = j;
                            }
                        }
                    }

                    if (tmpValue < bestValueBackup) {
                        if (lastDistance < tmpDistance) {
                            bestValueBackup = tmpValue;
                            op1Backup = i;
                            op2Backup = j;
                        }
                    }

                    /*
                    if (tmpValue < bestValue) {
                        if(lastDistance < tmpDistance){
                            if (IsNull(diam) || (IsFarther(diam.Solution1, x, tmp) && IsFarther(diam.Solution2, x, tmp))) {
                                bestValue = tmpValue;
                                op1 = i;
                                op2 = j;
                            }
                        }
                    }

                    if (tmpValue < bestValueBackup) {
                        if (lastDistance < tmpDistance) {
                            if (IsNull(diam) || (IsFarther(diam.Solution1, x, tmp) || IsFarther(diam.Solution2, x, tmp))) {
                                bestValueBackup = tmpValue;
                                op1Backup = i;
                                op2Backup = j;
                            }
                        }
                    } */
                }
            }

            /*
            if (op1Backup < 0 || op2Backup < 0) {
                return null;
            }
            */

            if (op1 < 0 || op2 < 0) {
                op1 = op1Backup;
                op2 = op2Backup;
            }
            FsspSolution next = ((FsspSolution)x.Clone()).Swap(op1, op2);

            return next;
        }

        private FsspSolution BestImprovement(FsspSolution x) {
            int op1 = -1;
            int op2 = -1;
            int bestValue = int.MaxValue;
            int tmpValue;

            for (int i = 0; i < x.Size - 1; i++) {
                for (int j = i + 1; j < x.Size; j++) {
                    x.Swap(i, j);
                    tmpValue = fssp.Evaluate(x);
                    x.Swap(i, j);

                    if (tmpValue < bestValue) {
                        bestValue = tmpValue;
                        op1 = i;
                        op2 = j;
                    }
                }
            }

            FsspSolution next = (FsspSolution)x.Clone();
            next.Swap(op1, op2);

            return next;
        }

        public override string ToString() {
            return "HigherLevelSearch[loop=" + loopMax + ",rs=" + regionSize + "]";
        }

        public HigherLevelSearch(int loopMax, int regionSize) {
            this.loopMax = loopMax;
            this.regionSize = regionSize;
        }
    }
}
