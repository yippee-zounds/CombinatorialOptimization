using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drace.OptimizationLibrary;

namespace Drace.KnapsackProblem
{
    public class BapSolution : ISolution
    {
        private BitArrayProblem bap;
        private int[] bit;
        private int value;

        public int OperationDistance(IOperation op, ISolution[] s)
        {
            BapOperation bop = (BapOperation)op;
            int ret = 0;

            if (bop.j == -1)
            {
                if (this.bit[bop.i] == 0)
                {
                    ret += s.Sum((x) => ((BapSolution)x).bit[bop.i]);
                }
                else
                {
                    ret += s.Sum((x) => 1 - ((BapSolution)x).bit[bop.i]);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }


        public bool IsFeasible()
        {
            return true;
        }

        public ISolution Apply(IOperation op)
        {
            BapOperation bop = (BapOperation)op;

            if (bop.j == -1)
            {
                this.bit[bop.i] = 1 - this.bit[bop.i];
            }
            else
            {
                this.bit[bop.i] = 1 - this.bit[bop.i];
                this.bit[bop.j] = 1 - this.bit[bop.j];
            }
            value = bap.Evaluate(this);
            
            return this;
        }

        public int DistanceTo(IMetric s)
        {
            int ret = 0;
            int[] p = ((BapSolution)s).bit;

            for (int i = 0; i < bit.Length; i++)
            {
                if (bit[i] != p[i])
                {
                    ++ret;
                }
            }

            return ret;
        }

        public ISolution Clone()
        {
            return new BapSolution(this.bap, (int[])this.bit.Clone()); ;

        }

        public ISolution CloneApply(IOperation op)
        {
            throw (new NotImplementedException());

        }
        public ISolution ReverseApply(IOperation op)
        {
            throw (new NotImplementedException());
        }

        public int this[int index]
        {
            get
            {
                return bit[index];
            }
        }
        
        public int Value
        {
            get
            {
                if (value < 0)
                {
                    value = bap.Evaluate(this);
                }
                return this.value;
            }
        }
        public override string ToString()
        {
            return "{" + String.Join(",", bit.Select(n => n.ToString())) + "};";
        }

        public BapSolution(BitArrayProblem bap, int[] bit)
        {
            this.bap = bap;
            this.bit = bit;
            this.value = -1;
        }
    }
}
