using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class RandomLocalSearch :  IOptimizationAlgorithm
    {
        private int loopMax;
        private int sizeOfSubset;
        private long limit;
        
        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        private double calcRatio(ISolution s, List<ISolution> ss, int n)
        {
            return (double)s.DistanceTo(ss.ElementAt(Math.Max(ss.Count() - 2, 0)));
        }
        
        private double calcCorrelation(ISolution origin, ISolution sa, ISolution sb)
        {
            double dist_oa = origin.DistanceTo(sa);
            double dist_ob = origin.DistanceTo(sb);
            double dist_ab = sa.DistanceTo(sb);
            double ret = (dist_oa * dist_oa + dist_ob * dist_ob - dist_ab * dist_ab) / (2.0 * dist_oa * dist_ob);

            if(double.IsNaN(ret))
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
            ISolution ret = sol.Clone();
            ISolution s = sol.Clone();
            ISolution domBest = sol.Clone();
            MinimumKeeper mk = new MinimumKeeper();
            long endCount;
            List<ISolution> doms = new List<ISolution>();
            List<ISolution> ss = new List<ISolution>();
            int sizeOfNeighborhood = p.OperationSet().Count();
            int sumDistance = 0;
            double sumV = 0.0;
            double varV = 0.0;
            double sumD = 0.0;
            double varD = 0.0;
            int count = 0;
            w.WriteLine("loop:r:vx:doptx:vdom:ddomx:r10:r100:r1000:cor10:cor100:cor1000:dm10:dm100:dm1000:dom:x");
            
            for(int loop = 0; loop < loopMax; loop++)
            {
                double plus = p.OperationSet().Where((op) => p.OperationValue(op, s) >= 0).Count();
                double minus = p.OperationSet().Where((op) => p.OperationValue(op, s) < 0).Count();
                endCount = limit;

                ISolution dom = new LocalSearch().Solve(p, s);
                if (dom.Value < domBest.Value) domBest = dom.Clone();

                doms.Add(dom);
                ss.Add(s.Clone());

                //ISolution s1    = ss.ElementAt(Math.Max(ss.Count() - 1, 0));
                ISolution s10   = ss.ElementAt(Math.Max(ss.Count() -   10, 0));
                ISolution s100  = ss.ElementAt(Math.Max(ss.Count() -  100, 0));
                //ISolution s1000 = ss.ElementAt(Math.Max(ss.Count() - 1000, 0));
                double r10 = (double)s.DistanceTo(s10) / Math.Min(2 * Math.Min(Math.Max(ss.Count(), 1), 10), p.Size);
                double r100 = (double)s10.DistanceTo(s100) / Math.Min(2 * Math.Min(Math.Max(ss.Count(), 1), 90), p.Size);
                //double r1000 = (double)s100.DistanceTo(s1000) / Math.Min(2 * Math.Min(Math.Max(ss.Count(), 1), 900), p.Size);
                
                //double cor10 = calcCorrelation(s, s, s10);
                double cor100 = calcCorrelation(s, s10, s100);
                //double cor1000 = calcCorrelation(s, s100, s1000);

                Diameter dm10   = Diameter.CalculateDiameter(ss, 10);
                Diameter dm100  = Diameter.CalculateDiameter(ss, 100);
                //Diameter dm1000 = Diameter.CalculateDiameter(ss, 1000);

                //Console.WriteLine(loop + ":" + s.Value + ":" + dom.Value + ":" + endCount);
                w.WriteLine(Trace(loop, minus / (plus + minus), p.Optimum, dom, r10, r100, 0, 0, cor100, 0, dm10.DiameterValue, dm100.DiameterValue, 0, s));

                IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
                //int val = p.OperationValue(bestOp, s);
                ISolution tmp = s.Clone().Apply(bestOp);

                if (s.Value <= ret.Value)
                    ret = s.Clone();

                /*
                if (this.loop == 0 && endCount <= 0)
                {
                    //Analizer.createDomainMap(ss, doms, p);
                    return ret;
                }

                if (limit == 0 && this.loop <= loop)
                {
                    //Analizer.createDomainMap(ss, doms, p);
                    return ret;
                }

                if (this.loop == 0 && mk.IsNotMinimumStrict(tmp.Value))
                {
                    endCount -= sizeOfSubset;
                    --loop;
                }/**/

                if (500 < loop)
                {
                    sumV += s.Value;
                    varV += (double)s.Value * (double)s.Value;
                    sumD += sol.DistanceTo(tmp);
                    varD += sol.DistanceTo(tmp) * sol.DistanceTo(tmp);
                    ++count;
                    //Console.WriteLine(varV);
                }

                sumDistance += s.DistanceTo(tmp);
                //Console.WriteLine(sizeOfSubset + "\t" + loop + "\t" + sol.DistanceTo(tmp) + "\t" + sumDistance + "\t" + ((double)sol.DistanceTo(tmp) / sumDistance));
                s = tmp;
            }

            //Console.WriteLine(varV + "\t" + Math.Pow(sumV / count, 2));
            Console.WriteLine(sizeOfSubset + "\t" + (sumV / count) + "\t" + Math.Sqrt(varV / count - Math.Pow(sumV / count, 2.0)) + "\t" + (sumD / count) + "\t" + Math.Sqrt(varD / count - Math.Pow(sumD / count, 2.0)));
            //Console.WriteLine();
            //Console.WriteLine(sizeOfSubset + ":" + sizeOfNeighborhood + ":" + ret.Value + ":" + domBest.Value + ":" + (ret.Value - domBest.Value));
            return ret;
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
        private string Trace(int loop, double ratio, ISolution opt, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, Diameter d1, Diameter d2, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + d1.DiameterValue + ":" + d1.RadiusValue + ":" + d2.DiameterValue + ":" + d2.RadiusValue + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, double r10, double r100, double rr10, double rr100, ISolution x)
        {
            return loop + ":" + ratio.ToString("F5") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + r10.ToString("F5") + ":" + r100.ToString("F5") + ":" + rr10.ToString("F5") + ":" + rr100.ToString("F5") + ":" + x;
        }
        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, double r10, double r100, double r1000, double rr10, double rr100, double rr1000, int dm10, int dm100, int dm1000, ISolution x)
        {
            return loop + ":" + ratio.ToString("F5") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + r10.ToString("F5") + ":" + r100.ToString("F5") + ":" + r1000.ToString("F5") + ":" + rr10.ToString("F5") + ":" + rr100.ToString("F5") + ":" + rr1000.ToString("F5") + ":" + dm10 + ":" + dm100 + ":" + dm1000 + ":" + dom + ":" + x;
        }

        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, Diameter d1, Diameter d2, int d10, int d100, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + d1.DiameterValue + ":" + d2.DiameterValue + ":" + d10 + ":" + d100 + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[loop=" + loopMax + ",subset=" + sizeOfSubset + ",lim=" + limit + "]";
        }

        public RandomLocalSearch(int loop, int sizeOfSubset, long limit)
        {
            this.loopMax = loop;
            this.sizeOfSubset = sizeOfSubset;
            this.limit = limit;
        }

        public RandomLocalSearch(int sizeOfSubset, long limit)
        {
            this.loopMax = 0;
            this.sizeOfSubset = sizeOfSubset;
            this.limit = limit;
        }
    }
}