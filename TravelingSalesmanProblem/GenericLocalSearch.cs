using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class GenericLocalSearch
    {
        public ArrayTour Solve(TravelingSalesmanProblem tsp, Func<int, ArrayTour, ArrayTour, bool> condition, Predicate<TspOperation> filter, DataStoringWriter w)
        {
            return Solve(tsp, ArrayTour.Random(tsp), condition, filter, w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ArrayTour t, Func<int, ArrayTour, ArrayTour, bool> condition , Predicate<TspOperation> filter, DataStoringWriter w)
        {
            return this.Solve(tsp, t, condition, tsp.TwoOptNeighborhood(t), filter, w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ArrayTour t, Func<int, ArrayTour, ArrayTour, bool> condition, IEnumerable<TspOperation> neighbor, Predicate<TspOperation> filter, DataStoringWriter w)
        {
            MinimumKeeper mk = new MinimumKeeper();
            ArrayTour ret = t;

            for (int loop = 0; condition(loop, t, ret); loop++)
            {
                ArrayTour tmp = BestImprovement(tsp, t, neighbor.Where(op => filter(op)));
                w.WriteLine(loop + " : " + tmp.TourLength.ToString());
                if (mk.IsMinimumStrict(tmp.TourLength))
                    ret = tmp;

                t = tmp;
            }

            return ret;
        }

        private ArrayTour BestImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            MinimumKeeper mk = new MinimumKeeper();
            return t.TwoOpt(n.Last(op => mk.IsMinimum(tsp.TwoOptValue(op, t))));
        }

        private ArrayTour FirstImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            MinimumKeeper mk = new MinimumKeeper();
            return t.TwoOpt(n.First(op => mk.IsMinimum(tsp.TwoOptValue(op, t))));
        }
    }
}
