using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
using Drace.KnapsackProblem;
using Drace.TravelingSalesmanProblem;
using Drace.QuadraticAssignmentProblem;
using Drace.Optimization.FSSP;
using Combinatorics;
using System.IO;

namespace CombinatorialOptimization
{
    class Program
    {
        static string root = @"D:\Result\data\";

        static void Main(string[] args)
        {
#if DEBUG
            root = @"D:\Result\debug\";
#endif
            /*
            Fssp fssp = new Fssp("ta001");
            long count = 0;
            var data = new Dictionary<int, int>();
            foreach(var p in Permutation.EnumerateAllPermutation(20))
            {
                int ret = fssp.Evaluate(p.p);

                if (!data.ContainsKey(ret)) data.Add(ret, 0);
                data[ret]++;

                if ((++count) % 10000000 == 0)
                {
                    Console.WriteLine(ret + ":" + p);
                    File.WriteAllLines(@"d:\Result\ta001.sol", data.Keys.OrderBy((k) => k).Select((k) => "" + k + "\t" + data[k]));
                }
            }*/

            for (int i = 0; true; i++)
            {
                SingleProblemTest(new TravelingSalesmanProblem("pr107"), 100);
                //SingleFsspTest();
                //SingleTspTest(1);
            }
        }

        static void SingleProblemTest(IOptimizationProblem p, int itr)
        {
            IAlgorithm a = new Algorithm.LocalSearch();

            for (int i = 0; i < itr; i++)
            {
                ISolution xa = a.solve(p, p.CreateRandomSolution(), new NullWriter());
                System.Console.WriteLine(xa.Value);
            }

        }

        static void SingleFsspTest()
        {
            IOptimizationProblem p = new FlowShopSchedulingProblem("ta041");
            //IOptimizationAlgorithm a = new HigherLevelSearch(1000, 5);
            IOptimizationAlgorithm a = new RandomLocalSearch(100000, 500, 0);
            DataStoringWriter w = new DataStoringWriter(root + p.Name + @"\" + a.ToString());
            ISolution s = null;

            for (int i = 0; i < 1000; i++)
            {
                w.Open();
                s = a.Solve(p, w);
                w.Close(s.Value);
            }

            Console.WriteLine("End.");
            Console.ReadKey();
        }

        static void SingleTspTest(int itr)
        {
            IOptimizationProblem p = new TravelingSalesmanProblem("pr439");
            //IOptimizationAlgorithm a = new HigherLevelSearch(5000, 10);
            //IOptimizationAlgorithm a = new LocalSearch();
            //IOptimizationAlgorithm a = new TabuSearch2(5000, 100);
            IOptimizationAlgorithm a = new RandomLocalSearch(40000, 8000, 0);
            //IOptimizationAlgorithm a = new MultipleRandomLocalSearch(5, 10000 * 500 * 2, 500);
            DataStoringWriter w = new DataStoringWriter(root + p.Name + @"\" + a.ToString());
            ISolution s = null;

            Console.Write(p.Optimum.Value);

            for (int i = 0; i < itr; i++)
            {
                w.Open();
                s = a.Solve(p, w);
                w.Close(s.Value);
            }

            Console.WriteLine("\t" + s.Value);
            //Console.ReadKey();
        }
        
        static void CombinationTest(int itr, int loopMax)
        {
            DataStoringWriter w = null;
            IOptimizationProblem[] op = new IOptimizationProblem[]
            {
                //new TravelingSalesmanProblem("kroC100"),
                //new TravelingSalesmanProblem("pr107"),
                new TravelingSalesmanProblem("kroA100"),
                //new QuadraticAssignmentProblem("sko81"),
                ///new TravelingSalesmanProblem("ch150"),
                //new BitArrayProblem(100, 20),
                //new KnapsackProblem(1000),
                //new FlowShopSchedulingProblem("ta041"),
                /*
                new TravelingSalesmanProblem("eil51"),
                new KnapsackProblem( 500),
                new KnapsackProblem( 100),
                new FlowShopSchedulingProblem("ta041"),
                new QuadraticAssignmentProblem("sko64"),
                
                new QuadraticAssignmentProblem("sko81"),
                new TravelingSalesmanProblem("pr76"),
                
                
                /*
                new TravelingSalesmanProblem("eil76"),
                new TravelingSalesmanProblem("eil101"),
                new TravelingSalesmanProblem("st70"),
                /**/ 
                //new TravelingSalesmanProblem("gr120"),
                //new TravelingSalesmanProblem("gr202"),
                //new TravelingSalesmanProblem("pa561"),
                //new TravelingSalesmanProblem("pr1002")
                //new TravelingSalesmanProblem("pr2392")
                
                //new TravelingSalesmanProblem("att48"),
                //new TravelingSalesmanProblem("pcb442"),
                //new TravelingSalesmanProblem("pr439"),
                //new FlowShopSchedulingProblem("ta001"),
            };
            IOptimizationAlgorithm[] oa = new IOptimizationAlgorithm[]
            {
                //new RandomWalk(10000),
                //new ReactiveRandomLocalSearch6((long)30000000, 0.75, 0.30, 3000),
                new RandomLocalSearch(2000, 128 + 64 + 32, 0),
                new RandomLocalSearch(2000, 512 + 256, 0),
                
                /*new RandomLocalSearch(2000, 64 + 32, 0),
                new RandomLocalSearch(2000, 128 + 64, 0),
                new RandomLocalSearch(2000, 256 + 128, 0),
                new RandomLocalSearch(2000, 512 + 256, 0),
                /*new RandomLocalSearch(2000, 1, 0),
                new RandomLocalSearch(2000, 2, 0),
                new RandomLocalSearch(2000, 4, 0),
                new RandomLocalSearch(2000, 8, 0),
                new RandomLocalSearch(2000, 16, 0),
                new RandomLocalSearch(2000, 32, 0),
                new RandomLocalSearch(2000, 64, 0),
                new RandomLocalSearch(2000, 128, 0),
                new RandomLocalSearch(2000, 256, 0),
                new RandomLocalSearch(2000, 512, 0),
                new RandomLocalSearch(2000, 1024, 0),
                new RandomLocalSearch(2000, 2048, 0),
                new RandomLocalSearch(2000, 4096, 0),
                new RandomLocalSearch(2000, 100*99/2, 0),
                //new ReactiveStochasticSearch01((long)30000000, 0.65, 0.3),
                //new ReactiveStochasticSearch06((long)30000000, 0.65, 0.3, 3),
                //new ReactiveRandomLocalSearch69( (long)150000000, 0.70, 0.35, 1000),
                /*new ReactiveRandomLocalSearch69((long)150000000, 0.75, 0.40, 1000),
                new ReactiveRandomLocalSearch69((long)150000000, 0.70, 0.40, 1000),
                new ReactiveRandomLocalSearch69((long)150000000, 0.65, 0.40, 1000),
                new ReactiveRandomLocalSearch69((long)150000000, 0.75, 0.35, 1000),
                new ReactiveRandomLocalSearch69((long)150000000, 0.65, 0.35, 1000),
                new ReactiveRandomLocalSearch69((long)150000000, 0.75, 0.30, 1000),
                new ReactiveRandomLocalSearch69((long)150000000, 0.70, 0.30, 1000),
                new ReactiveRandomLocalSearch69((long)150000000, 0.65, 0.30, 1000),
                //new ReactiveRandomLocalSearch63((long)30000000, 0.65, 0.0, 3000),
                //new ReactiveRandomLocalSearch63((long)30000000, 0.65, 0.0, 2000),
                
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.65, 500),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.55, 500),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.45, 500),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.35, 500),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.65, 1000),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.55, 1000),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.45, 1000),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.35, 1000),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.65, 2000),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.55, 2000),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.45, 2000),
                //new ReactiveRandomLocalSearch631((long)30000011, 0.75, 0.35, 2000),
                /*new ReactiveRandomLocalSearch68((long)150000003, 0.70, 0.0, 200),
                new ReactiveRandomLocalSearch63((long)150000003, 0.70, 0.0, 400),
                new ReactiveRandomLocalSearch63((long)150000003, 0.75, 0.0, 400),
                new ReactiveRandomLocalSearch63((long)150000003, 0.60, 0.0, 200),
                new ReactiveRandomLocalSearch63((long)150000003, 0.60, 0.0, 400),
                new ReactiveRandomLocalSearch63((long)150000003, 0.65, 0.0, 200),
                new ReactiveRandomLocalSearch63((long)150000003, 0.65, 0.0, 400),
                //new ReactiveRandomLocalSearch63((long)300000003, 15, 0.70, 0.0, 1000, 200),
                //new ReactiveRandomLocalSearch64((long)300000003,  3, 0.70, 0.0, 1000, 200),
                //new ReactiveRandomLocalSearch63((long)3000000003, 0.70, 0.0, 1000),
                //new ReactiveRandomLocalSearch63((long)3000000003, 0.70, 0.0,  500),
                /*new ReactiveRandomLocalSearch63((long)3000000003, 0.70, 0.0, 800),
                new ReactiveRandomLocalSearch63((long)3000000003, 0.70, 0.0, 1000),
                new ReactiveRandomLocalSearch63((long)3000000003, 0.70, 0.0, 1500),
                new ReactiveRandomLocalSearch63((long)3000000003, 0.70, 0.0, 700),
                new ReactiveRandomLocalSearch63((long)3000000003, 0.70, 0.0, 500),
                //new SimulatedAnnealing(5560, 0.96039),
                /*
                new ReactiveRandomLocalSearch63((long)30000002, 1.00, 0.00,  100),
                new ReactiveRandomLocalSearch63((long)30000002, 1.00, 0.00,  200),
                new ReactiveRandomLocalSearch63((long)30000002, 1.00, 0.00,  300),
                new ReactiveRandomLocalSearch63((long)30000002, 1.00, 0.00,  500),
                new ReactiveRandomLocalSearch63((long)30000002, 1.00, 0.00,  700),
                 * */
            };

            foreach (var p in op)
            {
                Console.WriteLine(p.Name + "\topt=" + p.Optimum.Value);
            }

            foreach (var p in op)
            {
                ISolution[] s_init = new ISolution[itr];
                ISolution[] s_lmin = new ISolution[itr];

                for (int i = 0; i < itr; i++)
                {
                    s_init[i] = p.CreateRandomSolution();
                    s_lmin[i] = s_init[i];
                }

                foreach (var a in oa)
                {
                    int min = int.MaxValue;
                    int max = int.MinValue;
                    int sum = 0;

                    ISolution s = null;
                    Timer.Start();
                    for (int i = 0; i < itr; i++)
                    {
                        w = new DataStoringWriter(root + p.Name + @"\" + a.ToString());
                        w.Open();
                        s = a.Solve(p, s_init[i], w);
                        string delta = "000000" + (s_lmin[i].Value - s.Value);
                        w.Close(s.Value + "_" + delta.Substring(delta.Length - 6, 6));

                        min = Math.Min(min, s.Value);
                        max = Math.Max(max, s.Value);
                        sum += s.Value;
                    }
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}  {5}", p.Optimum.Value, min, sum / itr, max, a.ToString(), Timer.Time());
                }
                Console.WriteLine();
            }

            Console.WriteLine("End.");
        }
    }
}
