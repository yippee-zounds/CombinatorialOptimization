using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class LocalSearch : IOptimizationAlgorithm
    {
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
            ISolution s = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();

            w.WriteLine("loop:vx:doptx:dom:x");

            for(int loop = 0; true; loop++)
            {
                //double plus = p.OperationSet().Where((op) => p.OperationValue(op, s) >= 0).Count();
                //double minus = p.OperationSet().Where((op) => p.OperationValue(op, s) < 0).Count();
                //w.WriteLine(Trace(loop, p.Optimum, s));

                IOperation bestOp = p.OperationSet().ArgMinStrict((op) => p.OperationValue(op, s));
                //int val = p.OperationValue(bestOp, s);
                ISolution tmp = s.Apply(bestOp);

                if (mk.IsNotMinimumStrict(tmp.Value))
                {
                    w.WriteLine(Trace(loop, p.Optimum, s, sol));
                    return s;
                }
                s = tmp;
            }
        }

        public ISolution SolveByStep(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution s = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();
            BestImprovement m = new BestImprovement();

            w.WriteLine("loop:vx:doptx:dom:x");

            for (int loop = 0; true; loop++)
            {
                ISolution tmp = m.Move(s, def);

                if (mk.IsNotMinimumStrict(tmp.Value))
                {
                    w.WriteLine(Trace(loop, p.Optimum, s, sol));
                    return s;
                }
                s = tmp;
            }
        }

        public ISolution Solve(IOptimizationProblem p, IOperationSet ops, ISolution sol, DataStoringWriter w)
        {
            ISolution s = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();

            w.WriteLine("loop:vx:doptx:x");

            for (int loop = 0; true; loop++)
            {
                w.WriteLine(Trace(loop, p.Optimum, s));

                IOperation bestOp = ops.ArgMinStrict((op) => p.OperationValue(op, s));
                ISolution tmp = s.Apply(bestOp);

                if (mk.IsNotMinimumStrict(tmp.Value))
                    return s;

                s = tmp;
            }
        }

        private string Trace(int loop, ISolution opt, ISolution x)
        {
            return loop + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, ISolution opt, ISolution dom, ISolution x)
        {
            return loop + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom + ":" + x ;
        }
        
        private string Trace(int loop, double ratio, ISolution opt, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

    }
}