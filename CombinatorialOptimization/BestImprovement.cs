using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class BestImprovement
    {
        public ISolution Move(ISolution solution, Object def)
        {
            throw new NotImplementedException();
            //ISolution tmp = solution.Neighborhood().ArgMinStrict((s) => s.Value);

            //return tmp;
        }
    }
}
