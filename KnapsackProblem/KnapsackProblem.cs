using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Drace.OptimizationLibrary;

namespace Drace.KnapsackProblem
{
    public class KnapsackProblem : IOptimizationProblem
    {
        private int size;
        private int limit;

        public int Limit
        {
            get { return limit; }
        }
        private string name;
        private int[] value;
        private int[] weight;
        private ISolution optimum;

        public int OperationValue(IOperation op, ISolution s)
        {
            KpOperation kop = (KpOperation)op;
            KpSolution ks = (KpSolution)s;
            int i = kop.i;
            int j = kop.j;
            int ii = 1 - 2 * ks[i];
            int dv = ii * value[i];
            int dw = ii * weight[i];

            if (j != -1)
            {
                int jj = 1 - 2 * ks[j];
                dv += ii * value[j];
                dw += jj * weight[j];
            }

            if (this.limit < ks.Weight + dw)
            {
                dv += (ks.Weight + dw) * 100;
                //dv = int.MaxValue;
            }

            return dv;
        }

        public int Evaluate(ISolution s)
        {
            KpSolution ks = (KpSolution)s;
            int tmpv = 0;
            int tmpw = 0;

            for(int i = 0; i < this.size; i++)
            {
                tmpv += ks[i] * value[i];
                tmpw += ks[i] * weight[i];
            }

            if(this.limit < tmpw)
            {
                tmpv += tmpw * 10;
            }

            return tmpv;
        }
        public int EvaluateWeight(ISolution s)
        {
            KpSolution ks = (KpSolution)s;
            int tmpw = 0;

            for (int i = 0; i < this.size; i++)
            {
                tmpw += ks[i] * weight[i];
            }

            return tmpw;
        }

        public KpItem this[int index]
        {
            get
            {
                return new KpItem(value[index], weight[index]);
            }
        }

        public int Size
        {
            get
            {
                return this.size;
            }
        }

        public ISolution Optimum
        {
            get
            {
                return this.optimum;
            }
        }

        public IEnumerable<IOperation> OperationSet()
        {
            for (int i = 0; i < this.size; i++)
            {
                yield return new KpOperation(i);
            }   
            /*
            for (int i = 0; i < this.size - 1; i++)
            {
                for (int j = i + 1; j < this.size; j++)
                {
                    yield return new KpOperation(i, j);
                }
            }/**/
        }

        public ISolution CreateRandomSolution()
        {
            int[] bit = new int[this.size];
            int tmp = 0;
            
            foreach(int n in ParallelEnumerable.Range(0, this.size).Shuffle())
            { 
                if(tmp + weight[n] <= limit)
                {
                    bit[n] = 1;
                    tmp += weight[n];
                }
                else
                {
                    bit[n] = 0;
                }
            }
            return new KpSolution(this, bit);
        }

        public void Scan(string path)
        {
            IEnumerable<int> istr = new DicimalStream(File.ReadAllLines(path)).GetIntStream();

            this.limit = istr.Skip(1).First();
            this.value = new int[this.size];
            this.weight = new int[this.size];

            int c = 0;
            foreach (int v in istr.Skip(2).Take(size))
            {
                this.value[c] = v;
                ++c;
            }

            c = 0;
            foreach (int w in istr.Skip(2 + size))
            {
                this.weight[c] = w;
                ++c;
            }
        }

        public KpSolution ScanOpt(string path)
        {
            IEnumerable<int> istr = new DicimalStream(File.ReadAllLines(path)).GetIntStream();

            int[] optbit = new int[this.size];

            int c = 0;
            foreach (int b in istr.Skip(3))
            {
                optbit[c] = b;
                ++c;
            }

            return new KpSolution(this, optbit);
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public KnapsackProblem(int size)
        {
            this.size = size;
            this.name = "kp" + size.ToString();
            Scan("../../../kp/" + this.name + ".dat");
            this.optimum = ScanOpt("../../../kp/" + this.name + ".opt");
        }
    }
}
