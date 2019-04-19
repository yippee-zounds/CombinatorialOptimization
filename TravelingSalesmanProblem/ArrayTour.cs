using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    [Serializable()]
    public class ArrayTour : ISolution
    {
        private int[] p;
        private int tourLength = 0;
        private TravelingSalesmanProblem tsp;
        private int[,] mat;
        private string edgeType;

        public int OperationDistance(IOperation op, ISolution[] s)
        {
            TwoOptOperation top = (TwoOptOperation)op;
            int ret = 0;
            int b1x = Math.Max(p[top.i - 1], p[top.i]) * tsp.Size + Math.Min(p[top.i - 1], p[top.i]);
            int b2x = Math.Max(p[top.j], p[top.j + 1]) * tsp.Size + Math.Min(p[top.j], p[top.j + 1]);
            int a1x = Math.Max(p[top.i - 1], p[top.j]) * tsp.Size + Math.Min(p[top.i - 1], p[top.j]);
            int a2x = Math.Max(p[top.i], p[top.j + 1]) * tsp.Size + Math.Min(p[top.i], p[top.j + 1]);

            for (int i = 0; i < s.Length; i++)
            {
                ArrayTour at = (ArrayTour)s[i];
                int b1y = Math.Max(at.p[top.i - 1], at.p[top.i]) * tsp.Size + Math.Min(at.p[top.i - 1], at.p[top.i]);
                int b2y = Math.Max(at.p[top.j], at.p[top.j + 1]) * tsp.Size + Math.Min(at.p[top.j], at.p[top.j + 1]);

                if (b1x == b1y) ++ret;
                if (b1x == b2y) ++ret;
                if (b2x == b1y) ++ret;
                if (b2x == b2y) ++ret;

                if (a1x == b1y) --ret;
                if (a1x == b2y) --ret;
                if (a2x == b1y) --ret;
                if (a2x == b2y) --ret;
            }

            return ret;
        }

        public int DistanceTo(IMetric s)
        {
            ArrayTour t = (ArrayTour)s;
            int length = p.Length;
            int ret = 0;

            for (int i = 0; i < length - 1; i++)
            {
                ret += mat[t.p[i], t.p[i + 1]];
            }

            return ret;
        }

        public Boolean IsFeasible()
        {
            return true;
        }

        private void CreateMatrix()
        {
            int length = p.Length;
            mat = new int[length, length];
            
            for (int i = 0; i < length - 1; i++)
            {
                for (int j = 0; j < length - 1; j++)
                {
                    mat[i, j] = 1;
                }
            }

            for (int i = 0; i < length - 1; i++)
            {
                mat[p[i], p[i + 1]] = mat[p[i + 1], p[i]] = 0;
            }
        }

        public int this[int i]
        {
            get { return p[i]; }
        }

        public override string ToString()
        {
            return string.Join(",", p);

        }

        public ISolution Clone()
        {
            ArrayTour ret = (ArrayTour)this.MemberwiseClone();
            ret.tsp = this.tsp;
            ret.p = (int[])this.p.Clone();
               
            ret.mat = (int[,])this.mat.Clone();
            return ret;
        }

        public ISolution CloneApply(IOperation op)
        {
            return (ISolution)CloneTwoOpt((TwoOptOperation)op);
        }

        public ISolution Apply(IOperation op)
        {
            return (ISolution)TwoOpt((TwoOptOperation)op);
        }

        public ISolution ReverseApply(IOperation op)
        {
            return (ISolution)TwoOpt((TwoOptOperation)op);
        }

        public ArrayTour CloneTwoOpt(TwoOptOperation op)
        {
            return CloneTwoOpt(op.i, op.j);
        }

        public ArrayTour TwoOpt(TwoOptOperation op)
        {
            return TwoOpt(op.i, op.j);
        }

        public ArrayTour TwoOpt(int i, int j)
        {
            
            mat[p[i - 1], p[i]] = mat[p[i], p[i - 1]] = 1;
            mat[p[j + 1], p[j]] = mat[p[j], p[j + 1]] = 1;
            mat[p[i - 1], p[j]] = mat[p[j], p[i - 1]] = 0;
            mat[p[j + 1], p[i]] = mat[p[i], p[j + 1]] = 0;
            
            while (i < j)
            {
                int tmp = p[i];
                p[i] = p[j];
                p[j] = tmp;

                i++;
                j--;
            }

            if (p[p.Length - 2] < p[1])
                Array.Reverse(this.p);
            //this.CreateMatrix();
            tourLength = 0;
            return this;
        }

        public ArrayTour CloneTwoOpt(int i, int j)
        {
            int[] pTmp = (int[])p.Clone();

            while (i < j)
            {
                int tmp = pTmp[i];
                pTmp[i] = pTmp[j];
                pTmp[j] = tmp;

                i++;
                j--;
            }

            return new ArrayTour(this.tsp, pTmp);
        }

        public int Value
        {
            get
            {
                if (tourLength == 0)
                {
                    tourLength = tsp.Evaluate(this);
                }
                return tourLength;
            }
        }

        public static ArrayTour Random(TravelingSalesmanProblem tsp)
        {
            int[] p = Enumerable.Range(0, tsp.Size).ToArray();

            for (int i = 1; i < p.Length - 1; i++)
            {
                int j = StrictRandom.Next(i + 1, p.Length - 1);
                int tmp = p[i];
                p[i] = p[j];
                p[j] = tmp;
            }

            return new ArrayTour(tsp, p);
        }

        public int Length
        {
            get { return p.Length - 1; }
        }

        public override bool Equals(object obj)
        {
            ArrayTour a = (ArrayTour)obj;

            if(a.p.Length == this.p.Length)
            {
                for(int i = 0; i < this.p.Length; i++)
                {
                    if(a.p[i] != this.p[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public ArrayTour(TravelingSalesmanProblem tsp, int[] p)
        {
            this.tsp = tsp;
            
            if (p[0] == p[p.Length - 1])
            {
                this.p = new int[p.Length];
                Array.Copy(p, this.p, p.Length);
            }
            else
            {
                this.p = new int[p.Length + 1];
                Array.Copy(p, this.p, p.Length);
                this.p[this.p.Length - 1] = p[0];
            }
            
            if (p[p.Length - 2] < p[1])
                Array.Reverse(this.p);
            
            CreateMatrix();
        }
    }
}
