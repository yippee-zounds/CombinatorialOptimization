using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drace.OptimizationLibrary;

namespace Drace.QuadraticAssignmentProblem
{
    class QapSolution : ISolution
    {
        private QuadraticAssignmentProblem qap;
        public int[] perm;
        private int value = -1;

        public int OperationDistance(IOperation op, ISolution[] s)
        {
            QapOperation qop = (QapOperation)op;
            int ret = 0;

            for (int i = 0; i < s.Length; i++ )
            {
                QapSolution qs = (QapSolution)s[i];

                if (this.perm[qop.i] == qs.perm[qop.i])
                {
                    ret++;
                }
                if (this.perm[qop.j] == qs.perm[qop.j])
                {
                    ret++;
                }
                if (this.perm[qop.j] == qs.perm[qop.i])
                {
                    ret--;
                }
                if (this.perm[qop.i] == qs.perm[qop.j])
                {
                    ret--;
                }
            }

            return ret;
        }

        public Boolean IsFeasible()
        {
            return true;
        }

        public int this[int index]
        {
            get
            {
                return perm[index];
            }
        }

        public int Size
        {
            get
            {
                return this.perm.Length;
            }
        }

        public int DistanceTo(ISolution s)
        {
            int ret = 0;
            int[] p = ((QapSolution)s).perm;
            
            for (int i = 0; i < perm.Length; i++)
            {
                if(perm[i] != p[i])
                {
                    ++ret;
                }
            }
            /*
            double tmp = 0;
            for (int i = 0; i < perm.Length; i++)
            {
                for (int j = 0; j < perm.Length; j++)
                {
                    tmp += (qap.Flow(perm[i], perm[j]) - qap.mean) * (qap.Flow(p[i], p[j]) - qap.mean);
                    //tmp += qap.Flow(perm[i], perm[j]) * qap.Flow(p[i], p[j]);
                }
            }
            //tmp -= 3 * qap.mean;
            tmp /= qap.dev;
            ret = (int)(1000 * (1 - tmp));/**/
            return ret;
        }

        public int Value
        {
            get{    
                if(this.value < 0)
                {
                    value = qap.Evaluate(this);
                }
                return value;
            }
        }

        public ISolution Apply(IOperation op)
        {
            QapOperation qop = (QapOperation)op;
            int tmp = this.perm[qop.i];
            this.perm[qop.i] = this.perm[qop.j];
            this.perm[qop.j] = tmp;
            this.value = -1;

            return this;
        }

        public ISolution Clone()
        {
            return new QapSolution(this.qap, (int[])this.perm.Clone()); ;

        }

        public ISolution CloneApply(IOperation op)
        {
            throw (new NotImplementedException());

        }

        public ISolution ReverseApply(IOperation op)
        {
            throw (new NotImplementedException());

        }

        public override string ToString()
        {
            return "{" + String.Join(",", this.perm.Select((x) => x.ToString())) + "}";
        }

        public static ISolution Random(QuadraticAssignmentProblem qap, int size)
        {
            int[] s = new int[size];
            
            for(int i = 0; i < s.Length; i++)
            {
                s[i] = i;
            }

            return new QapSolution(qap, s.Shuffle().ToArray());
        }

        public QapSolution(QuadraticAssignmentProblem qap, int[] perm)
        {
            this.qap = qap;
            this.perm = perm;
        }
    }
}
