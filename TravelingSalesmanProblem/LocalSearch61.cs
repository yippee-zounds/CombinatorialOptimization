using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class LocalSearch61 : IOptimizationMethod
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
            int min = int.MaxValue;
            return t.TwoOpt(n.Last(op => 
                {
                    int tmp = tsp.TwoOptValue(op, t);
                    if (tmp <= min)
                    {
                        min = tmp;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                ));
        }

        private ArrayTour FirstImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            MinimumKeeper mk = new MinimumKeeper();
            return t.TwoOpt(n.First(op => mk.IsMinimum(tsp.TwoOptValue(op, t))));
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

    }
}
