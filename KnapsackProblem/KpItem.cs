using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drace.KnapsackProblem
{
    public class KpItem
    {
        private int value;

        public int Value
        {
            get { return this.value; }
        }
        private int weight;

        public int Weight
        {
            get { return weight; }
        }



        public KpItem(int value, int weight)
        {
            this.value = value;
            this.weight = weight;
        }
    }
}
