using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Drace.Optimization.FSSP {
    class Diameter {
        private FsspSolution s1;
        private FsspSolution s2;
        private int diameter;

        public FsspSolution Solution1 {
            get {
                return this.s1;
            }
        }

        public FsspSolution Solution2 {
            get {
                return this.s2;
            }
        }

        public int DistanceSum(FsspSolution x) {
            return x.DistanceTo(s1) + x.DistanceTo(s2);
        }

        public int Distance {
            get {
                return this.diameter;
            }
        }

        public static Diameter CalculateDiameter(List<FsspSolution> ss, int regionSize) {
            int distMax = int.MinValue;
            FsspSolution far1 = null;
            FsspSolution far2 = null;
            
            for (int i = Math.Max(0, ss.Count - regionSize); i < ss.Count - 1; i++) {
                for (int j = i + 1; j < ss.Count - 1; j++) {
                    int distTmp = ss.ElementAt(i).DistanceTo(ss.ElementAt(j));
                    if (distMax <= distTmp) {
                        far1 = ss.ElementAt(i);
                        far2 = ss.ElementAt(j);
                        distMax = distTmp;
                    }
                }
            }
            return new Diameter(far1, far2, distMax);
        }

        public Diameter(FsspSolution s1, FsspSolution s2) : this(s1, s2, s1.DistanceTo(s2)) {
        }

        public Diameter(FsspSolution s1, FsspSolution s2, int diameter){
            this.s1 = s1;
            this.s2 = s2;
            this.diameter = diameter;
        }
    }
}
