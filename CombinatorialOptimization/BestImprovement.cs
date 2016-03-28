using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class BestImprovement
    {
        public ISolution Move(ISolution solution, NeighborhoodDefinition def)
        {
            ISolution tmp = solution.Neighborhood(def).ArgMinStrict((s) => s.Value);

            return tmp;
        }
    }
}
