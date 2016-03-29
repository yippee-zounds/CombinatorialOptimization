using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization.Algorithm
{
    class TabuSearch : LocalSearch
    {
        TabuList tl;
        protected override IOperation calculateBestOperation(IOptimizationProblem p, ISolution x)
        {
            return p.OperationSet(x).Where((op) => tl.isNotTabu(x, op)).ArgMinStrict((op) => p.OperationValue(op, x));
        }

        public TabuSearch(int tabuLength)
        {
            this.tl = new TabuList(tabuLength);
        }

        private class TabuList {
            private int tabuLength;
            IList<int> list;

            public bool isNotTabu(ISolution x, IOperation op)
            {


                return true;
            }

            public TabuList(int tabuLength)
            {
                this.tabuLength = tabuLength;
                this.list = new List<int>();
            }
        }
    }
}
