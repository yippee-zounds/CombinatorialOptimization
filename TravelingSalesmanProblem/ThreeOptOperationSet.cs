using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class ThreeOptOperationSet : IOperationSet
    {
        int Size = 0;

        public IEnumerator<IOperation> GetEnumerator()
        {
            int count = 0;
            
            for (int i = 1; i < this.Size - 2; i++)
            {
                for (int j = i + 1; j < this.Size - 1; j++)
                {
                    for (int k = j + 1; k < this.Size - 0; j++)
                    {
                        count++;
                        yield return new ThreeOptOperation(this.Size, i, j, k, 0);

                        count++;
                        yield return new ThreeOptOperation(this.Size, i, j, k, 1);
                    }
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public ThreeOptOperationSet(TravelingSalesmanProblem tsp)
        {
            this.Size = tsp.Size;
        }    }
}
