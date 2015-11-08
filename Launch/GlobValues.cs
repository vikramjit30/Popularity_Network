using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launch
{
    class GlobValues
    {
        public int eachClusterNodes { get; set; }
        public int nodes { get; set; }
        public int interEdges { get; set; }
        public int intraEdges { get; set; }
        public int clusters { get; set; }
        public int numberOfGraphs { get; set; }
        public double couplingStrength { get; set; }
        public double couplingProb { get; set; }
        public double runningTime { get; set; }
     }
}
