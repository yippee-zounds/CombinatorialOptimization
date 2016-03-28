using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class LocalSearch1 : IOptimizationMethod
    {
        private TravelingSalesmanProblem tsp;

        public ArrayTour Solve(TravelingSalesmanProblem tsp, DataStoringWriter w)
        {
            return this.Solve(tsp, ArrayTour.Random(this.tsp), w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ISolution s, DataStoringWriter w)
        {
            ArrayTour t = (ArrayTour)s;
            this.tsp = tsp;

            while (true)
            {
                ArrayTour tmp = BestImprovement(t);
                w.WriteLine(tmp.TourLength.ToString());
                if (t.TourLength <= tmp.TourLength)
                    return t;

                t = tmp;
            }
        }

        private ArrayTour BestImprovement(ArrayTour t)
        {
            int neighborBest = int.MaxValue;
            int op_i = -1;
            int op_j = -1;

            for (int i = 1; i < t.Length - 1; i++)
            {
                for (int j = i + 1; j < t.Length; j++)
                {
                    int tmp = tsp.TwoOptValue(t[i - 1], t[i], t[j], t[j + 1]);
                    if (tmp <= neighborBest)
                    {
                        neighborBest = tmp;
                        op_i = i;
                        op_j = j;
                    }
                }
            }

            return t.TwoOpt(op_i, op_j);
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}
