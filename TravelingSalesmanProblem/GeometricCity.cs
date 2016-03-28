using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.TravelingSalesmanProblem
{
    class GeographicalCity : ICity
    {
        private int id;
        private double x;
        private double y;
        private const double RRR = 6378.388;
        public int DistanceTo(ICity city)
        {
            GeographicalCity c = (GeographicalCity)city;
            double q1 = Math.Cos(calcTude(y) - calcTude(c.y));
            double q2 = Math.Cos(calcTude(x) - calcTude(c.x));
            double q3 = Math.Cos(calcTude(x) + calcTude(c.x));

            double r = 0.5 * ((1.0 + q1) * q2 - (1.0 - q1) * q3);
            double ret = RRR * Math.Acos(r) + 1.0;
            return (int)ret;
        }

        private double calcTude(double x)
        {
            double PI = 3.141592;
            double deg = (int)(x + 0.0);
            double min = x - deg;
            return PI * (deg + 5.0 * min / 3.0) / 180.0;
        }

        public int ID
        {
            get { return id; }
        }

        public double Y
        {
            get { return y; }
        }

        public double X
        {
            get { return x; }
        }

        public GeographicalCity(int id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }
    }
}
