using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP {
    class TabuSearch : IOptimizationMethod {
        private FlowShopSchedulingProblem fssp;
        private int loopMax;
        private int tabuLength;

        public ISolution Solve(FlowShopSchedulingProblem fssp, DataStoringWriter w)
        {
            return this.Solve(fssp, FsspSolution.Random(fssp, fssp.NumberOfItems), w);
        }

        public ISolution Solve(FlowShopSchedulingProblem fssp, ISolution s, DataStoringWriter w) {
            this.fssp = fssp;
            TabuList tabuList = new TabuList(fssp, tabuLength);
            FsspSolution x = (FsspSolution)s.Clone();

            for (int loop = 0; loop < loopMax; loop++) {
                w.WriteLine(loop + ":" + fssp.Evaluate(x) + ":" + x.ToString());
                x = Move(x, tabuList, loop);
            }

            w.WriteLine(loopMax + ":" + fssp.Evaluate(x) + ":" + x.ToString());
                
            return x;
        }

        private FsspSolution Move(FsspSolution x, TabuList tabuList, int loop) {
            int op1 = -1;
            int op2 = -1;
            int bestValue = int.MaxValue;
            int tmpValue;

            for (int i = 0; i < x.Size - 2; i++) {
                for (int j = i + 1; j < x.Size - 1; j++) {
                    if (tabuList.IsNotTabu(i, j, loop)) {
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
            }

            FsspSolution next = (FsspSolution)x.Clone();
            next.Swap(op1, op2);
            tabuList.AddList(op1, op2, loop);

            return next;
        }

        public override string ToString() { 
            return "TabuSearch[loop=" + loopMax + ",tl=" + tabuLength + "]";
        }

        public TabuSearch(int loopMax, int tabuLength) {
            this.loopMax = loopMax;
            this.tabuLength = tabuLength;
        }
    }
}
