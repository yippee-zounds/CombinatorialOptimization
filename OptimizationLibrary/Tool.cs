using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Drace.OptimizationLibrary
{
    class Tool
    {
        static void Main(string[] args)
        {
            foreach (var problem in Directory
                .EnumerateFiles(@"I:\data", "*.txt", SearchOption.AllDirectories)
                .GroupBy(x => new FileInfo(x).Directory.Parent.Name))
            {
                Console.WriteLine("[{0}]", problem.Key);
                foreach (var algorithm in problem
                    .GroupBy(x => new FileInfo(x).Directory.Name, (key, val) => val)
                    .OrderBy(x => x.Select(y => GetResultValue(y)).Average()))
                {
                    IEnumerable<int> v = algorithm.Select(x => GetResultValue(x));
                    Console.WriteLine("  {0}\t{1,3:G}\t{2,8:G}\t{3,10:F2}\t{4,8:G}\t{5,6:F2}",
                        new FileInfo(algorithm.First()).Directory.Name,
                        v.Count(), v.Min(), v.Average(), v.Max(), v.StandardDeviation()
                        );
                }
                Console.WriteLine();
            }
            Console.ReadKey();
        }

        static int GetResultValue(string x)
        {
            return int.Parse(Regex.Match(x, "([0-9]+)_").Groups[1].Value);
        }
    }
}
