using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drace.OptimizationLibrary;

namespace Drace.KnapsackProblem
{
    public class KpSolution : ISolution
    {
        private KnapsackProblem kp;
        private int[] bit;
        private int value = int.MaxValue;
        private int weight = -1;
        private bool isFeasible = false;

        public int OperationDistance(IOperation op, ISolution[] s)
        {
            KpOperation kop = (KpOperation)op;
            int ret = 0;

            if(kop.j == -1)
            {
                if(this.bit[kop.i] == 0)
                {
                    ret += s.Sum((x) => ((KpSolution)x).bit[kop.i]);
                }
                else
                {
                    ret += s.Sum((x) => 1 - ((KpSolution)x).bit[kop.i]);
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
            return isFeasible;
        }

        public ISolution Apply(IOperation op)
        {
            KpOperation kop = (KpOperation)op;

            if (kop.j == -1)
            {
                this.bit[kop.i] = 1 - this.bit[kop.i];
            }
            else
            {
                this.bit[kop.i] = 1 - this.bit[kop.i];
                this.bit[kop.j] = 1 - this.bit[kop.j];
            }
            value = kp.Evaluate(this);
            weight = kp.EvaluateWeight(this);
            this.isFeasible = (weight <= kp.Limit);
            if(!this.isFeasible)
            {
                value += weight * 10;
            }

            return this;
        }

        public int DistanceTo(IMetric s)
        {
            int ret = 0;
            int[] p = ((KpSolution)s).bit;

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
            return new KpSolution(this.kp, (int[])this.bit.Clone()); ;

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
            get{
                return bit[index];
            }
        }

        public int Weight
        {
            get
            {
                if(weight < 0)
                {
                    value = kp.Evaluate(this);
                    weight = kp.EvaluateWeight(this);
                }
                return this.weight;
            }
        }
        public int Value
        {
            get
            {
                if (weight < 0)
                {
                    value = kp.Evaluate(this);
                    weight = kp.EvaluateWeight(this);
                }
                return this.value;
            }
        }
        public override string ToString()
        {
            return "{" + String.Join(",", bit.Select(n => n.ToString())) + "};";
        }

        public KpSolution(KnapsackProblem kp, int[] bit)
        {
            this.kp = kp;
            this.bit = bit;
        }
    }
}
