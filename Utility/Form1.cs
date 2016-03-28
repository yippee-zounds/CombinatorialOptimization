using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Utility
{
    public partial class Form1 : Form
    {
        private int count = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            button1.Enabled = false;

            //this.listBox1.Items.Add(d_ls.FullName);
            this.backgroundWorker1.RunWorkerAsync();

            /*
            foreach (var f in d.EnumerateFiles("*.txt", SearchOption.AllDirectories))
            {
                var dic = new Dictionary<string, List<string>>();
                bool firstLine = true;

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

                string newFullName = f.FullName.Replace(".txt", ".list");
                File.WriteAllLines(newFullName, dic.Keys.Where((k) => k != "x").Select((k) => k + " = {" + string.Join(",", dic[k]) + "};\n"));
            }
            */

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string root = @"E:\data\";
            DirectoryInfo d = new DirectoryInfo(root);

            foreach (var d_ls in d.EnumerateDirectories("LocalSearch", SearchOption.AllDirectories))
            {
                StreamWriter w = new StreamWriter(@"C:\data\" + d_ls.Parent.Name + ".txt");
                
                foreach (var f in d_ls.EnumerateFiles("*.txt", SearchOption.AllDirectories))
                {
                    this.listBox1.Items.Add(f.FullName);
                    if (100 < this.listBox1.Items.Count)
                    {
                        this.listBox1.Items.RemoveAt(0);
                    }

                    IEnumerable<string> lines = File.ReadLines(f.FullName);
                    string[] first = lines.ElementAt(1).Split(':');
                    string[] last = lines.Last().Split(':');
                    w.WriteLine(String.Join("\t", f.Name, last[1], int.Parse(first[1]) - int.Parse(last[1]), first[1], last[0], last[2], last[3]));
                }
                w.Close();
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Invalidate();
            this.Text = "" + count++;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string root = @"C:\data\";
            DirectoryInfo d = new DirectoryInfo(root);

            foreach (var f in d.EnumerateFiles("*.txt", SearchOption.AllDirectories))
            {
                this.listBox1.Items.Add(f.FullName);
                //IDictionary<string, int> dict = new Dictionary<string, int>();
                IDictionary<string, int[]> dict = new Dictionary<string, int[]>();

                foreach (var line in File.ReadLines(f.FullName))
                {
                    string[] elt = line.Split('\t');

                    if (!dict.ContainsKey(elt[6]))
                    {
                        dict.Add(elt[6], new int[]{int.Parse(elt[1]), 0});
                    }

                    dict[elt[6]][1]++;
                    /*
                    if(!dict.ContainsKey(elt[1] + "_" + elt[5]))
                    {
                        dict.Add(elt[1] + "_" + elt[5], 0);
                    }

                    dict[elt[1] + "_" + elt[5]]++;
                     */ 
                }

                //File.WriteAllLines(f.FullName.Replace(".txt", ".out"), dict.Select((x) => x.Key + "\t" + x.Value + "\n").OrderBy((x) => x));
                File.WriteAllLines(f.FullName.Replace(".txt", ".out"), dict.Select((x) => x.Value[0] + "\t" + x.Value[1] + "\n").OrderBy((x) => x));
                File.WriteAllText(f.FullName.Replace(".txt", ".list"), f.Name.Replace(".txt", "") + "={" + System.String.Join(",", dict.Select((x) => "{" + x.Value[0] + "," + x.Value[1] + "}").OrderBy((x) => x)) + "};");
            }
        }
    }
}
