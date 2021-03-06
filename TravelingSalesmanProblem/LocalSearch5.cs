﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class LocalSearch5 : IOptimizationMethod
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
            MinimumKeeper mk = new MinimumKeeper();
            return t.TwoOpt(
                n.Where(op => mk.IsMinimum(tsp.TwoOptValue(op, t)))
                .Aggregate((ac, op) => op));
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

    }
}
