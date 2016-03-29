using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.TravelingSalesmanProblem
{
    public interface ICity
    {
        int DistanceTo(ICity city);
        int ID { get;}
    }
}
