using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class TwoOptOperationSet : IOperationSet
    {
        int Size = 0;

        public IEnumerator<IOperation> GetEnumerator()
        {
            int count = 0;
            
            for (int i = 1; i < this.Size - 1; i++)
            {
                for (int j = i + 1; j < this.Size - 0; j++)
                {
                    count++;
                    yield return new TwoOptOperation(this.Size, i, j, null);
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public TwoOptOperationSet(TravelingSalesmanProblem tsp)
        {
            this.Size = tsp.Size;
        }
    }
}
