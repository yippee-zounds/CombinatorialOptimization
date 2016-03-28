using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Drace.OptimizationLibrary;

namespace Drace.KnapsackProblem
{
    public class BitArrayProblem : IOptimizationProblem
    {
        private int size;
        private int select;
        private string name;
        private ISolution optimum;

        public int OperationValue(IOperation op, ISolution s)
        {
            BapSolution bs = (BapSolution)s;
            BapOperation bop = (BapOperation)op;
            int tmpv = 0;
            int dv = 0;

            for (int i = 0; i < this.size; i++)
            {
                tmpv += bs[i];
            }

            if(tmpv <= select)
            {
                dv -= 1 - 2 * bs[bop.i];
            }
            else
            {
                dv += 1 - 2 * bs[bop.i];
            }

            if (bop.j != -1)
            {
                if (tmpv <= select)
                {
                    dv += 1 - 2 * bs[bop.j];
                }
                else
                {
                    dv -= 1 - 2 * bs[bop.j];
                }
            }

            return dv;
        }

        public int Evaluate(ISolution s)
        {
            BapSolution bs = (BapSolution)s;
            int tmpm = 0;
            int tmpv = 0;
            
            for (int i = 0; i < this.size; i++)
            {
                tmpv += bs[i];
            }

            for (int i = 0; i < 10; i++)
            {
                tmpm -= bs[i];
            }

            return Math.Abs(select - tmpv) + tmpm;
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
                yield return new BapOperation(i);
            }
            /*
            for (int i = 0; i < this.size - 1; i++)
            {
                for (int j = i + 1; j < this.size; j++)
                {
                    yield return new BapOperation(i, j);
                }
            }/**/
        }

        public ISolution CreateRandomSolution()
        {
            int[] bit = new int[this.size];

            foreach (int n in ParallelEnumerable.Range(0, this.size).Shuffle())
            {
                if (StrictRandom.Next() < 0.5)
                {
                    bit[n] = 1;
                }
                else
                {
                    bit[n] = 0;
                }
            }
            return new BapSolution(this, bit);
        }

        public BapSolution CreateOpt()
        {
            int[] optbit = new int[this.size];

            for (int i = 0; i < optbit.Length; i++ )
            {
                if(i < select)
                {
                    optbit[i] = 1;
                }
                else
                {
                    optbit[i] = 0;
                }
            }

            return new BapSolution(this, optbit);
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public BitArrayProblem(int size, int sel)
        {
            this.size = size;
            this.select = sel;
            this.name = "ba" + size.ToString();
            this.optimum = CreateOpt();
        }
    }
}
