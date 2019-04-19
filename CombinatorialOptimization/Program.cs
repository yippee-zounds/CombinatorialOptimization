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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CombinatorialOptimization
{
    class Program
    {
        static string root = "";//@"F:\Result\data3\";

        static void Main(string[] args)
        {
            root = File.ReadAllText(@".\path.ini");

#if DEBUG
            root = @"F:\Result\debug\";
#endif            
            //DomainSearch(1);
            //PhaseTest();

            for (int i = 0; i < 100; i++)
            {
                CombinationTest(1, 3000);
                //SingleFsspTest();
                //SingleTspTest(1);
            }/**/
        }

        public static object LoadFromBinaryFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryFormatter f = new BinaryFormatter();
            //読み込んで逆シリアル化する
            object obj = f.Deserialize(fs);
            fs.Close();

            return obj;
        }

        public static void SaveToBinaryFile(object obj, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            //シリアル化して書き込む
            bf.Serialize(fs, obj);
            fs.Close();
        }

        static void DomainSearch(int itr)
        {
            IOptimizationProblem p = new QuadraticAssignmentProblem("sko81");
            //IOptimizationProblem p = new TravelingSalesmanProblem("pr107");
            IOptimizationAlgorithm[] a = {
                
                new NonReactiveRandomLocalSearch(10001, 0.05),
                new NonReactiveRandomLocalSearch(10001, 0.15),
                new NonReactiveRandomLocalSearch(10001, 0.25),
                new NonReactiveRandomLocalSearch(10001, 0.35),
                new NonReactiveRandomLocalSearch(10001, 0.45),
                new NonReactiveRandomLocalSearch(10001, 0.55),
                new NonReactiveRandomLocalSearch(10001, 0.65),
                new NonReactiveRandomLocalSearch(10001, 0.75),
                new NonReactiveRandomLocalSearch(10001, 0.85),
                new NonReactiveRandomLocalSearch(10001, 0.95),/**/
                /*
                new NonReactiveRandomLocalSearch(10001, 0.00),
                new NonReactiveRandomLocalSearch(10001, 0.10),
                new NonReactiveRandomLocalSearch(10001, 0.20),
                new NonReactiveRandomLocalSearch(10001, 0.30),
                new NonReactiveRandomLocalSearch(10001, 0.40),
                new NonReactiveRandomLocalSearch(10001, 0.50),
                new NonReactiveRandomLocalSearch(10001, 0.60),
                new NonReactiveRandomLocalSearch(10001, 0.70),
                new NonReactiveRandomLocalSearch(10001, 0.80),
                new NonReactiveRandomLocalSearch(10001, 0.90),
                new NonReactiveRandomLocalSearch(10001, 1.00),/**/
                
            };
            DataStoringWriter w = null;
            ISolution s = null;

            /*
            for (int i = 0; i < 100; i++)
            {
                ISolution tmp = new ReactiveRandomLocalSearch69((long)30000000, 0.80, 0.35, 1000).Solve(p, p.CreateRandomSolution());
                SaveToBinaryFile(tmp, root + p.Name + @"\" + tmp.Value + ".sol");
            }*/

            for (int i = 0; i < itr; i++)
            {
                //ISolution x = p.CreateRandomSolution();
                //ISolution x = new HierarchicalRandomLocalSearch01((long)300000, 0.90, 0.70, 0.50).Solve(p, p.CreateRandomSolution());
                //ISolution x = p.Optimum;
                ISolution x = (QapSolution)LoadFromBinaryFile(@".\91102.sol");
                //ISolution x = (ArrayTour)LoadFromBinaryFile(@".\44438.sol");

                for (int j = 0; j < a.Length; j++)
                {
                    w = new DataStoringWriter(root + p.Name + @"\" + a[j].ToString());
                    w.Open();
                    s = a[j].Solve(p, x, w);
                    w.Close(s.Value + "_" + "x" + x.Value);
                }
            }

        }
        
        static void CombinationTest(int itr, int loopMax)
        {
            DataStoringWriter w = null;
            IOptimizationProblem[] op = new IOptimizationProblem[]
            {
                //new TravelingSalesmanProblem("kroC100"),
                //new TravelingSalesmanProblem("pr107"),
                new QuadraticAssignmentProblem("sko81"),
                //new TravelingSalesmanProblem("kroA100"),
                
                //new TravelingSalesmanProblem("ch150"),
                //new BitArrayProblem(100, 20),
                //new KnapsackProblem(1000),
                //new FlowShopSchedulingProblem("ta041"),
                /*
                new TravelingSalesmanProblem("eil101"),
                new TravelingSalesmanProblem("st70"),
                /**/ 
                //new TravelingSalesmanProblem("gr120"),
                //new TravelingSalesmanProblem("gr202"),
                //new TravelingSalesmanProblem("pa561"),
                //new TravelingSalesmanProblem("pr1002")
                //new TravelingSalesmanProblem("pr2392")
                /*
                new TravelingSalesmanProblem("eil76"),
                
                //new TravelingSalesmanProblem("att48"),
                //new TravelingSalesmanProblem("pcb442"),
                //new TravelingSalesmanProblem("pr439"),
                //new FlowShopSchedulingProblem("ta001"),/**/
            };
            IOptimizationAlgorithm[] oa = new IOptimizationAlgorithm[]
            {
                //new ReactiveRandomLocalSearch69((long)30000000, 0.65, 0.45, 1000),
                new HierarchicalComposeSearch0207((long)30000000, 5, 3, 0.85, 0.65, 0.45, 100),
                //new HierarchicalRandomLocalSearch0322((long)30000002, 5, 3, 0.85, 0.65, 0.45, 1000),
                /*
                new HierarchicalRandomLocalSearch022((long)30000002,  2, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch022((long)30000002,  3, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch022((long)30000002,  5, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch022((long)30000002,  7, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch022((long)30000002, 10, 0.85, 0.65, 0.45, 1000),
                /*
                new HierarchicalRandomLocalSearch071((long)30000000,  3, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch071((long)30000000,  4, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch071((long)30000000,  5, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch071((long)30000000,  7, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch071((long)30000000, 10, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch071((long)30000000, 15, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch071((long)30000000, 20, 0.85, 0.65, 0.45, 1000),
                //new NonReactiveRandomLocalSearch(10000, 0.6),
                /*
                new RandomLocalSearch(100000, 324, long.MaxValue - 1),
                new RandomLocalSearch(100000,  32, long.MaxValue - 1),
                new RandomLocalSearch(100000,   2, long.MaxValue - 1),
                new RandomLocalSearch(100000,   1, long.MaxValue - 1),
                /*
                new HierarchicalRandomLocalSearch0A((long)30000005, 1, 0.8, 0.65, 0.4, 1000),
                new HierarchicalRandomLocalSearch0A((long)30000005, 2, 0.8, 0.65, 0.4, 1000),
                new HierarchicalRandomLocalSearch0A((long)30000005, 3, 0.8, 0.65, 0.4, 1000),
                new HierarchicalRandomLocalSearch0A((long)30000005, 4, 0.8, 0.65, 0.4, 1000),
                new HierarchicalRandomLocalSearch0A((long)30000005, 5, 0.8, 0.65, 0.4, 1000),
                /**/
                /*
                new HierarchicalRandomLocalSearch0B((long)30000003, 20, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003, 15, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003, 10, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003,  7, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003,  5, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003,  4, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003,  3, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003,  2, 0.85, 0.65, 0.45, 1000),
                new HierarchicalRandomLocalSearch0B((long)30000003,  1, 0.85, 0.65, 0.45, 1000),
                /*
                new ReactiveRandomLocalSearch69((long)30002020, 0.85, 0.70, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.85, 0.60, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.85, 0.50, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.85, 0.40, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.75, 0.60, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.75, 0.50, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.75, 0.40, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.65, 0.50, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.65, 0.40, 1000),
                new ReactiveRandomLocalSearch69((long)30002020, 0.55, 0.40, 1000),
                //new TabuSearch2(10000, 50),
                //new SimulatedAnnealing(5560, 0.96039),
                //new HierarchicalRandomLocalSearch01((long)30000000, 0.65, 0.40, 0.25),
                /*
                new RandomLocalSearch(1000, 1, long.MaxValue - 1),
                new RandomLocalSearch(1000, 2, long.MaxValue - 1),
                new RandomLocalSearch(1000, 6, long.MaxValue - 1),
                new RandomLocalSearch(1000, 56, long.MaxValue - 1),
                new RandomLocalSearch(1000, 167, long.MaxValue - 1),
                new RandomLocalSearch(1000, 278, long.MaxValue - 1),
                new RandomLocalSearch(1000, 557, long.MaxValue - 1),
                new RandomLocalSearch(1000, 835, long.MaxValue - 1),
                new RandomLocalSearch(1000, 1113, long.MaxValue - 1),
                new RandomLocalSearch(1000, 1670, long.MaxValue - 1),
                new RandomLocalSearch(1000, 2226, long.MaxValue - 1),
                new RandomLocalSearch(1000, 3339, long.MaxValue - 1),
                new RandomLocalSearch(1000, 4452, long.MaxValue - 1),
                new RandomLocalSearch(1000, 5565, long.MaxValue - 1),
                //new RandomWalk(10000),
                //new ReactiveRandomLocalSearch6((long)30000000, 0.75, 0.30, 3000),
                //new RandomLocalSearch(2000, 128 + 64 + 32, 0),
                //new RandomLocalSearch(2000, 512 + 256, 0),
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
                new RandomLocalSearch(20
                //new ReactiveStochasticSearch01((long)30000000, 0.65, 0.3),
                //new ReactiveStochasticSearch06((long)30000000, 0.65, 0.3, 3),
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
                //
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
                Console.WriteLine(p.OperationSet().Count());
                ISolution[] s_init = new ISolution[itr];
                ISolution[] s_lmin = new ISolution[itr];

                for (int i = 0; i < itr; i++)
                {
                    //s_init[i] = new LocalSearch().Solve(p, p.CreateRandomSolution());
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
                    //Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}  {5}", p.Optimum.Value, min, sum / itr, max, a.ToString(), Timer.Time());
                }
                //Console.WriteLine();
            }

            //Console.WriteLine("End.");
        }

        static void PhaseTest()
        {
            DataStoringWriter w = null;
            IOptimizationProblem p = new TravelingSalesmanProblem("ch150");
            ISolution s_init = new IterativeSearch(new LocalSearch(), 100).Solve(p, p.CreateRandomSolution());

            for (int start = 1; start <= 8; start++)
            {
                for (int i = start; i < p.OperationSet().Count(); i += 8)
                {
                    IOptimizationAlgorithm a = new RandomLocalSearch(1000, i, long.MaxValue - 1);
                    ISolution s = null;
                    w = new DataStoringWriter(root + p.Name + @"\" + a.ToString());
                    w.Open();
                    s = a.Solve(p, s_init, w);
                    string delta = "000000" + (s.Value);
                    w.Close(s.Value + "_" + delta.Substring(delta.Length - 6, 6));
                }
            }
        }
    }
}
