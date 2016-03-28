using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public interface IOptimizationProblem
    {
        int Size { get; }
        ISolution Optimum { get; }
        ISolution CreateRandomSolution();
        IEnumerable<IOperation> OperationSet();
        int Evaluate(ISolution s); 
        int OperationValue(IOperation op, ISolution s);
        string Name { get; }
    }
}
