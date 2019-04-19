using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Drace.OptimizationLibrary;

namespace Drace.QuadraticAssignmentProblem
{
    [Serializable()]
    public class QuadraticAssignmentProblem : IOptimizationProblem
    {
        private int[,] dist = null;
        private int[,] flow = null;
        private QapSolution opt = null;
        private string name;
        private int[, , ,] mat;
        private int[, ,] delDist;
        private int[, ,] delFlow;
        public double dev = 0;
        public double mean = 0;

        public int Flow(int i, int j)
        {
            return this.flow[i, j];
        }

        public int Size
        {
            get
            {
                return this.dist.GetLength(0);
            }
        }

        public ISolution Optimum {
            get {
                return this.opt;
            }
        }

        public int NumberOfPlaces {
            get {
                return dist.Length;
            }
        }

        public int NumberOfEquipments {
            get {
                return this.flow.Length;
            }
        }

        public string Name {
            get {
                return this.name;
            }
        }

        public int OperationValue(IOperation iop, ISolution s)
        {
            /*
            int nowValue = s.Value;
            int nextValue = s.Apply(iop).Value;
            s.Apply(iop);
            int tmp = nextValue - nowValue;

            /*
            QapSolution x = (QapSolution)s;
            int totalFlow = 0;

            for (int i = 0; i < x.Size; i++)
            {
                for (int j = 0; j < x.Size; j++)
                {
                    totalFlow += dist[i, j] * flow[x[i], x[j]];
                }
            }*/
            
            QapOperation op = (QapOperation)iop;
            int[] x = ((QapSolution)s).perm;
            int ret = 0;
            int size = this.Size;

            for (int k = 0; k < size; k++)
            {
                ret += (dist[op.i, k] - dist[op.j, k]) * (flow[x[op.j], x[k]] - flow[x[op.i], x[k]]); //delFlow[op.i, op.j, k] * delDist[x[op.i], x[op.j], x[k]]; 
            }


            int del = (dist[op.i, op.i] - dist[op.j, op.i]) * (flow[x[op.j], x[op.i]] - flow[x[op.i], x[op.i]]) + (dist[op.i, op.j] - dist[op.j, op.j]) * (flow[x[op.j], x[op.j]] - flow[x[op.i], x[op.j]]);
            //int del = delFlow[op.i, op.j, op.i] * delDist[x[op.i], x[op.j], x[op.i]] + delFlow[op.i, op.j, op.j] * delDist[x[op.i], x[op.j], x[op.j]];
            ret -= del;
            ret *= 2;/**/
            return ret;
            /*
            
            int nowValue = s.Value;
            int nextValue = s.Apply(iop).Value;
            s.Apply(iop);
            return nextValue - nowValue;/**/
        }

        public IEnumerable<IOperation> OperationSet()
        {
            for (int i = 0; i < this.flow.GetLength(0) - 1; i++)
            {
                for (int j = i + 1; j < this.flow.GetLength(0); j++)
                {
                    yield return new QapOperation(i, j);
                }
            }
        }

        public ISolution CreateRandomSolution()
        {
            return QapSolution.Random(this, this.Size);
        }

        public int Evaluate(ISolution s) {
            /*
            int[] x = ((QapSolution)s).perm;
            int totalFlow = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++) {
                for (int j = 0; j < n; j++) {
                    totalFlow += mat[i, j, x[i], x[j]];
                    //totalFlow += dist[i, j] * flow[x[i], x[j]];
                }
            }
            
            return totalFlow;
            /**/
            int[] x = ((QapSolution)s).perm;
            int totalFlow = 0;
            int min = x.Min();
            int max = x.Max();

            if(min !=0 || max == x.Length)
            {
                Console.WriteLine("ERROR!!");
            }

            for (int i = 0; i < x.Length; i++) {
                for (int j = 0; j < x.Length; j++) {
                    totalFlow += dist[i, j] * flow[x[i], x[j]];
                }
            }
            
            return totalFlow;/**/
        }

        public override string ToString() {
            return "qap";
        }

        private void Scan(string path) {
            IEnumerable<int> istr = new DicimalStream(File.ReadAllLines(path)).GetIntStream();

            int size = istr.First();
            this.dist = new int[size, size];
            this.flow = new int[size, size];

            int c = 0;
            foreach(int d in istr.Skip(1).Take(size * size))
            {
                this.dist[c / size, c % size] = d;
                ++c;
            }

            c = 0;
            foreach (int f in istr.Skip(1 + size * size))
            {
                this.flow[c / size, c % size] = f;
                ++c;
            }

            this.mat = new int[size, size, size, size];

            for(int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        for (int l = 0; l < size; l++)
                        {
                            mat[i, j, k, l] = dist[i, j] * flow[k, l];
                        }

                    }

                }

            }

            this.delDist = new int[size, size, size];
            this.delFlow = new int[size, size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    for (int k = 0; k < size; k++)
                    {
                        delDist[i, j, k] = dist[j, k] - dist[i, k];
                        delFlow[i, j, k] = flow[j, k] - flow[i, k];
                    }

                }

            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    mean += this.flow[i, j];
                }
            }
            mean /= size * size;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    dev += (this.flow[i, j] - mean) * (this.flow[i, j] - mean);
                }
            }
            //dev /= size * size;
        }

        private QapSolution ScanOpt(string path)
        {
            IEnumerable<int> istr = new DicimalStream(File.ReadAllLines(path)).GetIntStream();
            int size = istr.First();
            int[] p = new int[size];

            int n = 0;
            foreach (int d in istr.Skip(2))
            {
                p[n] = d - 1;
                ++n;
            }
            return new QapSolution(this, p);
        }

        public QuadraticAssignmentProblem(string path)
        {
            Scan("../../../qap/" + path + ".dat");
            this.opt = ScanOpt("../../../qap/" + path + ".sln");
            this.name = path;
        }
    }
}
