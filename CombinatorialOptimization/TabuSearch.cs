using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class TabuSearch : IOptimizationAlgorithm
    {
        private int loopMax;
        private int tabuLength;

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
            ISolution ret = s.Clone();
            TabuList tabuList = new TabuList(this.tabuLength);
            ISolution lopt = new LocalSearch().Solve(p, sol);
            IOptimizationAlgorithm ls = new LocalSearch();
            List<ISolution> ss = new List<ISolution>();

            //w.WriteLine("loop:vx:doptx:dloptx:doptlopt:x");
            w.WriteLine("loop:vbest:vx:vdom:doptx:dloptx:doptlopt:ddomx:doptdom:dloptdom:doptbest:dloptbest:ddombest:dbestx:act:x");
            
            for (int loop = 0; loop < loopMax; loop++)
            {
                ss.Add(s.Clone());
                if (10 < ss.Count()) ss.RemoveAt(0);
                double currentAct = calculateActivity(ss);

                w.WriteLine(Trace(loop, p.Optimum, lopt, ls.Solve(p, s), currentAct, ret, s));
                
                IOperation bestOp = p.OperationSet()
                    .Where((op) => tabuList.IsNotTabu(op, loop))
                    .ArgMinStrict((op) => p.OperationValue(op, s));
                int bestOpValue = p.OperationValue(bestOp, s);
                ISolution tmp = s.Apply(bestOp);

                if (mk.IsMinimumStrict(tmp.Value))
                    ret = tmp.Clone();

                s = tmp;
                tabuList.Add(bestOp, loop);
            }
            w.WriteLine(Trace(loopMax, p.Optimum, lopt, ls.Solve(p, s), 0, ret, s));
            w.WriteLine(Trace(loopMax, p.Optimum, lopt, ls.Solve(p, ret), 0, ret, ret));
            //w.WriteLine(loopMax + ":" + s.Value + ":" + lopt.DistanceTo(s) + ":" + p.Optimum.DistanceTo(s) + ":" + s.ToString());
            //w.WriteLine(loopMax + ":" + ret.Value + ":" + lopt.DistanceTo(ret) + ":" + p.Optimum.DistanceTo(ret) + ":" + ret.ToString());
                
            return ret;
        }

        private double calculateActivity(List<ISolution> ss)
        {
            int d10 = 0;
            for (int i = 0; i < ss.Count - 1; i++) d10 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));

            return (double)ss.First().DistanceTo(ss.Last()) / d10;
        }

        private string Trace(int loop, ISolution opt, ISolution lopt, ISolution dom, double act, ISolution best, ISolution x)
        {
            return loop + ":" + best.Value + ":" + x.Value + ":" + dom.Value + ":" + opt.DistanceTo(x) + ":" + lopt.DistanceTo(x) + ":" + opt.DistanceTo(lopt) + ":" + dom.DistanceTo(x) + ":" + opt.DistanceTo(dom) + ":" + lopt.DistanceTo(dom) + ":" + opt.DistanceTo(best) + ":" + lopt.DistanceTo(best) + ":" + dom.DistanceTo(best) + ":" + x.DistanceTo(best) + ":" + act + ":" + x;
        }
        
        private string Trace(int loop, ISolution x, ISolution opt, ISolution lopt)
        {
            return loop + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + lopt.DistanceTo(x) + ":" + opt.DistanceTo(lopt) + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[loop=" + loopMax + ",tl=" + tabuLength + "]";
        }

        public TabuSearch(int loopMax, int tabuLength)
        {
            this.loopMax = loopMax;
            this.tabuLength = tabuLength;
        }
    }
}
