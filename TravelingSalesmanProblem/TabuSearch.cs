using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class TabuSearch : IOptimizationMethod
    {
        private int loopMax; 
        private int tabuLength;

        public ArrayTour Solve(TravelingSalesmanProblem tsp, DataStoringWriter w)
        {
            return Solve(tsp, ArrayTour.Random(tsp), w);
        }

        public ArrayTour Solve(TravelingSalesmanProblem tsp, ISolution s, DataStoringWriter w)
        {
            ArrayTour t = (ArrayTour)s;
            MinimumKeeper mk = new MinimumKeeper();
            ArrayTour ret = null;
            TabuList tabuList = new TabuList(this.tabuLength);

            for(int loop = 0; loop < loopMax; loop++)
            {
                TspOperation op = BestImprovement(tsp, t, tsp.TwoOptNeighborhood(t).Where(x => tabuList.IsNotTabu(x)));
                tabuList.Add(op, loop);
                ArrayTour tmp = t.TwoOpt(op);
                w.WriteLine(tmp.TourLength.ToString());
                if (mk.IsMinimumStrict(tmp.TourLength))
                    ret = tmp;

                t = tmp;
            }

            return ret;
        }

        private TspOperation BestImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            MinimumKeeper mk = new MinimumKeeper();
            return n.Last(op => mk.IsMinimum(tsp.TwoOptValue(op, t)));
        }

        private ArrayTour FirstImprovement(TravelingSalesmanProblem tsp, ArrayTour t, IEnumerable<TspOperation> n)
        {
            MinimumKeeper mk = new MinimumKeeper();
            return t.TwoOpt(n.First(op => mk.IsMinimum(tsp.TwoOptValue(op, t))));
        }

        public TabuSearch(int loopMax, int tabuLength)
        {
            this.loopMax = loopMax;
            this.tabuLength = tabuLength;
        }
    }
}
