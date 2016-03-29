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
        private int hashCode = -1;

        public TwoOptOperation(int size, int i, int j, ArrayTour t)
        {
            this.i = i;
            this.j = j;
            this.size = size;

            int bi = branch(size, t[i - 1], t[i]);
            int bj = branch(size, t[j], t[j + 1]);
            this.hashCode = size * size * Math.Min(bi, bj) + Math.Max(bi, bj);
        }

        private int branch(int size, int m, int n)
        {
            return size * Math.Min(m, n) + Math.Max(m, n);
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public override string ToString()
        {
            return "{" + i + "," + j + "}";
        }
    }
}
