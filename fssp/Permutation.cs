using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.Optimization.FSSP {
    class Permutation {
        private int[] x;

        public int[] X
        {
            get { return this.x; }
        }

        public int[] next() {
            int n = this.x.Length - 1;
            int j_index = -1;

            for (int j = n - 1; 0 <= j && j_index < 0; j--) {
                if (this.x[j] < this.x[j + 1]) {
                    j_index = j;
                }
            }

            int k_index = Enumerable.Range(j_index + 1, n - j_index).FirstOrDefault(k => x[k] < x[j_index]);

            if (k_index == 0) {
                k_index = n + 1;
            }

            Swap(j_index, k_index - 1);

            int s = j_index + 1;
            int t = n;

            while (s < t) {
                Swap(s, t);
                s++;
                t--;
            }

            return this.x;
        }

        public bool HasNext {
            get {
                for (int i = 0; i < this.x.Length - 1; i++) {
                    if (this.x[i] < this.x[i + 1]) {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Swap(int i, int j) {
            int tmp = this.x[i];
            this.x[i] = this.x[j];
            this.x[j] = tmp;
        }

        public override string ToString() {
            return "{" + String.Join(",", x.Select(n => n.ToString())) + "};";
        }

        public Permutation(int length) {
            this.x = Enumerable.Range(0, length).ToArray();
        }
    }
}
