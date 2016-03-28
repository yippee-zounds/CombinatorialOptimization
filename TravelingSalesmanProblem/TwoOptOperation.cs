using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class TwoOptOperation : IOperation
    {
        public int size = -1;
        public int i;
        public int j;

        public TwoOptOperation(int i, int j)
        {
            this.i = i;
            this.j = j;
        }

        public TwoOptOperation(int size, int i, int j)
        {
            this.i = i;
            this.j = j;
            this.size = size;
        }

        public override int GetHashCode()
        {
            return i * size + j;
        }

        public override string ToString()
        {
            return "{" + i + "," + j + "}";
        }
    }
}
