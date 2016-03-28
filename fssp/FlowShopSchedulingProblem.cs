using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP {
    public class FlowShopSchedulingProblem : IOptimizationProblem {
        private int[][] time;
        private FsspSolution opt;
        private string name;

        public int Size
        {
            get
            {
                return this.time.Length;
            }
        }

        public ISolution Optimum {
            get {
                return this.opt;
            }
        }

        public int NumberOfItems {
            get {
                return time.Length;
            }
        }

        public int NumberOfMachines {
            get {
                return this.time[0].Length;
            }
        }

        public string Name {
            get {
                return this.name;
            }
        }

        public int OperationValue(IOperation op, ISolution s)
        {
            int nowValue = s.Value;
            int nextValue = s.Apply(op).Value;
            s.Apply(op);
            return nextValue - nowValue;
        }

        public IEnumerable<IOperation> OperationSet()
        {
            for (int i = 0; i < this.NumberOfItems - 1; i++)
            {
                for (int j = i + 1; j < this.NumberOfItems - 0; j++)
                {
                    yield return new FsspOperation(this.NumberOfItems, i, j);
                }
            }
        }

        public ISolution CreateRandomSolution()
        {
            return FsspSolution.Random(this, this.NumberOfItems);
        }

        public int Evaluate(ISolution s) {
            FsspSolution x = (FsspSolution)s;
            int[] time_end = new int[time[0].Length];

            for (int j = 0; j < time_end.Length; j++) {
                time_end[j] = time[x[0]][j];
                if (j != 0) {
                    time_end[j] += time_end[j - 1];
                }
            }

            for (int i = 1; i < x.Size; i++) {
                for (int j = 0; j < time[x[i]].Length; j++) {
                    if (j == 0) {
                        time_end[j] = time[x[i]][j] + time_end[j];
                    }
                    else {
                        time_end[j] = time[x[i]][j] + Math.Max(time_end[j], time_end[j - 1]);
                    }
                }
            }

            return time_end[time_end.Length - 1];
        }

        public override string ToString() {
            StringBuilder buf = new StringBuilder();

            buf.Append("{");
            for (int i = 0; i < time.Length; i++) {
                if (i != 0) buf.Append(",");
                buf.Append("{");
                
                for (int j = 0; j < time[i].Length; j++) {
                    if (j != 0) buf.Append(",");
                    buf.Append(time[i][j]);
                }
                buf.Append("}");
            }
            buf.Append("};");

            return buf.ToString();
        }

        private int[] Scan(string line) {
            string[] buf = line.Split(new char[] { '\t', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            return buf.Select((t) => int.Parse(t)).ToArray();
        }

        public FlowShopSchedulingProblem(string path) {
            this.opt = new FsspSolution(this, Scan(File.ReadLines("../../../" + path + ".txt").First()));
            this.time = File.ReadLines("../../../" + path + ".txt").Skip(1).Select(line => Scan(line)).ToArray();

            string name = new FileInfo("../../../" + path + ".txt").Name;
            int index = name.LastIndexOf('.');
            this.name = name.Substring(0, index);

            //System.Console.WriteLine("item=" + this.NumberOfItems + ", machine=" + this.NumberOfMachines);
        }
    }
}
