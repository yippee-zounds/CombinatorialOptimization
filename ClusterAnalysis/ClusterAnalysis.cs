using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterAnalysis
{
    class ClusterAnalysis
    {
        //private Func<double> f;

        /* 
        public static double NearstNeighbor(Cluster c1, Cluster c2)
        {
            return c1.SelectMany(x => c2,
                (x, y) => x.DistanceTo(y)).Min();
        }

        public static double FurthestNeighbor(Cluster c1, Cluster c2)
        {
            return c1.SelectMany(x => c2,
                (x, y) => x.DistanceTo(y)).Max();
        }

        public static double GroupAverage(Cluster c1, Cluster c2)
        {
            return c1.SelectMany(x => c2,
                (x, y) => x.DistanceTo(y)).Average();
        }
        */
        public static double Ward(Cluster c1, Cluster c2)
        {
            return -1;
        }
    }
}
