using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class AdaptiveRandomLocalSearch : IOptimizationAlgorithm
    {
        private int loopMax;
        private int initialSizeOfSubset;
        private int maxActivity;
        private int feedBack;
        private int tryCount;
        
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
            ISolution ret = sol.Clone();
            ISolution s = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();
            int maxCalc = this.loopMax;
            int sizeOfSubset = this.initialSizeOfSubset;
            Queue<int> q = new Queue<int>(feedBack);
            int loop = 0;
            int bestLoop1 = 0;
            int bestLoop2 = 0;

            w.WriteLine("loop:r:sub:vx:doptx:vdom:ddomx:x");

            for (int totalCalc = 0; totalCalc < maxCalc; totalCalc += sizeOfSubset)
            {
                double plus = p.OperationSet().Where((op) => p.OperationValue(op, s) >= 0).Count();
                double minus = p.OperationSet().Where((op) => p.OperationValue(op, s) < 0).Count();

                ISolution dom = new LocalSearch().Solve(p, s);

                w.WriteLine(Trace(loop, minus / (plus + minus), sizeOfSubset, p.Optimum, dom, s));

                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
                ISolution tmp = s.Apply(bestOp);

                if (mk.IsMinimumStrict(tmp.Value))
                {
                    ret = tmp.Clone();
                    bestLoop1 = loop;
                    bestLoop2 = loop;
                }

                if(feedBack <= q.Count())
                {
                    q.Dequeue();
                }
                q.Enqueue(tmp.Value - s.Value);

                if(maxActivity <= loop && loop % 1000 == 0)
                {
                    sizeOfSubset += initialSizeOfSubset;
                }

                /*
                if (maxActivity < loop)
                {
                    if (feedBack < loop - bestLoop1)
                    {
                        sizeOfSubset = Math.Min(sizeOfSubset + tryCount, this.initialSizeOfSubset * 2);
                        bestLoop1 = loop;
                    }


                    if (feedBack * 40 < loop - bestLoop2)
                    {
                        sizeOfSubset = Math.Max(sizeOfSubset / 2, this.initialSizeOfSubset - 40 * tryCount);
                        //sizeOfSubset = Math.Max(sizeOfSubset / 2, this.initialSizeOfSubset / 2);
                        bestLoop1 = bestLoop2 = loop;
                    }
                }
                 */ 
                /*
                                if(q.Select((x) => Math.Abs(x)).Sum() != 0 && q.Sum() / q.Select((x) => Math.Abs(x)).Sum() == 0)
                                {
                                    sizeOfSubset -= tryCount;
                                }

                                if (maxActivity <= q.Select((x) => Math.Abs(x) * Math.Abs(x)).Sum())
                                {
                                    sizeOfSubset += tryCount;
                                }

                                while (sizeOfSubset <= 0)
                                {
                                    sizeOfSubset += tryCount;
                                }
                                */
                ++loop;
                s = tmp;
            }

            return ret;
        }

        private string Trace(int loop, ISolution opt, ISolution x)
        {
            return loop + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, int sizeOfSubset, ISolution opt, ISolution dom, ISolution x)
        {
            return loop + ":" + ratio + ":" + sizeOfSubset + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[loop=" + loopMax + ",subset=" + initialSizeOfSubset + ",maxAct=" + maxActivity + ",fb=" + feedBack + ",try=" + tryCount + "]";
        }

        public AdaptiveRandomLocalSearch(int loopMax, int initialSizeOfSubset, int maxActivity, int feedBack, int tryCount)
        {
            this.loopMax = loopMax;
            this.initialSizeOfSubset = initialSizeOfSubset;
            this.maxActivity = maxActivity;
            this.feedBack = feedBack;
            this.tryCount = tryCount;
        }
    }
}