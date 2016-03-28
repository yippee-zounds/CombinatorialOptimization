using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Drace.Optimization.FSSP
{
    public class Fssp
    {
        private int[][] time;
        private string name;

        public int Evaluate(int[] x)
        {
            int[] time_end = new int[time[0].Length];
            int xLength = x.Length - 1;
            int tLength = time[0].Length;

            for (int j = 0; j < time_end.Length; j++) {
                time_end[j] = time[x[0 + 1]][j];
                if (j != 0) {
                    time_end[j] += time_end[j - 1];
                }
            }

            for (int i = 1; i < xLength; i++) {
                for (int j = 0; j < tLength; j++) {
                    if (j == 0) {
                        time_end[j] = time[x[i + 1]][j] + time_end[j];
                    }
                    else {
                        time_end[j] = time[x[i + 1]][j] + Math.Max(time_end[j], time_end[j - 1]);
                    }
                }
            }

            return time_end[time_end.Length - 1];
        }

        private int[] Scan(string line) {
            string[] buf = line.Split(new char[] { '\t', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            return buf.Select((t) => int.Parse(t)).ToArray();
        }

        public Fssp(string path) {
            this.time = File.ReadLines("../../../" + path + ".txt").Skip(1).Select(line => Scan(line)).ToArray();

            string name = new FileInfo("../../../" + path + ".txt").Name;
            int index = name.LastIndexOf('.');
            this.name = name.Substring(0, index);
        }
    }
}
