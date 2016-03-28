using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // RRLS11で常に探索点をMovingModeにした
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class ReactiveRandomLocalSearch12 : IOptimizationAlgorithm
    {
        private int initialSizeOfSubset;
        private long limit;
        private int targetRange;
        private double targetHighAct;
        private double targetLowAct;
        private int trajectory;
        private int movingRange;
        int sizeOfNeighborhood;

        public ISolution Solve(IOptimizationProblem p, DataStoringWriter w)
        {
            return Solve(p, p.CreateRandomSolution(), w);
        }

        public ISolution Solve(IOptimizationProblem p, ISolution s)
        {
            return Solve(p, s, new NullWriter());
        }

        private double CalcR(IEnumerable<ISolution> ss, int n)
        {
            int d = 0;
            for (int i = 0; i < Math.Min(ss.Count() - 1, n); i++)
            {
                d += ss.ElementAt(i).DistanceTo(ss.ElementAt(i + 1));
            }

            return (double)ss.Last().DistanceTo(ss.ElementAt(Math.Max(ss.Count() - n, 0))) / d;
        }

        private int CalcSizeOfSubset(double r10, double targetAct, int sizeOfSubset)
        {
            if (r10 < targetAct)
            {
                sizeOfSubset = Math.Max(sizeOfSubset - (int)(sizeOfNeighborhood * 0.01), 1);
            }
            else
            {
                sizeOfSubset = Math.Min(sizeOfSubset + (int)(sizeOfNeighborhood * 0.01), sizeOfNeighborhood);
            }
            return sizeOfSubset;
        }

        private double GetTargetAct(int bestLoop)
        {
            if (100 < bestLoop)
            {
                return targetHighAct;
            }
            else
            {
                return targetLowAct;
            }
        }

        private void ConsoleOutput(int t, int lastMovingMode, int cbestIndex)
        {
            if (t == lastMovingMode)
            {
                Console.Write("+");
            }
            if (t == cbestIndex)
            {
                Console.WriteLine("*");
            }
            else
            {
                Console.WriteLine();
            }

        }

        public ISolution Solve(IOptimizationProblem p, ISolution sol, DataStoringWriter w)
        {
            ISolution[] s = new ISolution[trajectory];
            for (int i = 0; i < trajectory; i++) s[i] = p.CreateRandomSolution();

            ISolution[] lbest = new ISolution[trajectory];
            for (int i = 0; i < trajectory; i++) lbest[i] = s[i].Clone();

            int[] sizeOfSubset = new int[trajectory];
            for (int i = 0; i < trajectory; i++) sizeOfSubset[i] = this.initialSizeOfSubset;

            double[] startRatio = new double[trajectory];
            for (int i = 0; i < trajectory; i++) startRatio[i] = targetHighAct;

            double[] targetAct = (double[])startRatio.Clone();

            int[] bestDomValue = new int[trajectory];
            for (int i = 0; i < trajectory; i++) bestDomValue[i] = int.MaxValue;

            int[] subLoop = new int[trajectory];
            int[] bestLoop = new int[trajectory];
            int[] lastBest = new int[trajectory];

            sizeOfNeighborhood = p.OperationSet().Count();
            long endCount = limit;
            ISolution gbest = s.ArgMinStrict((x) => x.Value);
            int gbestLoop = 0;
            int movingMode = -1;
            int cbestIndex = -1;
            int lastMovingMode = -1;
            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");

            List<ISolution>[] ss = new List<ISolution>[trajectory];
            for (int i = 0; i < trajectory; i++) ss[i] = new List<ISolution>();

            for (int loop = 0; true; loop++)
            {
                ++gbestLoop;

                if (movingRange * 2 < gbestLoop)
                {
                    movingMode = -1;
                    gbestLoop = 0;
                }
                else if (movingRange < gbestLoop)
                {
                    lastMovingMode = movingMode = lbest.Select((x, i) => new Tuple<ISolution, int>(x, i)).ArgMin((y) => -y.Item1.Value).Item2;
                }

                for (int t = 0; t < trajectory; t++)
                {
                    ss[t].Add(s[t].Clone());
                    double r10 = CalcR(ss[t], 10);

                    Console.Write(loop + ":" + t.ToString("D2") + ":" + movingMode.ToString("D2") + ":" + gbest.Value + ":" + s[t].Value + ":" +/* dom.Value + ":" + distMaxX + "," + distMinX +*/ ":" + s[t].DistanceTo(p.Optimum).ToString("D3") + ":" + s[t].DistanceTo(lbest[t]).ToString("D3") + ":" + sizeOfSubset[t].ToString("D3") + ":" + targetAct[t].ToString("F2") + ":" + r10.ToString("F2") + ":" + ":" + endCount);
                    ConsoleOutput(t, lastMovingMode, cbestIndex);


                    if (3 * targetRange + 1000 < loop - lastBest[t])
                    {
                        targetRange += 1000;
                        lastBest[t] = loop;
                    }

                    targetAct[t] = GetTargetAct(bestLoop[t]);
                    ++bestLoop[t];

                    if (0 <= subLoop[t])
                    {
                        sizeOfSubset[t] = CalcSizeOfSubset(r10, targetAct[t], sizeOfSubset[t]);
                        subLoop[t] = -10;
                    }
                    else
                    {
                        ++subLoop[t];
                    }

                    if (!s[t].IsFeasible())
                    {
                        sizeOfSubset[t] = Math.Min(sizeOfSubset[t] + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
                    }

                    w.WriteLine(Trace(loop, sizeOfSubset[t], 0, p.Optimum, s[t], r10, 0, Diameter.CalculateDiameter(ss[t], 10).DiameterValue, 0, targetAct[t], s[t]));

                    ISolution tmp = null;
                    if (t == movingMode)
                    {
                        IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset[t]).ArgMinStrict((op) => s[t].OperationDistance(op, s));
                        tmp = s[t].Apply(bestOp);
                    }
                    else
                    {
                        IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset[t]).ArgMinStrict((op) => p.OperationValue(op, s[t]));
                        tmp = s[t].Apply(bestOp);
                        endCount -= sizeOfSubset[t];
                    }

                    if (s[t].Value < ss[t].Skip(ss[t].Count() - targetRange).Min((x) => x.Value))
                    {
                        bestLoop[t] = 0;
                    }

                    if (s[t].Value < lbest[t].Value)
                    {
                        lastBest[t] = loop;
                        targetRange = 1000;
                        lbest[t] = s[t].Clone();
                    }


                    s[t] = tmp;
                }

                ISolution cbest = s[0].Clone();

                for (int i = 0; i < trajectory; i++)
                {
                    if (s[i].Value < cbest.Value)
                    {
                        cbest = s[i].Clone();
                        cbestIndex = i;
                    }
                }

                if (cbest.Value < gbest.Value)
                {
                    movingMode = -1;
                    gbestLoop = 0;
                    gbest = cbest.Clone();
                }

                if (endCount <= 0)
                {
                    return gbest;
                }

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
            return this.GetType().Name + "[lim=" + limit + ",t=" + trajectory + ",hact=" + targetHighAct + ",lact=" + targetLowAct + ",mrange=" + movingRange + "]";
        }

        public ReactiveRandomLocalSearch12(long limit, int trajectry, double targetHighAct, double targetLowAct, int movingRange)
        {
            this.limit = limit;
            this.initialSizeOfSubset = 1;
            this.targetHighAct = targetHighAct;
            this.targetLowAct = targetLowAct;
            this.targetRange = 1000;
            this.trajectory = trajectry;
            this.movingRange = movingRange;
        }

    }
}