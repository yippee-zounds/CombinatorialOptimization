using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public interface IAlgorithm
    {
        ISolution solve(IOptimizationProblem p, ISolution init, DataStoringWriter w);
    }
}
