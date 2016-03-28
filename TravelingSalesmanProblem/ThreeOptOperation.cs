using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    class ThreeOptOperation : IOperation
    {
        public int size = -1;
        public int i;
        public int j;
        public int k;
        public int inin;

        public ThreeOptOperation(int i, int j, int k, int inin)
        {
            this.i = i;
            this.j = j;
            this.k = k;
            this.inin = inin;
        }

        public ThreeOptOperation(int size, int i, int j, int k, int inin)
        {
            this.i = i;
            this.j = j;
            this.k = k;
            this.size = size;
            this.inin = inin;
        }

        public override int GetHashCode()
        {
            return ((i * size + j) * size + k) * 2 + inin;
        }

        public override string ToString()
        {
            return "{" + i + "," + j + "," + k + "," + inin + "}";
        }
    }
}
