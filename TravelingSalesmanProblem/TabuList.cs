using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.TravelingSalesmanProblem
{
    public class TabuList
    {
        private Dictionary<TspOperation, int> list = new Dictionary<TspOperation, int>();
        private int tabuLength;

        public bool IsNotTabu(TspOperation op)
        {
            return true;
        }

        public void Add(TspOperation op, int loop)
        {
            this.list[op] = loop + tabuLength;
        }

        public TabuList(int tabuLength)
        {
            this.tabuLength = tabuLength;
        }
    }
}
