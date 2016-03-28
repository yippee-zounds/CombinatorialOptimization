using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP
{
    class FsspOperation : IOperation
    {
        public int i;
        public int j;
        public int size = -1;

        public FsspOperation(int i, int j) {
            this.i = i;
            this.j = j;
        }

        public FsspOperation(int size, int i, int j)
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
