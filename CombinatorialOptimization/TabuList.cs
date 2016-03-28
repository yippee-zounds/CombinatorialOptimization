using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class TabuList
    {
        private Dictionary<int, int> list = new Dictionary<int, int>();
        private int tabuLength;

        public bool IsNotTabu(IOperation op, int loop)
        {
            int value;
            if (list.TryGetValue(op.GetHashCode(), out value))
            {
                return value < loop;
            }
            return true;
        }

        public void Add(IOperation op, int loop)
        {
            this.list[op.GetHashCode()] = loop + tabuLength;
        }

        public TabuList(int tabuLength)
        {
            this.tabuLength = tabuLength;
        }
    }
}
