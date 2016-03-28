using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drace.OptimizationLibrary;

namespace Combinatorics
{
    public class Permutation
    {
        public int[] p;

        public static IEnumerable<Permutation> EnumerateAllPermutation(int size)
        {
            int[] a = new int[size + 1];
            for (int k = 0; k < a.Length; k++)
                a[k] = k - 1;
            Permutation perm = new Permutation(a);
            while (true)
            {
                int i = size - 1;
                while (a[i] >= a[i + 1]) i--;
                if (i == 0) break;
                int j = size;
                while (a[i] >= a[j]) j--;

                int tmp = a[i];
                a[i] = a[j];
                a[j] = tmp;
                i++;
                j = size;

                while (i < j)
                {
                    tmp = a[i];
                    a[i] = a[j];
                    a[j] = tmp;
                    i++;
                    j--;
                }
                yield return perm;
            }
        }

        public static Permutation CreateRandomPermutation(int size)
        {
            return new Permutation(ParallelEnumerable.Range(0, size).Shuffle().ToArray());
        }

        public override string ToString()
        {
            return String.Join(",", p);
        }

        public Permutation(int[] perm)
        {
            this.p = perm;
        }

        public Permutation(int size)
        {
            this.p = ParallelEnumerable.Range(0, size).ToArray();
        }
    }
}
