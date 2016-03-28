using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Drace.OptimizationLibrary
{
    public class Analizer
    {
        static public void createDomainMap(List<ISolution> ss, List<ISolution> doms, IOptimizationProblem p)
        {
            int i = 0;
            Diameter domdiam = Diameter.CalculateDiameter(doms, doms.Count());
            Diameter optdiam = new Diameter(p.Optimum, doms.ArgMin((x) => -p.Optimum.DistanceTo(x)));
            String dom = String.Join(",", ss.Select((s) => "{" + s.DistanceTo(domdiam.Solution1) + "," + s.DistanceTo(domdiam.Solution2) + "}"));
            String optdom = String.Join(",", ss.Select((s) => "{" + s.DistanceTo(optdiam.Solution1) + "," + s.DistanceTo(optdiam.Solution2) + "}"));
            //String optdom3D = String.Join(",", ss.Select((s) => "{" + s.DistanceTo(domdiam.Solution1) + "," + s.DistanceTo(domdiam.Solution2) + "," + s.DistanceTo(p.Optimum) + "}"));
            String optdom3D = String.Join(",", ss.Select((s) => "{" + (++i) + "," + s.DistanceTo(optdiam.Solution1) + "," + s.DistanceTo(optdiam.Solution2) + "}"));

            i = 0;
            String domdom = String.Join(",", doms.Select((s) => "{" + s.DistanceTo(domdiam.Solution1) + "," + s.DistanceTo(domdiam.Solution2) + "}"));
            String domoptdom = String.Join(",", doms.Select((s) => "{" + s.DistanceTo(optdiam.Solution1) + "," + s.DistanceTo(optdiam.Solution2) + "}"));
            String domoptdom3D = String.Join(",", doms.Select((s) => "{" + (++i) + "," + s.DistanceTo(optdiam.Solution1) + "," + s.DistanceTo(optdiam.Solution2) + "}"));
            File.WriteAllText(@"D:\Result\data\domainMap(" + p.ToString() + ").list", "domMap={" + dom + "};\n" + "optMap={" + optdom + "};\n" + "optMap3D={" + optdom3D + "};\n" + "domdomMap={" + domdom + "};\n" + "domOptMap={" + domoptdom + "};\n" + "domOptMap3D={" + domoptdom3D + "};\n");
        }
    }
}
