using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;
namespace CombinatorialOptimization
{
    class ReactiveTabuSearchItem
    {
        private ISolution s;
        private int loop;

        public int Loop
        {
            get { return loop; }
            set { loop = value; }
        }
        private int repetition;

        public int Repetition
        {
            get { return repetition; }
            set { repetition = value; }
        }

        public ISolution Solution
        {
            get { return s; }
        }

        public ReactiveTabuSearchItem(ISolution s, int loop)
        {
            this.s = s;
            this.loop = loop;
            this.repetition = 1;
        }
    }
}
