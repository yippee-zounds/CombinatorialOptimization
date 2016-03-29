using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Drace.OptimizationLibrary;

namespace Drace.TravelingSalesmanProblem
{
    public class TravelingSalesmanProblem : IOptimizationProblem
    {
        private int idCount;
        private ArrayTour opt;
        private string name;
        private int[][] distance;
        private ICity[] city;
        private string edgeType;

        public string EdgeType
        {
            get { return edgeType; }
        }
        //private Boolean threeOpt = false;

        public ISolution Optimum
        {
            get{
                return opt;
            }
        }

        public ISolution CreateRandomSolution()
        {
            return ArrayTour.Random(this);
        }

        public IEnumerable<IOperation> OperationSet(ISolution x)
        {
            int count = 0;
            for (int i = 1; i < this.Size - 1; i++)
            {
                for (int j = i + 1; j < this.Size - 0; j++)
                {
                    count++;
                    yield return new TwoOptOperation(this.Size, i, j, (ArrayTour)x);
                }
            }
        }

        public int OperationValue(IOperation op, ISolution s)
        {
            return TwoOptValue((TwoOptOperation)op, (ArrayTour)s);
        }

        public int TwoOptValue(TwoOptOperation op, ArrayTour t)
        {
            return distance[t[op.i - 1]][t[op.j]] + distance[t[op.i]][t[op.j + 1]] - distance[t[op.i - 1]][t[op.i]] - distance[t[op.j]][t[op.j + 1]];
        }

        public int Size
        {
            get { return this.city.Length; }
        }

        public int Evaluate(ISolution s)
        {
            ArrayTour t = (ArrayTour)s;
            int ret = 0;

            for (int i = 0; i < t.Length; i++)
            {
                ret += distance[t[i]][t[i + 1]];
            }
            return ret;
        }

        private int[] Scan(string line)
        {
            string[] buf = line.Split(new char[] { '\t', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            return buf.Select((t) => int.Parse(t)).ToArray();
        }
        private double[] ScanDouble(string line)
        {
            string[] buf = line.Split(new char[] { '\t', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            return buf.Select((t) => double.Parse(t)).ToArray();
        }

        private ICity ScanCity(string line)
        {
            
            if (this.edgeType == "EUC_2D")
            {
                double[] c = ScanDouble(line);
                return new EuclideanCity(idCount++, c[1], c[2]);
            }
            else if (this.edgeType == "GEO")
            {
                double[] c = ScanDouble(line);
                return new GeographicalCity(idCount++, c[1], c[2]);
            }
            else if (this.edgeType == "ATT")
            {
                int[] c = Scan(line);
                return new PseudoEuclideanCity(idCount++, c[1], c[2]);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public TravelingSalesmanProblem(string name)
        {
            Console.WriteLine(name);

            this.idCount = 0;
            this.name = name;
            this.edgeType = File.ReadLines("../../../data/" + this.name + ".tsp").First((s) => Regex.IsMatch(s, "^EDGE_WEIGHT_TYPE")).Split(new char[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries)[1];
            this.opt = new ArrayTour(this, File.ReadLines("../../../data/" + this.name + ".opt.tour").Where((s) => Regex.IsMatch(s, "^[0-9]")).Select((s) => int.Parse(s) - 1).ToArray());
            this.city = File.ReadLines("../../../data/" + this.name + ".tsp").Where((s) => Regex.IsMatch(s, "^[ 0-9]")).Select((s) => ScanCity(s)).ToArray();
            
            this.distance = new int[this.city.Length][];
            for (int i = 0; i < this.city.Length; i++)
            {
                this.distance[i] = this.city.Select((c) => this.city[i].DistanceTo(c)).ToArray();
            }
        }
    }
}
