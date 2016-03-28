using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class TabuSearch2 : IOptimizationAlgorithm
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
            TabuList2 tabuList = new TabuList2(this.tabuLength);
            ISolution lopt = new LocalSearch().Solve(p, sol);
            IOptimizationAlgorithm ls = new LocalSearch();
            List<ISolution> doms = new List<ISolution>();
            List<ISolution> ss = new List<ISolution>();

            //w.WriteLine("loop:vx:doptx:dloptx:doptlopt:x");
            //w.WriteLine("loop:vbest:vx:vdom:r10:r100:r1000:doptx:dloptx:doptlopt:ddomx:doptdom:dloptdom:doptbest:dloptbest:ddombest:dbestx:x");
            w.WriteLine("loop:vbest:vx:vdom:doptx:ddomx:r10:r100:cor10:cor100:dm10:dm100:dom:x");

            for (int loop = 0; loop < loopMax; loop++)
            {
                ISolution dom = new LocalSearch().Solve(p, s);

                doms.Add(dom);
                ss.Add(s.Clone());

                ISolution s1 = ss.ElementAt(Math.Max(ss.Count() - 1 - 1, 0));
                ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10 - 1, 0));
                ISolution s100 = ss.ElementAt(Math.Max(ss.Count() - 100 - 1, 0));
                //ISolution s1000 = ss.ElementAt(Math.Max(ss.Count() - 1000 - 1, 0));
                double r10 = (double)s.DistanceTo(s10) / Math.Min(2 * Math.Min(Math.Max(ss.Count() - 1, 1), 10), p.Size);
                double r100 = (double)s10.DistanceTo(s100) / Math.Min(2 * Math.Min(Math.Max(ss.Count() - 1, 1), 90), p.Size);
                //double r1000 = (double)s100.DistanceTo(s1000) / Math.Min(2 * Math.Min(Math.Max(ss.Count() - 1, 1), 900), p.Size);

                double cor10 = calcCorrelation(s, s1, s10);
                double cor100 = calcCorrelation(s, s10, s100);
                //double cor1000 = calcCorrelation(s, s100, s1000);

                int dm10 = Diameter.CalculateDiameter(ss, 10).DiameterValue;
                int dm100 = Diameter.CalculateDiameter(ss, 100).DiameterValue;
                //int dm1000 = Diameter.CalculateDiameter(ss, 1000).DiameterValue;

                w.WriteLine(Trace2(loop, p.Optimum, lopt, dom, ret, s, p.OperationSet(), tabuList, p, r10, r100, cor10, cor100 , dm10, dm100));

                /*
                ss.Add(s.Clone());
                double r10 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 10, 0))) / Math.Min(2 * Math.Min(ss.Count(), 10), p.Size);
                double r100 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 100, 0))) / Math.Min(2 * Math.Min(ss.Count(), 100), p.Size);
                double r1000 = (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 1000, 0))) / Math.Min(2 * Math.Min(ss.Count(), 1000), p.Size);

                w.WriteLine(Trace2(loop, p.Optimum, lopt, dom, ret, s, p.OperationSet(), tabuList, p, r10, r100, r1000));
                //w.WriteLine(Trace2(loop, p.Optimum, lopt, ls.Solve(p, s), ret, s, p.OperationSet(), tabuList, p));
                */
                IOperation bestOp = p.OperationSet()
                    .Where((op) => tabuList.IsNotTabu(op, loop))
                    .ArgMinStrict((op) => p.OperationValue(op, s));
                int bestOpValue = p.OperationValue(bestOp, s);
                ISolution tmp = s.Apply(bestOp);

                if (mk.IsMinimumStrict(tmp.Value))
                    ret = tmp.Clone();

                s = tmp;
                tabuList.Add(bestOp, loop, bestOpValue);
            }
            //w.WriteLine(Trace(loopMax, p.Optimum, lopt, ls.Solve(p, s), ret, s));
            //w.WriteLine(Trace(loopMax, p.Optimum, lopt, ls.Solve(p, ret), ret, ret));
            //w.WriteLine(loopMax + ":" + s.Value + ":" + lopt.DistanceTo(s) + ":" + p.Optimum.DistanceTo(s) + ":" + s.ToString());
            //w.WriteLine(loopMax + ":" + ret.Value + ":" + lopt.DistanceTo(ret) + ":" + p.Optimum.DistanceTo(ret) + ":" + ret.ToString());

            //Analizer.createDomainMap(ss, doms, p);

            return ret;
        }

        private string Trace2(int loop, ISolution opt, ISolution lopt, ISolution dom, ISolution best, ISolution x, IEnumerable<IOperation> ops, TabuList2 tl, IOptimizationProblem p, double r10, double r100, double r1000)
        {
            return loop + ":" + best.Value + ":" + x.Value + ":" + dom.Value + ":" + r10.ToString("F5") + ":" + r100.ToString("F5") + ":" + r1000.ToString("F5")
                + ":" + opt.DistanceTo(x) + ":" + lopt.DistanceTo(x) + ":" + opt.DistanceTo(lopt) + ":"
                + dom.DistanceTo(x) + ":" + opt.DistanceTo(dom) + ":" + lopt.DistanceTo(dom) + ":" + opt.DistanceTo(best) + ":" + lopt.DistanceTo(best)
                + dom.DistanceTo(best) + ":" + x.DistanceTo(best) + ":" + x //+ "{" + kvp.Select((kv) => kv.Value).Min() + "," + kvp.Select((kv) => kv.Value).Max() + "}"
                //+ ":"+ string.Join(",", kvp.Select((kv) => kv.Key))
                ;
        }

        private string Trace2(int loop, ISolution opt, ISolution lopt, ISolution dom, ISolution best, ISolution x, IEnumerable<IOperation> ops, TabuList2 tl, IOptimizationProblem p, double r10, double r100, double cor10, double cor100, int dm10, int dm100)
        {
            IEnumerable<IOperation> tabuOp = ops.Where((op) => !tl.IsNotTabu(op, loop));
            IEnumerable<IOperation> notTabuOp = ops.Where((op) => tl.IsNotTabu(op, loop));
            int notTabuUp = notTabuOp.Count((op) => p.OperationValue(op, x) < 0);
            int notTabuDn = notTabuOp.Count() - notTabuUp;

            return loop + ":" + best.Value + ":" + x.Value + ":" + dom.Value + ":" + opt.DistanceTo(x) + ":" + dom.DistanceTo(x) + ":"
                + r10.ToString("F5") + ":" + r100.ToString("F5") + ":" + cor10.ToString("F5") + ":" + cor100.ToString("F5") + ":" + dm10 + ":" + dm100 
                + ":" + dom + ":" + x //+ "{" + kvp.Select((kv) => kv.Value).Min() + "," + kvp.Select((kv) => kv.Value).Max() + "}"
                //+ ":"+ string.Join(",", kvp.Select((kv) => kv.Key))
                ;
        }

        private string Trace2(int loop, ISolution opt, ISolution lopt, ISolution dom, ISolution best, ISolution x, IEnumerable<IOperation> ops, TabuList2 tl, IOptimizationProblem p)
        {
            IEnumerable<IOperation> tabuOp = ops.Where((op) => !tl.IsNotTabu(op, loop));
            int tabuUpUp = tabuOp.Count((op) => p.OperationValue(op, x) < 0 & tl.OperationToTabuElement(op, loop).Value < 0);
            int tabuUpDn = tabuOp.Count((op) => p.OperationValue(op, x) < 0 & tl.OperationToTabuElement(op, loop).Value >= 0);
            int tabuDnUp = tabuOp.Count((op) => p.OperationValue(op, x) >= 0 & tl.OperationToTabuElement(op, loop).Value < 0);
            int tabuDnDn = tabuOp.Count((op) => p.OperationValue(op, x) >= 0 & tl.OperationToTabuElement(op, loop).Value >= 0);
            IEnumerable<IOperation> notTabuOp = ops.Where((op) => tl.IsNotTabu(op, loop));
            int notTabuUp = notTabuOp.Count((op) => p.OperationValue(op, x) < 0);
            int notTabuDn = notTabuOp.Count() - notTabuUp;

            return loop + "," + best.Value + "," + dom.Value + ","  + x.Value +" , " + tabuUpUp + "," + tabuUpDn + "," + notTabuUp + " , " + tabuDnUp + "," + tabuDnDn + "," + notTabuDn// + " , " + x
                + ","+ opt.DistanceTo(x) + "," + lopt.DistanceTo(x) + "," + opt.DistanceTo(lopt) + ","
                + dom.DistanceTo(x) + "," + opt.DistanceTo(dom) + "," + lopt.DistanceTo(dom) + "," + opt.DistanceTo(best) + "," + lopt.DistanceTo(best) + ","
                + dom.DistanceTo(best) + "," + x.DistanceTo(best) //+ "{" + kvp.Select((kv) => kv.Value).Min() + "," + kvp.Select((kv) => kv.Value).Max() + "}"
                //+ ":"+ string.Join(",", kvp.Select((kv) => kv.Key))
                ;
        }

        private string Trace(int loop, ISolution opt, ISolution lopt, ISolution dom, ISolution best, ISolution x)
        {
            return loop + ":" + best.Value + ":" + x.Value + ":" + dom.Value + ":" + opt.DistanceTo(x) + ":" + lopt.DistanceTo(x) + ":" + opt.DistanceTo(lopt) + ":"
                + dom.DistanceTo(x) + ":" + opt.DistanceTo(dom) + ":" + lopt.DistanceTo(dom) + ":" + opt.DistanceTo(best) + ":" + lopt.DistanceTo(best) + ":"
                + dom.DistanceTo(best) + ":" + x.DistanceTo(best) + ":" + x;
        }
        
        private string Trace(int loop, ISolution x, ISolution opt, ISolution lopt)
        {
            return loop + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + lopt.DistanceTo(x) + ":" + opt.DistanceTo(lopt) + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[loop=" + loopMax + ",tl=" + tabuLength + "]";
        }

        public TabuSearch2(int loopMax, int tabuLength)
        {
            this.loopMax = loopMax;
            this.tabuLength = tabuLength;
        }
    }
}
