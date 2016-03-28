using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    class LinkedListTour : Tour
    {
        private LinkedList<LinkedListNode<EuclideanCity>> list;
        private int[,] branch;

        public override ISolution Clone()
        {
            throw new NotImplementedException();
        }

        public override ISolution CloneApply(IOperation op)
        {
            throw new NotImplementedException();
        }

        public override ISolution Apply(IOperation op)
        {
            return this.TwoOpt((TwoOptOperation) op);
        }

        public override ISolution ReverseApply(IOperation op)
        {
            return this.TwoOpt((TwoOptOperation)op);
        }

        public ISolution TwoOpt(TwoOptOperation op)
        {
            return this;
        }

        public int Size
        {
            get
            {
                return list.Count();
            }
        }

        public override int Value
        {
            get
            {
                int ret = branch[list.First().Value.ID, list.Last().Value.ID];
                ret += list.Take(this.Size - 1).Sum((i) => branch[i.Value.ID, i.Next.Value.ID]);

                return ret;
            }
        }

        public LinkedListTour(IEnumerable<EuclideanCity> e)
        {
            this.list = new LinkedList<LinkedListNode<EuclideanCity>>(e.Select((i) => new LinkedListNode<EuclideanCity>(i)));
            
            branch = new int[this.Size, this.Size];
            e.Select((i) => e.Select((j) => branch[i.ID, j.ID] = branch[j.ID, i.ID] = i.DistanceTo(j)));
        }
    }
}
