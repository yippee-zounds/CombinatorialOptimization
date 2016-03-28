using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class NullWriter : DataStoringWriter
    {
        public override void Open() { }
        public override void Close(int value) { }
        public override void Close(string fileName) { }
        public override void Write(string s) { }
        public override void WriteLine(string s) { }
        public override string CreateTemporaryFilePath(string path) { return ""; }
        public override string Path
        {
            get{
                return "";
            }
        }

        public NullWriter() : base("")
        {
            
        }
    }
}
