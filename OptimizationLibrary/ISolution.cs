using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public interface ISolution : IMetric
    {
        ISolution Apply(IOperation op);
        ISolution ReverseApply(IOperation op);
        ISolution CloneApply(IOperation op);
        int Value {get; }
        int DistanceTo(IMetric m);
        ISolution Clone();

        int OperationDistance(IOperation op, ISolution[] s);

        bool IsFeasible();
    }
}
