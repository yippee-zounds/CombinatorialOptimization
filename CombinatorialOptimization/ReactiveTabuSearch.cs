using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class ReactiveTabuSearch : IOptimizationAlgorithm
    {
        private long loopMax;
        private int tabuLength;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        private double calcCorrelation(ISolution origin, ISolution sa, ISolution sb)
        {
            double dist_oa = origin.DistanceTo(sa);
            double dist_ob = origin.DistanceTo(sb);
            double dist_ab = sa.DistanceTo(sb);
            double ret = (dist_oa * dist_oa + dist_ob * dist_ob - dist_ab * dist_ab) / (2.0 * dist_oa * dist_ob);

            if (double.IsNaN(ret))
            {
                return 0.0;
            }
            else
            {
                return ret;
            }
        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution s = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();
            ISolution ret = s.Clone();
            VariableTabuList tabuList = new VariableTabuList(this.tabuLength);
            ISolution lopt = new LocalSearch().Solve(p, sol);
            IOptimizationAlgorithm ls = new LocalSearch();
            List<ISolution> doms = new List<ISolution>();
            List<ISolution> ss = new List<ISolution>();
            var slist = new List<ReactiveTabuSearchItem>();
            int maxRepetition = 50;
            int maxCycle = 50;
            int chaotic = 0;
            int maxChaos = 10;
            bool escape = false;
            int loopFromLastChange = 0;
            double movingAverage = 0;
            int sizeOfNeighborhood = p.OperationSet().Count();
            w.WriteLine("loop:vbest:vx:vdom:doptx:ddomx:r10:r100:cor10:cor100:dm10:dm100:dom:x");

            for (int loop = 0; true; loop++)
            {
                ISolution dom = s;//new LocalSearch().Solve(p, s);

                doms.Add(dom);
                ss.Add(s.Clone());

                escape = false;
                int cycle = 0;
                var t = find(slist, s);
                if (t != null)
                {
                    cycle = loop - t.Loop;
                    t.Loop = loop;
                    ++t.Repetition;

                    if(maxRepetition < t.Repetition)
                    {
                        ++chaotic;
                        if(maxChaos < chaotic)
                        {
                            chaotic = 0;
                            escape = true;
                        }
                    }
                    else if(cycle < maxCycle)
                    {
                        movingAverage = 0.1 * cycle + 0.9 * movingAverage;
                        tabuList.Enlarge();
                        loopFromLastChange = 0;
                    }
                }
                else
                {
                    slist.Add(new ReactiveTabuSearchItem(s, loop));
                }

                if(movingAverage < loopFromLastChange)
                {
                    tabuList.Ensmall();
                    loopFromLastChange = 0;
                }

                ISolution s1 = ss.ElementAt(Math.Max(ss.Count() - 1 - 1, 0));
                ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10 - 1, 0));
                ISolution s100 = ss.ElementAt(Math.Max(ss.Count() - 100 - 1, 0));
                double r10 = (double)s.DistanceTo(s10) / Math.Min(2 * Math.Min(Math.Max(ss.Count() - 1, 1), 10), p.Size);
                double r100 = (double)s10.DistanceTo(s100) / Math.Min(2 * Math.Min(Math.Max(ss.Count() - 1, 1), 90), p.Size);
                
                double cor10 = calcCorrelation(s, s1, s10);
                double cor100 = calcCorrelation(s, s10, s100);
                
                int dm10 = Diameter.CalculateDiameter(ss, 10).DiameterValue;
                int dm100 = Diameter.CalculateDiameter(ss, 100).DiameterValue;
                
                w.WriteLine(Trace2(loop, p.Optimum, lopt, dom, ret, s, p.OperationSet(), tabuList, p, r10, r100, cor10, cor100, dm10, dm100));
                Console.WriteLine(loop + ":" + ret.Value + ":" + s.Value + ":" + cycle + ":" + chaotic + ":" + loopMax + ":" + escape);

                if (escape)
                {
                    int steps = (int)(1 + (1 + StrictRandom.Next()) * movingAverage / 2);
                    while(0 <= steps--)
                    {
                        IOperation op = p.OperationSet().RandomSubset(1, sizeOfNeighborhood).First();
                        int opValue = p.OperationValue(op, s);

                        ISolution tmp = s.Apply(op);

                        if (mk.IsMinimumStrict(tmp.Value))
                            ret = tmp.Clone();

                        s = tmp;
                        tabuList.Add(op, loop, opValue);

                        loopMax -= 1;
                    }
                }
                else
                {
                    IOperation bestOp = p.OperationSet()
                        .Where((op) => tabuList.IsNotTabu(op, loop))
                        .ArgMinStrict((op) => p.OperationValue(op, s));
                    int bestOpValue = p.OperationValue(bestOp, s);
                    ISolution tmp = s.Apply(bestOp);

                    if (mk.IsMinimumStrict(tmp.Value))
                        ret = tmp.Clone();
                    
                    s = tmp;
                    tabuList.Add(bestOp, loop, bestOpValue);

                    loopMax -= sizeOfNeighborhood;
                }

                if (loopMax < 0) return ret;
            }
        }

        private ReactiveTabuSearchItem find(List<ReactiveTabuSearchItem> slist, ISolution s)
        {
            foreach(var t in slist)
            {
                if(s.Equals(t.Solution))
                {
                    return t;
                }
            }
            return null;
        }

        private string Trace2(int loop, ISolution opt, ISolution lopt, ISolution dom, ISolution best, ISolution x, IEnumerable<IOperation> ops, VariableTabuList tl, IOptimizationProblem p, double r10, double r100, double cor10, double cor100, int dm10, int dm100)
        {
            IEnumerable<IOperation> tabuOp = ops.Where((op) => !tl.IsNotTabu(op, loop));
            IEnumerable<IOperation> notTabuOp = ops.Where((op) => tl.IsNotTabu(op, loop));
            int notTabuUp = notTabuOp.Count((op) => p.OperationValue(op, x) < 0);
            int notTabuDn = notTabuOp.Count() - notTabuUp;

            return loop + ":" + best.Value + ":" + x.Value + ":" + dom.Value + ":" + opt.DistanceTo(x) + ":" + dom.DistanceTo(x) + ":"
                + r10.ToString("F5") + ":" + r100.ToString("F5") + ":" + cor10.ToString("F5") + ":" + cor100.ToString("F5") + ":" + dm10 + ":" + dm100
                + ":" + dom + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[loop=" + loopMax + ",tl=" + tabuLength + "]";
        }

        public ReactiveTabuSearch(long loopMax, int tabuLength)
        {
            this.loopMax = loopMax;
            this.tabuLength = tabuLength;
        }
    }
}
