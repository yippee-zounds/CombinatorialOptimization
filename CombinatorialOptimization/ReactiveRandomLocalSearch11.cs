using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using System.IO;

namespace CombinatorialOptimization
{
    // RRLS9の多軌道版
    // 制約付最適化問題(KnapsackProblem)にペナルティ関数法で対応
    class ReactiveRandomLocalSearch11 : IOptimizationAlgorithm
    {
        private int initialSizeOfSubset;
        private long limit;
        private int targetRange;
        private double targetHighAct;
        private double targetLowAct;
        private int trajectory;
        private int movingRange;

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
            for(int i = 0; i <trajectory; i++) bestDomValue[i] = int.MaxValue;

            int[] subLoop = new int[trajectory];
            int[] bestLoop = new int[trajectory];
            int[] lastBest = new int[trajectory];

            int sizeOfNeighborhood = p.OperationSet().Count();
            long endCount = limit;
            ISolution gbest = s.ArgMinStrict((x) => x.Value);
            int gbestLoop = 0;
            int movingMode = -1;
            int cbestIndex = -1;
            int lastMovingMode = -1;
            w.WriteLine("loop:sub:r:vx:doptx:vdom:ddomx:doptdom:r10:r100:tact:dm10:dm100:dom:x");

            List<ISolution>[] ss = new List<ISolution>[trajectory];
            for (int i = 0; i < trajectory; i++) ss[i] = new List<ISolution>();

            List<int>[] sss = new List<int>[trajectory];
            for (int i = 0; i < trajectory; i++) sss[i] = new List<int>();
            //List<ISolution> doms = new List<ISolution>();

            for (int loop = 0; true; loop++)
            {
                int distMinX = int.MaxValue;
                int distMaxX = int.MinValue;
                int distMinL = int.MaxValue;
                int distMaxL = int.MinValue;
                for (int i = 0; i < trajectory - 1; i++)
                {
                    for (int j = i + 1; j < trajectory; j++)
                    {
                        int distTmp = s[i].DistanceTo(s[j]);
                        distMinX = Math.Min(distMinX, distTmp);
                        distMaxX = Math.Max(distMaxX, distTmp);

                        distTmp = lbest[i].DistanceTo(lbest[j]);
                        distMinL = Math.Min(distMinL, distTmp);
                        distMaxL = Math.Max(distMaxL, distTmp);

                    }
                }

                ++gbestLoop;

                if(movingRange < gbestLoop)
                {
                    int maxValue = int.MinValue;

                    for (int i = 0; i < trajectory; i++)
                    {
                        if (maxValue < lbest[i].Value)
                        {
                            maxValue = lbest[i].Value;
                            movingMode = i;
                            lastMovingMode = i;
                        }
                    }
                }
                if(movingRange * 2 < gbestLoop)
                {
                    movingMode = -1;
                    gbestLoop = 0;
                }


                for (int t = 0; t < trajectory; t++)
                {
                    //ISolution dom = new LocalSearch().Solve(p, s[t]);
                    //bestDomValue[t] = Math.Min(bestDomValue[t], dom.Value);

                    //doms.Add(dom);
                    ss[t].Add(s[t].Clone());
                    if (10 < ss[t].Count())
                    {
                        ss[t].RemoveAt(0);
                    }
                    sss[t].Add(s[t].Value);
                    if (targetRange < sss.Count())
                    {
                        sss[t].RemoveAt(0);
                    }
                    ISolution s10 = ss[t].ElementAt(Math.Max(ss[t].Count() - 10, 0));
                    //ISolution s100 = ss[t].ElementAt(Math.Max(ss[t].Count() - 100, 0));

                    int d10 = 0;
                    for (int i = 0; i < Math.Min(ss[t].Count - 1, 10); i++)
                    {
                        d10 += ss[t].ElementAt(i).DistanceTo(ss[t].ElementAt(i + 1));
                    }
                    double r10 = (double)s[t].DistanceTo(s10) / d10;
                    /*
                    int d100 = 0;
                    for (int i = 0; i < Math.Min(ss[t].Count - 1, 100); i++)
                    {
                        d100 += ss[t].ElementAt(i).DistanceTo(ss[t].ElementAt(i + 1));
                    }
                    double r100 = (double)s10.DistanceTo(s100) / d100;
                     */ 
                    Diameter dm10 = Diameter.CalculateDiameter(ss[t], 10);
                    //Diameter dm100 = Diameter.CalculateDiameter(ss[t], 100);

                    Console.Write(loop + ":" + t.ToString("D2") + ":" + movingMode.ToString("D2") + ":" + gbest.Value + ":" + s[t].Value + ":" + distMaxX + "," + distMinX + ":" + s[t].DistanceTo(p.Optimum).ToString("D3") + ":" + s[t].DistanceTo(lbest[t]).ToString("D3") + ":" + sizeOfSubset[t].ToString("D3") + ":" + targetAct[t].ToString("F2") + ":" + r10.ToString("F2") + ":" + ":" + endCount);
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
                        Console.WriteLine(" ");
                    }

                    if (3 * targetRange + 1000 < loop - lastBest[t])
                    {
                        targetRange += 1000;
                        lastBest[t] = loop;
                    }

                    if (100 < bestLoop[t])
                    {
                        targetAct[t] = targetHighAct;
                    }
                    else
                    {
                        targetAct[t] = targetLowAct;
                    }

                    if (0 <= subLoop[t])
                    {

                        if (r10 < targetAct[t])
                        {
                            sizeOfSubset[t] = Math.Max(sizeOfSubset[t] - (int)(sizeOfNeighborhood * 0.01), 1);
                            subLoop[t] = -10;// Math.Min(-10, sizeOfSubset[t]);
                        }
                        else
                        {
                            sizeOfSubset[t] = Math.Min(sizeOfSubset[t] + (int)(sizeOfNeighborhood * 0.01), sizeOfNeighborhood);
                            subLoop[t] = -10;// Math.Min(-10, sizeOfSubset);
                        }
                    }
                    if (!s[t].IsFeasible())
                    {
                        sizeOfSubset[t] = Math.Min(sizeOfSubset[t] + (int)(sizeOfNeighborhood * 0.1), sizeOfNeighborhood);
                    }
                    ++subLoop[t];
                    ++bestLoop[t];
                    w.WriteLine(Trace(loop, sizeOfSubset[t], 0, p.Optimum, s[t], r10, 0, dm10.DiameterValue, 0, targetAct[t], s[t]));
                    //w.WriteLine(Trace(loop, sizeOfSubset[t], 0, p.Optimum, dom, r10, r100, dm10.DiameterValue, dm100.DiameterValue, targetAct[t], s[t]));

                    ISolution tmp = null;
                    if (t == movingMode)
                    {
                        IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset[t]).ArgMinStrict((op) => s[t].OperationDistance(op, s));
                        tmp = s[t].Apply(bestOp);
                    }
                    else
                    {
                        IOperation bestOp = p.OperationSet().RandomSubset(sizeOfNeighborhood, sizeOfSubset[t]).ArgMinStrict((op) => p.OperationValue(op, s[t]));
                        int delta = p.OperationValue(bestOp, s[t]);
                        tmp = s[t].Apply(bestOp);
                        endCount -= sizeOfSubset[t];
                    }

                    if (s[t].Value < sss[t].Skip(sss[t].Count() - targetRange).Min((x) => x))
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

                for (int i = 0; i < trajectory; i++ )
                {
                    if(s[i].Value < cbest.Value)
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

        public ReactiveRandomLocalSearch11(long limit, int trajectry, double targetHighAct, double targetLowAct, int movingRange)
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