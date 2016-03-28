using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    abstract class Tour : ISolution
    {
        public Boolean IsFeasible()
        {
            return true;
        }
        public int OperationDistance(IOperation op, ISolution[] s)
        {
            throw new NotImplementedException();
        }

        public abstract ISolution Apply(IOperation op);
        public abstract ISolution ReverseApply(IOperation op);
        public abstract ISolution CloneApply(IOperation op);
        public int DistanceTo(ISolution s)
        {
            return -1;
        }
        public abstract int Value{get;}

        public abstract ISolution Clone();
    }
}
