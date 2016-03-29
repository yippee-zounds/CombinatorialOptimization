using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization.Algorithm
{
    class LocalSearch : IAlgorithm
    {
        ISolution IAlgorithm.solve(IOptimizationProblem p, ISolution init, DataStoringWriter w)
        {
            ISolution x = init.Clone();

            w.WriteLine("loopCount:xValue");

            for (int loopCount = 0; true; ++loopCount) {
                w.WriteLine($"{loopCount}:{x.Value}");

                IOperation bestOp = calculateBestOperation(p, x);
                ISolution tmp = x.CloneApply(bestOp);

                if (tmp.Value < x.Value) x = tmp;
                else return x;
            }
        }

        protected virtual IOperation calculateBestOperation(IOptimizationProblem p, ISolution x)
        {
            return p.OperationSet(x).ArgMinStrict((op) => p.OperationValue(op, x));
        }
    }
}
