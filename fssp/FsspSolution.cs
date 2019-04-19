using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP {
    class FsspSolution : ISolution {
        private int[] x;
        private int[] rev_x;
        private FlowShopSchedulingProblem p;

        public Boolean IsFeasible()
        {
            return true;
        }

        public int OperationDistance(IOperation op, ISolution[] s)
        {
            FsspOperation fop = (FsspOperation)op;
            int ret = 0;

            for (int k = 0; k < s.Length; k++)
            {
                FsspSolution fsp = (FsspSolution)s[k];

                ret += Math.Abs(x[fop.i] - fsp.x[fop.i]) - Math.Abs(x[fop.j] - fsp.x[fop.i]);
                ret += Math.Abs(x[fop.j] - fsp.x[fop.j]) - Math.Abs(x[fop.i] - fsp.x[fop.j]);
            }
            return ret;
            /*
            int[] pos = new int[x.Length];

            for (int i = 0; i < s.Length; i++)
            {
                pos[x[i]] = i;
            }

            for (int i = 0; i < s.Length; i++)
            {
                ret += Math.Abs(i - pos[((FsspSolution)s[i]).x[i]]);
            }

            throw new NotImplementedException();
             * */
        }

        public int DistanceTo(IMetric s)
        {
            FsspSolution t = (FsspSolution)s;
            int[] pos = new int[t.Size];
            int ret = 0;

            for (int i = 0; i < pos.Length; i++)
            {
                pos[t.x[i]] = i;
            }

            for (int i = 0; i < this.x.Length; i++)
            {
                ret += Math.Abs(i - pos[x[i]]);
            }

            return ret;
        }

        public ISolution Clone() {
            return new FsspSolution(p, x);
        }

        public FsspSolution(FlowShopSchedulingProblem p, IEnumerable<int> y) : this(p, y.ToArray()){
        }

        public FsspSolution(FlowShopSchedulingProblem p, int[] x)
        {
            this.p = p;
            this.x = (int[])x.Clone();// new int[x.Length];
            this.rev_x = this.x.Select((n, i) => new { n, i }).OrderBy(a => a.n).Select(a => a.i).ToArray();
        }

        public ISolution CloneApply(IOperation op)
        {
            FsspOperation fsspOp = (FsspOperation)op;
            return ((FsspSolution)this.Clone()).Swap(fsspOp.i, fsspOp.j);
        }

        public ISolution Apply(IOperation op)
        {
            FsspOperation fsspOp = (FsspOperation)op;
            return this.Swap(fsspOp.i, fsspOp.j);
        }

        public ISolution ReverseApply(IOperation op)
        {
            return Apply(op);
        }

        public int this[int i]
        {
            get {
                return x[i];
            }
        }

        public int Value
        {
            get { return p.Evaluate(this);}
        }

        public FsspSolution Swap(int i, int j) {
            int tmp = this.x[i];
            this.x[i] = this.x[j];
            this.x[j] = tmp;

            this.rev_x[x[i]] = i;
            this.rev_x[x[j]] = j;

            return this;
        }

        public int Size {
            get {
                return this.x.Length;
            }
        }

        public override string ToString() {
            return "{" + String.Join(",", x.Select(n => n.ToString())) + "};";
        }

        private FsspSolution(int size) {
            this.x = new int[size];
            this.rev_x = new int[size];
        }

        public static FsspSolution Random(FlowShopSchedulingProblem p, int size) {
            int[] x = Enumerable.Range(0, size).ToArray();

            for (int i = 0; i < x.Length - 1; i++) {
                int j = StrictRandom.Next(i, x.Length - 1);
                int tmp = x[i];
                x[i] = x[j];
                x[j] = tmp;
            }

            return new FsspSolution(p, x);
        }

        public int DistanceTo(FsspSolution x) {
            return this.rev_x.Zip(x.rev_x, (a, b) => Math.Abs(a - b)).Sum();
        }
    }
}
