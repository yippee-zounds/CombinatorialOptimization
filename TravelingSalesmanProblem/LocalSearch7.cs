using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class LocalSearch7 : IOptimizationMethod
    {
        public ArrayTour Solve(TravelingSalesmanProblem tsp, DataStoringWriter w)
        {
            return Solve(tsp, tsp.CreateRandomSolution(), w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ISolution s, DataStoringWriter w)
        {
            ArrayTour t = (ArrayTour)s;
            MinimumKeeper mk = new MinimumKeeper();

            while (true)
            {
                ArrayTour tmp = t.TwoOpt
                    (
                        tsp.TwoOptNeighborhood(t).ArgMinStrict
                        (
                            (op) => tsp.TwoOptValue(op, t)
                        )
                    );
                w.WriteLine(tmp.TourLength.ToString());
                if (mk.IsNotMinimumStrict(tmp.TourLength))
                    return t;

                t = tmp;
            }
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

    }
}
