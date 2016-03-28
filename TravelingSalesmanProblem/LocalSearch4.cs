using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class LocalSearch4 : IOptimizationMethod
    {
        public ArrayTour Solve(TravelingSalesmanProblem tsp, DataStoringWriter w)
        {
            return Solve(tsp, ArrayTour.Random(tsp), w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ISolution s, DataStoringWriter w)
        {
            ArrayTour t = (ArrayTour)s;
            MinimumKeeper mk = new MinimumKeeper();

            while (true)
            {
                ArrayTour tmp = BestImprovement(tsp, t, tsp.TwoOptNeighborhood(t));
                w.WriteLine(tmp.TourLength.ToString());
                if (mk.IsNotMinimumStrict(tmp.TourLength))
                    return t;

                t = tmp;
            }
        }

        private ArrayTour BestImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            return t.TwoOpt(
                n.Aggregate(
                        new {mk = new MinimumKeeper(), op = new TspOperation(-1, -1)},
                        (ac, op) => ac.mk.IsMinimum(tsp.TwoOptValue(op, t)) ? new {mk = ac.mk, op} : ac,
                        (ac)     => ac.op
                    )
                );
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

    }
}
