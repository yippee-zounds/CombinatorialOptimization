using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterAnalysis
{
    public class Cluster : IMetric// : IEnumerable<IMetric>
    {
        private IMetric atomicElt;
        private List<Cluster> cs;
        private bool isAtomic;

        public IEnumerable<IMetric> GetEnumerator()
        {
            //yield return null;
            //yield return (T)this.atomicElt;
            
            if (this.isAtomic)
            {
                yield return (IMetric)this.atomicElt;
            }
            else
            {
                yield return (IMetric)this.atomicElt;
                /*
                foreach (var cluster in elt)
                {
                    yield return (T)cluster;
                }*/
            }
        }

        public double DistanceTo(IMetric m)
        {
            return -1;
        }

        public Cluster AddCluster(Cluster c)
        {
            this.cs.Add(c);
            return this;
        }

        public Cluster(Cluster c)
        {
            this.cs = new List<Cluster>();
            this.cs.Add(c);
            this.isAtomic = false;
        }

        public Cluster(IMetric m)
        {
            this.atomicElt = m;
            this.isAtomic = true;
        }
    }
}
