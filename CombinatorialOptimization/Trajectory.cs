using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drace.OptimizationLibrary;

namespace CombinatorialOptimization
{
    class Trajectory
    {
        ISolution s;

        public ISolution Solution
        {
            get { return s; }
            set {
                s = value;
                if (s.Value < lbest.Value)
                {
                    lbest = s.Clone();
                }
            }
        }

        ISolution lbest;

        public ISolution Lbest
        {
            get {
                return lbest; 
            }
        }

        public Trajectory(ISolution s)
        {
            this.s = s.Clone();
            this.lbest = s.Clone();
        }
    }
}
