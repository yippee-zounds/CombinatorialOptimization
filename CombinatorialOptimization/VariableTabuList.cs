using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class VariableTabuList
    {
        private Dictionary<int, KeyValuePair<int, int>> list = new Dictionary<int, KeyValuePair<int, int>>();
        private int tabuLength;
        private double tabuLengthPremitive;

        public bool IsNotTabu(IOperation op, int loop)
        {
            if (list.ContainsKey(op.GetHashCode()))
            {
                return list[op.GetHashCode()].Key + tabuLength < loop;
            }

            return true;
        }

        public KeyValuePair<int, int> OperationToTabuElement(IOperation op, int loop)
        {
            if (IsNotTabu(op, loop))
            {
                return new KeyValuePair<int, int>(-1, int.MaxValue);
            }

            if (list.ContainsKey(op.GetHashCode()))
            {
                return list[op.GetHashCode()];
            }

            return new KeyValuePair<int, int>(-1, int.MaxValue);
        }
        
        public void Enlarge()
        {
            this.ChangeTabuLength(+0.01);
        }
        public void Ensmall()
        {
            this.ChangeTabuLength(-0.01);
        }

        public void ChangeTabuLength(double factor)
        {
            this.tabuLengthPremitive *= (1.00 + factor);
            this.tabuLength = Math.Max((int)this.tabuLengthPremitive, 1);
        }

        public void Add(IOperation op, int loop, int delta)
        {
            this.list[op.GetHashCode()] = new KeyValuePair<int, int>(loop, delta);
        }

        public VariableTabuList(int tabuLength)
        {
            this.tabuLength = tabuLength;
            this.tabuLengthPremitive = this.tabuLength;
        }
    }
}
