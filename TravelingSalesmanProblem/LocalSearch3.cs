using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class LocalSearch3 : IOptimizationMethod
    {
        public ArrayTour Solve(TravelingSalesmanProblem tsp, DataStoringWriter w)
        {
            return Solve(tsp, ArrayTour.Random(tsp), w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ISolution s, DataStoringWriter w)
        {
            ArrayTour t = (ArrayTour)s;
            while (true)
            {
                ArrayTour tmp = BestImprovement(tsp, t, tsp.TwoOptNeighborhood(t));
                w.WriteLine(tmp.TourLength.ToString());
                if (t.TourLength <= tmp.TourLength)
                    return t;

                t = tmp;
            }
        }

        private ArrayTour BestImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            MinimumKeeper mk = new MinimumKeeper();
            TspOperation op = null;

            foreach (TspOperation p in n)
            {
                if (mk.IsMinimum(tsp.TwoOptValue(p, t)))
                {
                    op = p;
                }
            }

            ArrayTour ret = t.TwoOpt(op);
            return ret;
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

    }
}
