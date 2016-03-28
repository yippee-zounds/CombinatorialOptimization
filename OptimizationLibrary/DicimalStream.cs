using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class DicimalStream
    {
        private IEnumerable<string> stream;

        public IEnumerable<int> GetIntStream()
        {
            foreach(string line in stream)
            {
                foreach(string token in line.Split(new char[] { '\t', ' ', ':', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    yield return int.Parse(token);
                }
            }
        }

        public DicimalStream(string text)
        {
            this.stream = new string[1] { text };
        }

        public DicimalStream(IEnumerable<string> stream)
        {
            this.stream = stream;
        }
    }
}
