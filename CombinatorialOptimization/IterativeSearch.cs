using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class IterativeSearch : IOptimizationAlgorithm
    {
        IOptimizationAlgorithm a;
        int itr;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            ISolution best = null;

            for (int i = 0; i < itr; i++)
            {
                ISolution s = a.Solve(p, p.CreateRandomSolution(), w);

                if (s.Value < best.Value)
                    best = s;
            }

            return new LocalSearch().Solve(p, best, w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution best = sol.Clone();

            for(int i = 0; i < itr; i++)
            {
                ISolution s = a.Solve(p, sol.Clone(), w);

                if (s.Value < best.Value)
                    best = s;
            }

            return best;
        }
        public override string ToString()
        {
            return this.GetType().Name + "[a=" + a + ",itr=" + itr + "]";
        }

        public IterativeSearch(IOptimizationAlgorithm a, int itr)
        {
            this.a = a;
            this.itr = itr;
        }
    }
}
