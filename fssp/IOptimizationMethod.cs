using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP {
    public interface IOptimizationMethod {
        ISolution Solve(FlowShopSchedulingProblem fssp, DataStoringWriter w);
        ISolution Solve(FlowShopSchedulingProblem fssp, ISolution s, DataStoringWriter w);
    }
}
