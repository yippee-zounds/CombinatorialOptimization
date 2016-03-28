using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // RRLS63の多軌道版、相互作用あり
    class ReactiveRandomLocalSearch64 : IOptimizationAlgorithm
    {
        private int initialSizeOfSubset;
        private long limit;
        private int targetRange;
        private int mutualRange;
        private double targetHighAct;
        private double targetLowAct;
        private int trajectory;

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
            ISolution[] st = new ISolution[trajectory];
            ISolution[] lbestt = new ISolution[trajectory];
            int[] sizeOfSubsett = new int[trajectory];
            double[] startRatiot = new double[trajectory];
            double[] targetActt = (double[])startRatiot.Clone();
            int[] bestLoopt = new int[trajectory];
            int[] lastBestt = new int[trajectory];
            List<ISolution>[] sst = new List<ISolution>[trajectory];
            List<int>[] ssst = new List<int>[trajectory];
            List<ISolution>[] domst = new List<ISolution>[trajectory];
            ISolution[] rett = new ISolution[trajectory];

            for (int i = 0; i < trajectory; i++)
            {
                st[i] = p.CreateRandomSolution();
                lbestt[i] = st[i].Clone();
                sizeOfSubsett[i] = this.initialSizeOfSubset;
                startRatiot[i] = targetHighAct;
                sst[i] = new List<ISolution>();
                ssst[i] = new List<int>();
                domst[i] = new List<ISolution>();
                rett[i] = st[i].Clone();
            }

            int sizeOfNeighborhood = p.OperationSet().Count();
            ISolution ret = st.ArgMin((x) => x.Value);
            long endCount = limit;
            int allBestLoop = 0;
            Dictionary<int, int> d = new Dictionary<int, int>();

            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");


            for (int loop = 0; true; loop++)
            {
                for (int t = 0; t < trajectory; t++)
                {
                    ISolution dom = st[t];
                    sst[t].Add(st[t].Clone());
                    if (100 < sst[t].Count()) sst[t].RemoveAt(0);

                    ssst[t].Add(st[t].Value);
                    if (targetRange < ssst[t].Count()) ssst[t].RemoveAt(0);

                    double currentAct = calcActivity(sst[t]);

                    string tt = t.ToString("D2");
                    if (mutualRange < allBestLoop && st[t].Equals(rett.ArgMin((x) => -x.Value)))
                    {
                        tt = "**";
                    }
                    if (rett[t].Value == rett.Min((x) => x.Value))
                    {
                        tt = "!!";
                    }

                    //Console.WriteLine(loop + ":" + tt + ":" + rett.Min((x) => x.Value) + ":" + rett[t].Value + ":" + st[t].Value + ":" + st[t].DistanceTo(p.Optimum).ToString("D4") + ":" + st[t].DistanceTo(ret).ToString("D4") + ":" + sizeOfSubsett[t].ToString("D4") + ":" + ":" + targetActt[t].ToString("F2") + ":" + currentAct.ToString("F2") + ":" + endCount);
                    Console.Write(loop + ":" + tt + ":" + rett.Min((x) => x.Value) + ":" + st[t].Value + ":" + st[0].DistanceTo(p.Optimum).ToString("D2") + ":");
                    for(int u = 0; u < st.Length; u++) Console.Write(st[0].DistanceTo(rett[u]).ToString("D2") + ":");
                    Console.WriteLine(targetActt[t].ToString("F2"));
                    if ((loop + 1) % 1000 == 0) Console.WriteLine(string.Join(", ", d.Select((x) => "{" + x.Key + ", " + x.Value + "}")));

                    int nrange = 3;
                    if (nrange < sst[t].Count() && 2 <= sst[t].Skip(sst[t].Count() - nrange).Count((x) => x.DistanceTo(sst[t].Last()) == 0))
                    {
                        targetActt[t] = Math.Min(targetActt[t] + 0.05, targetHighAct);
                    }
                    else if (bestLoopt[t] == 0)
                    {
                        targetActt[t] = Math.Max(targetActt[t] - 0.05, targetLowAct);
                    }

                    ISolution tmp = null;
                    if (mutualRange < allBestLoop && allBestLoop < mutualRange && st[t].Equals(rett.ArgMin((x) => -x.Value)))
                    {
                        IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubsett[t]).ArgMinStrict((op) => st[t].OperationDistance(op, rett));
                        tmp = st[t].Clone().Apply(bestOp);
                    }
                    else
                    {
                        tmp = moveByActivity(p, st[t], currentAct, targetActt[t], ref sizeOfSubsett[t]);
                        endCount -= sizeOfSubsett[t];
                    }
                    ++bestLoopt[t];

                    if (st[t].Value < ssst[t].Skip(ssst[t].Count() - targetRange).Min((x) => x))
                    {
                        bestLoopt[t] = 0;
                    }

                    if (tmp.Value < rett[t].Value)
                    {
                        int dist = st[t].DistanceTo(rett[t]);
                        int disto = st[t].DistanceTo(p.Optimum);
                        if (2 < dist)
                        {
                            if (!d.ContainsKey(dist)) d.Add(dist, disto);
                            d[dist] = disto;
                        }

                        rett[t] = tmp.Clone();
                        ret = rett.ArgMin((x) => x.Value).Clone();
                        allBestLoop = 0;
                    }

                    if (endCount <= 0)
                    {
                        return ret;
                    }

                    st[t] = tmp;
                }
                ++allBestLoop;
            }
        }

        private double calcActivity(List<ISolution> ss)
        {
            ISolution s10 = ss.ElementAt(Math.Max(ss.Count() - 10, 0));

            int d10 = 0;
            for (int i = 0; i < Math.Min(ss.Count - 1, 10); i++)
            {
                d10 += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));
            }
            return (double)ss.Last().DistanceTo(s10) / d10;
        }

        private ISolution moveByActivity(IOptimizationProblem p, ISolution s, double currentAct, double targetAct, ref int sizeOfSubset)
        {
            int sizeOfNeighborhood = p.OperationSet().Count();

            if (Double.IsNaN(currentAct) || currentAct < targetAct)
            {
                sizeOfSubset = Math.Max(sizeOfSubset - Math.Max(1, (int)(sizeOfNeighborhood * 0.001)), 1);

            }
            else
            {
                sizeOfSubset = Math.Min(sizeOfSubset + Math.Max(1, (int)(sizeOfNeighborhood * 0.001)), sizeOfNeighborhood);
            }
            if (!s.IsFeasible())
            {
                sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
            }

            return moveBySizeOfNeighborhood(p, s, sizeOfNeighborhood, sizeOfSubset);
        }

        private ISolution moveBySizeOfNeighborhood(IOptimizationProblem p, ISolution s, int sizeOfNeighborhood, int sizeOfSubset)
        {
            IEnumerable<IOperation> ops = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset);
            IEnumerable<int> opsf = ops.Select((op) => p.OperationValue(op, s));
            IOperation tmpOp = ops.ArgMinStrict((op) => p.OperationValue(op, s));

            IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset).ArgMinStrict((op) => p.OperationValue(op, s));
            int delta = p.OperationValue(bestOp, s);
            return s.Apply(bestOp);
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
        private string Trace(int loop, int sizeOfSubset, double ratio, ISolution opt, ISolution dom, double r10, double r100, double dm10, double dm100, double targetAct, ISolution x)
        {
            return loop + ":" + sizeOfSubset + ":" + ratio.ToString("F3") + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + dom.DistanceTo(opt) + ":" + r10.ToString("F3") + ":" + r100.ToString("F3") + ":" + targetAct.ToString("F3") + ":" + dm10 + ":" + dm100 + ":" + dom + ":" + x;
        }

        private string Trace(int loop, double ratio, ISolution opt, ISolution dom, Diameter d1, Diameter d2, int d10, int d100, ISolution x)
        {
            return loop + ":" + ratio + ":" + x.Value + ":" + opt.DistanceTo(x) + ":" + dom.Value + ":" + dom.DistanceTo(x) + ":" + d1.DiameterValue + ":" + d2.DiameterValue + ":" + d10 + ":" + d100 + ":" + x;
        }

        public override string ToString()
        {
            return this.GetType().Name + "[lim=" + limit + ",t=" + trajectory + ",hact=" + targetHighAct + ",lact=" + targetLowAct + ",trange=" + targetRange + ",mrange=" + mutualRange + "]";
        }

        public ReactiveRandomLocalSearch64(long limit, int trajectory, double targetHighAct, double targetLowAct, int targetRange, int mutualRange)
        {
            this.limit = limit;
            this.trajectory = trajectory;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
            this.targetRange = targetRange;
            this.mutualRange = mutualRange;
        }

    }
}