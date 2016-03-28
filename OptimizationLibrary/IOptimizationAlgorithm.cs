using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public interface IOptimizationAlgorithm
    {
        ISolution Solve(IOptimizationProblem p, DataStoringWriter w);
        ISolution Solve(IOptimizationProblem p, ISolution s, DataStoringWriter w);
        ISolution Solve(IOptimizationProblem p, ISolution s);
    }
}
