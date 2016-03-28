using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drace.OptimizationLibrary;

namespace Drace.QuadraticAssignmentProblem
{
    class QapOperation : IOperation
    {
        public int i;
        public int j;

        public QapOperation(int i, int j)
        {
            this.i = i;
            this.j = j;
        }
    }
}
