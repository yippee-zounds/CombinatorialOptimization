using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class ConstraintProblem: IOptimizationProblem
    {
        IOptimizationProblem p;
        ISolution origin;
        int distance;

        public int Size
        {
            get { return p.Size; }
        }

        public ISolution Optimum
        {
            get
            {
                return p.Optimum;
            }
        }

        public ISolution CreateRandomSolution()
        {
            return p.CreateRandomSolution();
        }

        public IEnumerable<IOperation> OperationSet()
        {
            return p.OperationSet();
        }

        public int OperationValue(IOperation op, ISolution s)
        {
            return p.OperationValue(op, s);
        }

        public int Evaluate(ISolution s)
        {
            if (s.DistanceTo(origin) <= distance) return p.Evaluate(s);
            else return int.MaxValue;
        }

        public string Name
        {
            get
            {
                return "Constraint(" + p.Name + ")";
            }
        }

        public ConstraintProblem(IOptimizationProblem p, int distance, ISolution origin)
        {
            this.p = p;
            this.origin = origin;
            this.distance = distance;
        }
    }
}
