using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Drace.OptimizationLibrary;

namespace Drace.KnapsackProblem
{
    class Program
    {
        static string path = "../../../kp/";
        static void Main(string[] args)
        {
            int[] size = new int[] {10, 100, 500, 1000 };

            for(int i = 0; i < size.Length; i++)
            {
                //GenerateProblem(size[i]);
                SolveProblem(new KnapsackProblem(size[i]));
            }
            /*
            string[] token = Console.ReadLine().Split(new char[] { '\t', ' ', ':' });

            if (token.Length < 2)
            {
                Console.WriteLine("USAGE : ksp 'gen' bits or ksp 'solve' filename");
            }
            
            if (token[0] == "gen")
            {
                GenerateProblem(int.Parse(token[1]));
            }
            else if(token[0] == "solve")
            {
                SolveProblem(new KnapsackProblem(int.Parse(token[1])));
            }*/

        }

        static void GenerateProblem(int size)
        {
            string fname = path + "kp" + size + ".dat";
            int[] a = new int[size];
            int sum = 0;

            for(int i = 0; i < size; i++)
            {
                a[i] = (int)(1 - 100 * Math.Log(StrictRandom.Next()) / Math.Log(2));
                sum += a[i];
            }

            File.WriteAllText(fname, size + "  " + ((int)(sum / 4)).ToString() + "\n");
            File.AppendAllText(fname, string.Join("  ", a.Select((x) => -(int)(10 * x + 10 * StrictRandom.Next()))) + "\n");
            File.AppendAllText(fname, string.Join("  ", a.Select((x) => x)));
        }

        static void SolveProblem(KnapsackProblem kp)
        {/*
            int n = kp.Size;
            int[] c = ParallelEnumerable.Range(0, n).Select((m) => -kp[m].Value).ToArray();
            int C = 356028 * 2;
            int[] w = ParallelEnumerable.Range(0, n).Select((m) => kp[m].Weight).ToArray();
            int W = kp.Limit;
            int[,] s = new int[n + 1, C + 1];
            int[,] x = new int[n + 1, C + 1];
            x[0, 0] = 0;
            for (int k = 0; k < x.GetLength(1); k++) x[0, k] = int.MaxValue / 2;

            for (int j = 1; j < x.GetLength(0); j++)
            {
                for (int k = 0; k < x.GetLength(1); k++)
                {
                    s[j, k] = 0;
                    x[j, k] = x[j - 1, k];
                }                

                for (int k = c[j - 1]; k < x.GetLength(1); k++)
                {
                    if(x[j - 1, k - c[j - 1]] + w[j - 1] <= Math.Min(W, x[j, k]))
                    {
                        x[j, k] = x[j - 1, k - c[j - 1]];
                        s[j, k] = 1;
                    }
                }
            }

            int[] optbit = new int[n];
            int K = 0;
            for (int kk = 1; kk < x.GetLength(1); kk++ )
            {
                if (x[n, kk] == int.MaxValue) K = kk - 1;
            }

            for (int j = kp.Size; 0 < j; j--)
            {
                if (s[j, K] == 1)
                {
                    optbit[j - 1] = 1;
                    K = K - c[j - 1];
                }
                else
                {
                    optbit[j - 1] = 0;
                }

            }
            */
            
            int[] optbit = new int[kp.Size];
            int[] c_old = new int[kp.Limit + 1];
            int[] c_now = new int[kp.Limit + 1];
            int[,] s = new int[kp.Size + 1, kp.Limit + 1];
            long s_count = 0;

            for (int i = 0; i <= kp.Size; i++)
            {
                for (int w = 0; w < c_now.Length; w++)
                {
                    if (i == 0)
                    {
                        c_now[w] = 0;
                        s[i, w] = 1;
                        ++s_count;
                    }
                    else if (w == 0)
                    {
                        c_now[w] = 0;
                        s[i, w] = 0;
                    }
                    else
                    {
                        KpItem t = kp[i - 1];
                        int tmp = 0;
                        if (t.Weight <= w)
                        {
                            tmp = c_old[w - t.Weight] + (-t.Value);
                        }

                        if (tmp < c_old[w])
                        {
                            c_now[w] = c_old[w];
                            s[i, w] = 0;
                        }
                        else
                        {
                            c_now[w] = tmp;
                            s[i, w] = 1;
                            ++s_count;
                        }
                    }
                }

                for(int w = 0; w < c_now.Length; w++)
                {
                    c_old[w] = c_now[w];
                    c_now[w] = 0;
                }
            }
            StringBuilder sb = new StringBuilder();
            for(int w = kp.Limit, i = s.GetLength(0) - 1; 0 < i; i--)
            {
                if(s[i, w] == 1)
                {
                    optbit[i - 1] = 1;
                    w -= kp[i - 1].Weight;
                }
                else
                {
                    optbit[i - 1] = 0;
                }
            }
            
            KpSolution opt = new KpSolution(kp, optbit);
            string fname = path + "kp" + kp.Size + ".opt";

            File.WriteAllText(fname,  kp.Size + "  " + opt.Value + "  " + opt.Weight + "\n");
            File.AppendAllText(fname, string.Join("  ", optbit));
            
        }
    }
}
