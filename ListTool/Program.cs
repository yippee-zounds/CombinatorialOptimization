using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Drace.OptimizationLibrary;

namespace ListTool
{
    class Program
    {
        static string root = @"D:\Result\data\";
        static string domRoot = @"D:\Result\domain";

        static void Main(string[] args)
        {
#if DEBUG
            root = @"D:\Result\debug\";
#endif

            //sortQapItem("sko81");
            //sortKpItem("kp500");
            create();
            //createDomainSpace();
            //createSolutionSpaceStructure2();
            //createSolutionSpaceStructure();
            
            DirectoryInfo d = new DirectoryInfo(root);

            foreach (var f in d.EnumerateFiles("*.txt", SearchOption.AllDirectories))
            {
                textToList(f);
            }

            textToIntDiv();
        }

        static void sortQapItem(string name)
        {
            string path = "../../../qap/" + name + ".sln";
            IEnumerable<int> istrSln = new DicimalStream(File.ReadAllLines(path)).GetIntStream();
            int size = istrSln.First();
            int[] p = new int[size];
            List<Tuple<int, int>> lst = new List<Tuple<int, int>>();

            int n = 0;
            foreach (int d in istrSln.Skip(2))
            {
                p[n] = d - 1;
                lst.Add(new Tuple<int, int>(n, d - 1));
                ++n;
            }

            int[] index = lst.OrderBy((x) => x.Item2).Select((x) => x.Item1).ToArray();

            path = "../../../qap/" + name + ".dat";
            IEnumerable<int> istrDat = new DicimalStream(File.ReadAllLines(path)).GetIntStream();
            istrDat = new DicimalStream(File.ReadAllLines(path)).GetIntStream();

            size = istrDat.First();
            int[,] dist = new int[size, size];
            int[,] flow = new int[size, size];

            int c = 0;
            foreach (int d in istrDat.Skip(1).Take(size * size))
            {
                dist[c / size, c % size] = d;
                ++c;
            }

            c = 0;
            foreach (int f in istrDat.Skip(1 + size * size))
            {
                flow[c / size, c % size] = f;
                ++c;
            }

            int[,] newDist = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    //newDist[i, j] = dist[index[i], index[j]];
                    newDist[index[i], index[j]] = dist[i, j];

                }
            }

            List<string> slnOutput = new List<string>();
            slnOutput.Add(string.Join(" ", istrSln.Take(2)));
            slnOutput.Add(string.Join(" ", index.Select((i) => p[i] + 1)));
            File.WriteAllLines("../../../qap/" + name + "_sorted.sln", slnOutput);

            List<string> datOutput = new List<string>();
            datOutput.Add(string.Join(" ", istrDat.Take(1)));
            for (int i = 0; i < size; i++)
            {
                string tmp = "";
                for (int j = 0; j < size; j++)
                {
                    tmp = tmp + newDist[i, j] + " ";
                }
                datOutput.Add(tmp);
            }
            datOutput.Add("");
            for (int i = 0; i < size; i++)
            {
                string tmp = "";
                for (int j = 0; j < size; j++)
                {
                    tmp = tmp + flow[i, j] + " ";
                }
                datOutput.Add(tmp);
            }

            File.WriteAllLines("../../../qap/" + name + "_sorted.dat", datOutput);

        }
        
        static void sortKpItem(string name)
        {
            string path = @"..\..\..\kp";
            DirectoryInfo di = new DirectoryInfo(path);

            Console.Write(string.Join("\n", di.EnumerateFiles().Select((i) => i.Name)));

            string[] opt = null;
            string[] sol = null;
            List<Tuple<int, string>> lst = new List<Tuple<int, string>>();
            string[] dat = null;
            string[] val = null;
            string[] wei = null;
            
            foreach (var f in di.EnumerateFiles())
            {
                if (f.Name == name + ".opt")
                {
                    opt = File.ReadAllLines(f.FullName).ToArray();
                    sol = opt[1].Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                        
                    for (int i = 0; i < sol.Length; i++)
                    {
                        lst.Add(new Tuple<int, string>(i, sol[i]));
                    }
                }
                else if (f.Name == name + ".dat")
                {
                    dat = File.ReadAllLines(f.FullName).ToArray();
                    val = dat[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    wei = dat[2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            int[] index = lst.OrderByDescending((x) => x.Item2).Select((x) => x.Item1).ToArray();

            List<string> optOutput = new List<string>();
            optOutput.Add(opt[0]);
            optOutput.Add(string.Join("  ", index.Select((i) => sol[i])));
            File.WriteAllLines(path + "\\" + name + "_sorted.opt", optOutput);

            List<string> datOutput = new List<string>();
            datOutput.Add(dat[0]);
            datOutput.Add(string.Join("  ", index.Select((i) => val[i])));
            datOutput.Add(string.Join("  ", index.Select((i) => wei[i])));
            File.WriteAllLines(path + "\\" + name + "_sorted.dat", datOutput);

            System.Threading.Thread.Sleep(100000);
        }

        static void createDomainSpace()
        {
            foreach (var pd in new DirectoryInfo(root).EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                List<string> output = new List<string>();
                foreach (var d in new DirectoryInfo(pd.FullName).EnumerateDirectories("RandomWalk[*", SearchOption.AllDirectories))
                {
                    var data = new List<int>();
                    int loop = int.Parse(getParam(d.Name, "loop"));

                    //File.AppendAllText(d.FullName + "\\domainSpace.ds", "");

                    foreach (var f in d.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
                    {
                        Console.WriteLine(f.FullName);

                        var h = makeHeader(File.ReadAllLines(f.FullName).First());
                        string line = File.ReadAllLines(f.FullName).Last();

                        data.Add(int.Parse(getValue(line, h, "doptdom")));
                        
                    }

                    Console.WriteLine(pd.FullName + "\\domainSpace.ds");
                    output.Add("{" + loop + "," + data.Average() + "}");
                }

                File.WriteAllText(pd.FullName + "\\domainSpace.ds", "list={" + string.Join(",", output) + "};");
            }
        }

        static string getValue(string line, Dictionary<string, int> h, string key)
        {
            return line.Split(new char[] { ':', '\n' })[h[key]];
        }

        static Dictionary<string, int> makeHeader(string header)
        {
            var ret = new Dictionary<string, int>();
            int c = 0;
            foreach(var t in header.Split(new char[]{':', '\n'}))
            {
                ret[t] = c++;
            }

            return ret;
        }

        static void createSolutionSpaceStructure2()
        {
            foreach (var pd in new DirectoryInfo(root).EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                var data = new List<string>();
                var data2 = new List<string>();
                foreach (var d in new DirectoryInfo(pd.FullName).EnumerateDirectories("NonReactiveRandomLocalSearch[loop=6000,*", SearchOption.AllDirectories))
                {
                    var dic = new Dictionary<string, int>();
                    var doms = new List<int>();
                    var maxDm = new List<double>();
                    string tact = (getParam(d.Name, "tact") + "0000").Substring(0, 4);

                    foreach (var f in d.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
                    {
                        Console.WriteLine(f.Name);

                        bool firstLine = true;
                        maxDm.Add(double.Parse(File.ReadLines(f.FullName).Last().Split(':')[12]));
                        //data.Add("{" + tact + "," + File.ReadLines(f.FullName).Last().Split(':')[12] + "}");

                        foreach(var line in File.ReadAllLines(f.FullName))
                        {
                            if(!firstLine)
                            {
                                string dom = line.Split(':')[13];
                                if (!dic.ContainsKey(dom)) dic.Add(dom, 0);

                                ++dic[dom];
                            }

                            firstLine = false;
                        }

                        doms.Add(dic.Keys.Count());
                    }

                    if (0 < d.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly).Count())
                    {
                        data.Add("{" + tact + "," + maxDm.Average() + "}");
                        data2.Add("{" + tact + "," + (doms.Average() / 1000 / d.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly).Count()) + "}");
                    }
                }
                File.WriteAllText(pd.FullName + @"\dm.sss", "list={" + String.Join(",", data.OrderBy((x) => x).ToArray()) + "};\n");
                File.AppendAllText(pd.FullName + @"\dm.sss", "doms={" + String.Join(",", data2.OrderBy((x) => x).ToArray()) + "};");
            }
        }

        static string getParam(string s, string param)
        {
            Regex r = new Regex(param + @"=([^],]+)");
            Match m = r.Match(s);
            return m.Groups[1].Captures[0].Value;
        }

        static void createSolutionSpaceStructure()
        {
            foreach (var d in new DirectoryInfo(root).EnumerateDirectories("LocalSearch", SearchOption.AllDirectories))
            {
                var solDic = new Dictionary<int, int>();
                var domDic = new Dictionary<int, int>();
                var delDic = new Dictionary<int, int>();

                Console.Write(d.Name);

                foreach(var f in d.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly))
                {
                    Console.Write(".");

                    int del = getSubValue(f);
                    int dom = getValue(f);

                    if (!delDic.ContainsKey(del))
                    {
                        delDic.Add(del, 0);
                    }

                    if (!domDic.ContainsKey(dom))
                    {
                        domDic.Add(dom, 0);
                    }

                    if (!solDic.ContainsKey(dom + del))
                    {
                        solDic.Add(dom + del, 0);
                    }

                    ++solDic[dom + del];
                    ++delDic[del];
                    ++domDic[dom];
                }

                string domainFileName = d.FullName + "\\domain.sss";
                File.WriteAllText(domainFileName, "dom={" + string.Join(",", domDic.Keys.OrderBy((k) => k).Select((k) => "{" + k + "," + domDic[k] + "}")) + "};\n");
                File.AppendAllText(domainFileName, "sol={" + string.Join(",", solDic.Keys.OrderBy((k) => k).Select((k) => "{" + k + "," + solDic[k] + "}")) + "};\n");
                File.AppendAllText(domainFileName, "del={" + string.Join(",", delDic.Keys.OrderBy((k) => k).Select((k) => "{" + k + "," + delDic[k] + "}")) + "};\n");
                File.AppendAllText(domainFileName, "val={" + string.Join(",", domDic.Keys.OrderBy((k) => k).Select((k) => string.Join(",",ParallelEnumerable.Repeat(k, domDic[k])))) + "};\n");
            }
            
        }

        static void create()
        {
            var xDic = new Dictionary<string, Dictionary<string, List<int>>>();
            DirectoryInfo d = new DirectoryInfo(root);

            foreach (var f in d.EnumerateFiles("*.txt", SearchOption.AllDirectories))
            {
                Console.Write(f.Name);

                addData(xDic, f);

                Console.Write("\t" + f.Length);

                //textToList(f);

                Console.WriteLine();

            }
            createReport(xDic);
        }

        static void createReport(Dictionary<string, Dictionary<string, List<int>>> dic)
        {
            List<string> output = new List<string>();

            foreach(var mainKey in dic.Keys)
            {
                var tmp = new Dictionary<string, string>();

                foreach(var subKey in dic[mainKey].Keys)
                {
                    var data = dic[mainKey][subKey];
                    string avg = data.Average().ToString("F2");
                    string min = data.Min().ToString();
                    string max = data.Max().ToString();
                    string std = data.StandardDeviation().ToString("F2");

                    if (5 <= data.Count())
                    {
                        tmp.Add(avg + min + max + subKey, mainKey + "\t" + data.Count() + "\t" + min + "\t" + avg + "\t" + max + "\t" + std + "\t" + subKey);
                    }
                }

                foreach(var tmpKey in tmp.Keys.OrderBy((x) => x))
                {
                    output.Add(tmp[tmpKey]);
                }
                output.Add("");
            }
            File.WriteAllLines(root + "\\report.rpt", output);
        }

        static void addData(Dictionary<string, Dictionary<string, List<int>>> dic, FileInfo f)
        {
            //if(f.Length == 0) return;

            if(!dic.ContainsKey(getMainKey(f)))
            {
                dic.Add(getMainKey(f), new Dictionary<string, List<int>>());
            }

            var inDic = dic[getMainKey(f)];

            if(!inDic.ContainsKey(getSubKey(f)))
            {
                inDic.Add(getSubKey(f), new List<int>());
            }

            inDic[getSubKey(f)].Add(getValue(f));

        }

        static int getSubValue(FileInfo f)
        {
            string[] s = f.Name.Split('_').ToArray();
            string v = string.Join("", s[1].ToCharArray().SkipWhile((c) => c == '0'));
            if (v == "") v = "0";
            return Math.Abs(int.Parse(v));
        }

        static int getValue(FileInfo f)
        {
            return int.Parse(f.Name.Split('_').First());
        }

        static string getMainKey(FileInfo f)
        {
            return f.Directory.Parent.Name;
        }

        static string getSubKey(FileInfo f)
        {
            return f.Directory.Name;
        }

        static void textToIntDiv()
        {
            DirectoryInfo d = new DirectoryInfo(root);
            Dictionary<int, String> outputVx = new Dictionary<int, string>();
            Dictionary<int, String> outputVdom = new Dictionary<int, string>();
            Dictionary<int, String> outputVxAverage = new Dictionary<int, string>();
            Dictionary<int, String> outputVdomAverage = new Dictionary<int, string>();
            var dicVx = new Dictionary<string, List<double>>();
            var dicVdom = new Dictionary<string, List<double>>();
            var dicVxAverage = new Dictionary<string, List<double>>();
            var dicVdomAverage = new Dictionary<string, List<double>>();

            foreach (var f in d.EnumerateFiles("*.list", SearchOption.AllDirectories))
            {
                Console.Write(f.Name + "\t" + f.Directory.Name);

                foreach (var line in File.ReadAllLines(f.FullName))
                {
                    if (line.StartsWith("vx = "))
                    {
                        add(dicVx, f.Directory.Name, extractMin("vx", line));
                        add(dicVxAverage, f.Directory.Name, extractAverage("vx", line));
                    }
                    else if (line.StartsWith("vdom = "))
                    {
                        add(dicVdom, f.Directory.Name, extractMin("vdom", line));
                        add(dicVdomAverage, f.Directory.Name, extractAverage("vdom", line));
                    }
                }
            }

            Regex r = new Regex(@"subset=(?<subset>\d+)");
            foreach (var key in dicVx.Keys) {
                var result = r.Match(key);
                String s = result.Groups["subset"].Value;
                outputVx.Add(int.Parse(s), "{" + s + "," + dicVx[key].Average() + "}");
            }

            foreach (var key in dicVdom.Keys)
            {
                var result = r.Match(key);
                String s = result.Groups["subset"].Value;
                outputVdom.Add(int.Parse(s), "{" + s + "," + dicVdom[key].Average() + "}");
            }

            foreach (var key in dicVxAverage.Keys)
            {
                var result = r.Match(key);
                String s = result.Groups["subset"].Value;
                outputVxAverage.Add(int.Parse(s), "{" + s + "," + dicVxAverage[key].Average() + "}");
            }

            foreach (var key in dicVdomAverage.Keys)
            {
                var result = r.Match(key);
                String s = result.Groups["subset"].Value;
                outputVdomAverage.Add(int.Parse(s), "{" + s + "," + dicVdomAverage[key].Average() + "}");
            }

            File.WriteAllText(root + "v.rpt", "vx={" + String.Join(",", outputVx.Keys.OrderBy((k) => k).Select((k) => outputVx[k])) + "};\n");
            File.AppendAllText(root + "v.rpt", "vdom={" + String.Join(",", outputVdom.Keys.OrderBy((k) => k).Select((k) => outputVdom[k])) + "};\n");
            File.AppendAllText(root + "v.rpt", "vxA={" + String.Join(",", outputVxAverage.Keys.OrderBy((k) => k).Select((k) => outputVxAverage[k])) + "};\n");
            File.AppendAllText(root + "v.rpt", "vdomA={" + String.Join(",", outputVdomAverage.Keys.OrderBy((k) => k).Select((k) => outputVdomAverage[k])) + "};\n");
        }

        static double extractAverage(String match, String line)
        {
            return line.Substring(match.Length + 4, line.Length - match.Length - 4 - 2).Split(',').Skip(1000).Select((x) => int.Parse(x)).Average();
        }

        static double extractMin(String match, String line) {
            return line.Substring(match.Length + 4, line.Length - match.Length - 4 - 2).Split(',').Skip(1000).Select((x) => int.Parse(x)).Min();
        }

        static void add(Dictionary<string, List<double>> d, String key, double value)
        {
            if (!d.ContainsKey(key)) d.Add(key, new List<double>());
            d[key].Add(value);
        }

        static void textToList(FileInfo f)
        {
            if (f.Length < 1024) return;
            if (20 * 1024 * 1024 < f.Length) return;

            Console.Write(f.Name);
            var dic = new Dictionary<string, List<string>>();
            bool firstLine = true;
            bool containDomData = false;
            string newFullName = f.FullName.Replace(".txt", ".list");
            //int domCount = 0;
            //var tmpDic = new Dictionary<string, int>();

            if (!File.Exists(newFullName))
            {
                Console.Write(".");
                foreach (var line in File.ReadAllLines(f.FullName))
                {
                    int index = 0;
                    foreach (var token in line.Split(new char[] { ':' }))
                    {
                        if (firstLine)
                        {
                            dic.Add(token, new List<string>());
                        }
                        else
                        {
                            dic.ElementAt(index).Value.Add(token);
                        }
                        index++;
                    }
                    firstLine = false;
                }
                File.WriteAllLines(newFullName, dic.Keys.Select((k) => k + " = {" + string.Join(",", dic[k]) + "};"));
                if (dic.ContainsKey("dom"))
                {
                    containDomData = true;
                }

            }

            Console.Write("..");
            /*
            int skip = 0;
            string domFullName = f.FullName.Replace(".txt", ".dom");
            if(containDomData && !File.Exists(domFullName))
            {

                Console.Write("...");

                foreach(var key in dic.Keys)
                {
                    if (key == "x")
                    {
                        Dictionary<string, int> xDic = new Dictionary<string, int>();

                        foreach (var x in dic["x"].Skip(skip))
                        {
                            if (!xDic.ContainsKey(x))
                            {
                                xDic.Add(x, 0);
                            }
                            xDic[x]++;
                        }
                        File.AppendAllText(domFullName, xDic.Keys.Count.ToString() + "\n");
                    }
                    else if (key == "dom")
                    {
                        Dictionary<string, int> domDic = new Dictionary<string, int>();
                        var domCountList = new List<int>();
                        int loop = 0;
                        for (int i = 0; i < 100; i++) domCountList.Add(0);

                            foreach (var dom in dic["dom"].Skip(skip))
                            {
                                domCountList.Add(0);

                                if (!domDic.ContainsKey(dom))
                                {
                                    domDic.Add(dom, 0);

                                    for (int i = loop; i < Math.Min(loop + 100, domCountList.Count()); i++)
                                    {
                                        ++domCountList[i];
                                    }
                                }
                                domDic[dom]++;
                                ++loop;

                            }
                        int loopCount = 0;
                        var domRatioList = domCountList.Select((x) => (double)x / Math.Min(++loopCount, 100));

                        File.AppendAllText(domFullName, domDic.Keys.Count.ToString() + "\n");
                        File.AppendAllText(newFullName, "domRatio={" + string.Join(",", domRatioList.Select(x => x.ToString("F3"))) + "};\n");
                    }
                    else
                    {
                        var tmp = dic[key].Skip(skip);
                        File.AppendAllText(domFullName, key + "\t" + tmp.Average((v) => double.Parse(v)) + "\n");
                    }

                }
                

            }/**/

            Console.Write("....end");
        }
    }
}
