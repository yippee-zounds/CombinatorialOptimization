using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public class MinimumKeeper
    {
        private int minimum;

        public int Minimum {
            get
            {
                return this.minimum;
            }
        }

        public bool IsNotMinimumStrict(int value)
        {
            return !this.IsMinimumStrict(value);
        }

        public bool IsMinimumStrict(int value)
        {
            if (value < this.minimum)
            {
                this.minimum = value;
                return true;
            }
            return false;
        }

        public bool IsNotMinimum(int value)
        {
            return !this.IsMinimum(value);
        }

        public bool IsMinimum(int value)
        {
            if (value <= this.minimum)
            {
                this.minimum = value;
                return true;
            }
            return false;
        }

        public MinimumKeeper(int initialValue = int.MaxValue)
        {
            this.minimum = initialValue;    
        }
    }
}
