using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class MultipleRandomLocalSearch : IOptimizationAlgorithm
    {
        private int sizeOfSubset;
        private int initialPoint;
        private int calcMax;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            int subBlock = calcMax * 2 / initialPoint / 10;
            ISolution ret = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();
            var rls = new RandomLocalSearch(subBlock / sizeOfSubset, sizeOfSubset, 0);
            List<ISolution> s = new List<ISolution>();
            
            for(int i = 0; i < this.initialPoint; i++)
            {
                s.Add(p.CreateRandomSolution());
            }

            w.WriteLine("loop:r:vx:doptx:x");

            for (int calcTotal = 0; calcTotal < calcMax; )
            {
                for (int i = 0; i < s.Count(); i++)
                {
                    s[i] = rls.Solve(p, s[i]);
                    calcTotal += subBlock;
                }
                    
                if(2 <= s.Count())
                    s.Remove(s.ArgMinStrict((x) => -x.Value));
            }

            return s.First();
        }

        public MultipleRandomLocalSearch(int initialPoint, int totalCalc, int sizeOfSubset)
        {
            this.sizeOfSubset = sizeOfSubset;
            this.calcMax = totalCalc;

            if (0 < initialPoint)
            {
                this.initialPoint = initialPoint;
            }
            else
            {
                this.initialPoint = 1;
            }
        }
    }
}
