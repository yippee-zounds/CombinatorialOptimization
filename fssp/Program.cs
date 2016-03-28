using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP {
    class Program {
        static string Fssp {
            get {
                return Environment.GetCommandLineArgs()[1];
            }
        }

        static string Method {
            get {
                return Environment.GetCommandLineArgs()[2];
            }
        }

        static int Loop {
            get {
                return int.Parse(Environment.GetCommandLineArgs()[3]);
            }
        }

        static int Param {
            get {
                return int.Parse(Environment.GetCommandLineArgs()[4]);
            }
        }

        static int Itr {
            get {
                return int.Parse(Environment.GetCommandLineArgs()[5]);
            }
        }

        static void Main(string[] args) {
            OptimizationMethodTest(new FlowShopSchedulingProblem(Fssp));
        }

        private static void OptimizationMethodTest(FlowShopSchedulingProblem fssp) {
            /*
            var opt = new KeyValuePair<FsspSolution, int>(fssp.Opt, fssp.Evaluate(fssp.Opt));
            var best = new KeyValuePair<FsspSolution, int>(null, int.MaxValue);
            DataStoringWriter dsw = new DataStoringWriter("C:");

            for (int i = 0; i < Itr; i++) {
                IOptimizationMethod om = null;

                switch (Method) {
                    case "hls":
                        om = new HigherLevelSearch(Loop, Param);
                        break;
                    case "ts" :
                        om = new TabuSearch(Loop, Param);
                        break;
                    case "ls" :
                        om = new LocalSearchF1();
                        break;
                }
                
                dsw.Directory = "data/fssp/" + fssp.Name + "/" + om.ToString();
                dsw.Open();

                FsspSolution x = om.Solve(fssp, dsw);
                int xValue = fssp.Evaluate(x);

                if (xValue < best.Value) {
                    best = new KeyValuePair<FsspSolution,int>(x, xValue);
                    Console.WriteLine("{0:D}\t{1:D}", best.Value, best.Value - opt.Value);
                }

                dsw.Close(xValue);
            }
             */
        }

        private static void EnumTest(FlowShopSchedulingProblem fssp) {
            FsspSolution best = null;
            int bestValue = int.MaxValue;
            int xValue;
            int count = 0;
            Permutation perm = new Permutation(fssp.NumberOfItems);

            FsspSolution x = new FsspSolution(fssp, perm.X);
            best = x;
            bestValue = fssp.Evaluate(best);

            do {
                perm.next();
                x = new FsspSolution(fssp, perm.X);
                xValue = fssp.Evaluate(x);

                Console.WriteLine("{0:D}:{{{1:D},{2:D}}},", count++, fssp.Evaluate(x), fssp.Optimum.DistanceTo(x));

                if (xValue < bestValue) {
                    best = x;
                    bestValue = xValue;
                }
            } while (perm.HasNext);
        }
    }
}
