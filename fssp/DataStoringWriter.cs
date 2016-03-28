using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Drace.OptimizationLibrary;

namespace Drace.Optimization.FSSP
{
    public class DataStoringWriter2
    {
        private readonly string drive;
        private string dir;
        private StreamWriter writer;
        private string tmpFilePath;
        
        public void Open()
        {
            System.IO.Directory.CreateDirectory(this.Path);
            tmpFilePath = CreateTemporaryFilePath(this.Path);
            writer = new StreamWriter(tmpFilePath);
        }

        public void Close(int value)
        {
            this.Close(value.ToString());
        }

        public void Close(string fileName)
        {
            writer.Close();
            string rnd = "00000000" + StrictRandom.Next(100000000);
            File.Move(tmpFilePath, Path + "/" + fileName + "_" + rnd.Substring(rnd.Length - 8, 8) + ".txt");
        }

        public void Write(string s)
        {
            writer.Write(s);
        }

        public void WriteLine(string s)
        {
            writer.WriteLine(s);
        }

        public string CreateTemporaryFilePath(string path)
        {
            string tmpPath = null;
            
            do
            {
                int seed = StrictRandom.Next(100000000);
                tmpPath = Path + "/" + seed + ".tmp";
            } while (File.Exists(tmpPath));

            return tmpPath;
        }

        public string Path
        {
            get {
                return this.drive + "/" + this.dir;
            }
        }

        public string Directory
        {
            get {
                return this.dir;
            }
            set {
                this.dir = value;
            }
        }

        public DataStoringWriter2(string drive)
        {
            this.drive = drive;
        }
    }
}
