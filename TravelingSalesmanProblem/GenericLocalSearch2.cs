using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class GenericLocalSearch2
    {
        /*
        public ArrayTour Solve(TravelingSalesmanProblem tsp, Func<int, ArrayTour, ArrayTour, bool> condition, Predicate<Operation> filter, DataStoringWriter w)
        {
            return Solve(tsp, ArrayTour.Random(tsp), condition, filter, w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ISolution t, Func<int, ISolution, ArrayTour, bool> condition, Predicate<Operation> filter, DataStoringWriter w)
        {
            return this.Solve(tsp, t, condition, tsp.TwoOptNeighborhood(t), filter, w);
        }
        */

        public ISolution Solve(TravelingSalesmanProblem tsp, ISolution t, Func<int, ISolution, ISolution, bool> condition, IEnumerable<TspOperation> neighbor, Predicate<TspOperation> filter, DataStoringWriter w)
        {
            MinimumKeeper mk = new MinimumKeeper();
            ISolution ret = t;

            for (int loop = 0; condition(loop, t, ret); loop++)
            {
                ISolution tmp = BestImprovement(tsp, t, neighbor.Where(op => filter(op)));
                w.WriteLine(loop + " : " + tmp.Value.ToString());
                if (mk.IsMinimumStrict(tmp.Value))
                    ret = tmp;
                
                t = tmp;
            }

            return ret;
        }

        private ISolution BestImprovement(TravelingSalesmanProblem tsp, ISolution t, IEnumerable<TspOperation> n)
        {
            return t.Apply((IOperation)n.ArgMinStrict((x) => tsp.TwoOptValue(x, t)));
        }

        private ArrayTour FirstImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            MinimumKeeper mk = new MinimumKeeper();
            return t.TwoOpt(n.First(op => mk.IsMinimum(tsp.TwoOptValue(op, t))));
        }
    }
}
