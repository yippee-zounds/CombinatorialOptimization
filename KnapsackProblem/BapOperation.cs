using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drace.OptimizationLibrary;

namespace Drace.KnapsackProblem
{
    class BapOperation : IOperation
    {
        public int i;
        public int j;

        public BapOperation(int i) : this(i, -1) { }

        public BapOperation(int i, int j)
        {
            this.i = i;
            this.j = j;
        }
    }
}
