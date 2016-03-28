using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class Diameter
    {
        private ISolution s1;
        private ISolution s2;
        private int diameter;
        private ISolution c;
        private int radius;
        private int inmeter;

        public int Inmeter
        {
            get { return inmeter; }
            set { inmeter = value; }
        }

        public int DiameterValue
        {
            get
            {
                return this.diameter;
            }
        }

        public int RadiusValue
        {
            get
            {
                return this.radius;
            }
        }

        public ISolution Solution1
        {
            get
            {
                return this.s1;
            }
        }

        public ISolution Solution2
        {
            get
            {
                return this.s2;
            }
        }
        public ISolution Center
        {
            get
            {
                return this.c;
            }
        }

        public int DistanceSum(ISolution x)
        {
            return x.DistanceTo(s1) + x.DistanceTo(s2);
        }

        public int Distance
        {
            get
            {
                return this.diameter;
            }
        }
        public static Diameter CalculateDiameter(List<ISolution> ss)
        {
            return CalculateDiameter(ss, ss.Count);
        }

        public static Diameter CalculateDiameter(List<ISolution> ss, int regionSize)
        {
            int distMax = 0;
            int distMin = int.MaxValue;
            ISolution far1 = null;
            ISolution far2 = null;
            
            for (int i = Math.Max(0, ss.Count - regionSize); i < ss.Count; i++)
            {
                ISolution s = ss.ElementAt(i);
                for (int j = i + 1; j < ss.Count; j++)
                {
                    int distTmp = s.DistanceTo(ss.ElementAt(j));
                    if (distMax <= distTmp)
                    {
                        far1 = ss.ElementAt(i);
                        far2 = ss.ElementAt(j);
                        distMax = distTmp;
                    }
                    
                    if (distTmp <= distMin)
                    {
                        distMin = distTmp;
                    }
                }
            }
            ISolution c = null;
            int radMin = int.MaxValue;
            
            /*
            
            for (int i = Math.Max(0, ss.Count - regionSize); i < ss.Count - 1; i++)
            {
                int radMax = int.MinValue;

                for (int j = Math.Max(0, ss.Count - regionSize); j < ss.Count - 1; j++)
                {
                    radMax = Math.Max(radMax, ss.ElementAt(i).DistanceTo(ss.ElementAt(j)));
                }

                if(radMax < radMin)
                {
                    c = ss.ElementAt(i);
                    radMin = radMax;
                }
            }
            */

            return new Diameter(far1, far2, distMax, distMin, c, radMin);
        }

        public Diameter(ISolution s1, ISolution s2)
            : this(s1, s2, s1.DistanceTo(s2), s1.DistanceTo(s2))
        {
        }

        public Diameter(ISolution s1, ISolution s2, int diameter, int inmeter)
            : this(s1, s2, diameter, inmeter, null, -1)
        {
            this.s1 = s1;
            this.s2 = s2;
            this.diameter = diameter;
            this.c = null;
        }

        public Diameter(ISolution s1, ISolution s2, int diameter, int inmeter, ISolution c, int radius)
        {
            this.s1 = s1;
            this.s2 = s2;
            this.diameter = diameter;
            this.inmeter = inmeter;
            this.c = c;
            this.radius = radius;
        }
    }
}
