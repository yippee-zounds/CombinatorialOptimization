﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.OptimizationLibrary
{
    public interface IMetric
    {
        int DistanceTo(IMetric m);
    }
}
