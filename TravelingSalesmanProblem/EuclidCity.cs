using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.TravelingSalesmanProblem
{
    [Serializable()]
    internal class EuclideanCity : ICity
    {
        private int id;
        private double x;
        private double y;

        public int DistanceTo(ICity city)
        {
            EuclideanCity c = (EuclideanCity)city;
            return (int)(Math.Sqrt((x - c.x) * (x - c.x) + (y - c.y) * (y - c.y)) + 0.5);
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

        public EuclideanCity(int id, double x, double y)
        {
            this.id = id;
            this.x = x;
            this.y = y;
        }
    }
}
