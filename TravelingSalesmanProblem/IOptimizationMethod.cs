using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public interface IOptimizationMethod
    {
        ArrayTour Solve(TravelingSalesmanProblem tsp, ISolution s, DataStoringWriter w);
    }
}
