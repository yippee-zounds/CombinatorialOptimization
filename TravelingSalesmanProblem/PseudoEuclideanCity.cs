using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.TravelingSalesmanProblem
{
    class PseudoEuclideanCity : ICity
    {
        private int id;
        private int x;
        private int y;

        public int DistanceTo(ICity city)
        {
            PseudoEuclideanCity c = (PseudoEuclideanCity)city;
            double r = (Math.Sqrt(((x - c.x) * (x - c.x) + (y - c.y) * (y - c.y)) / 10.0));
            int t = (int)(r + 0.5);

            if (t < r) return t + 1;
            else return t;  
        }

        public int ID
        {
            get { return id; }
        }

        public int Y
        {
            get { return y; }
        }

        public int X
        {
            get { return x; }
        }

        public PseudoEuclideanCity(int id, int x, int y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }
    }
}
