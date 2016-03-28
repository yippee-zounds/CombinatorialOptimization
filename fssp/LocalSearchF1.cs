using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP {
    public class LocalSearchF1 : IOptimizationMethod{
        private FlowShopSchedulingProblem fssp;

        FsspSolution GetInitialSolution(int size) {
            return FsspSolution.Random(fssp, size);
        }

        public ISolution Solve(FlowShopSchedulingProblem fssp, DataStoringWriter w)
        {
            return this.Solve(fssp, GetInitialSolution(fssp.NumberOfItems), w);
        }

        public ISolution Solve(FlowShopSchedulingProblem fssp, ISolution sol, DataStoringWriter w) {
            this.fssp = fssp;
            FsspSolution x = (FsspSolution)sol.Clone();
            int loop = 0;

            while (true) {
                w.WriteLine(loop + ":" + fssp.Evaluate(x) + ":" + x.ToString());
                FsspSolution tmp = Move(x);
                if (fssp.Evaluate(tmp) < fssp.Evaluate(x)) {
                    x = tmp;
                }
                else {
                    return x;
                }
                loop++;
            }

        }

        FsspSolution Move(FsspSolution x) {
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

        public LocalSearchF1()
        {
        }
    }
}
