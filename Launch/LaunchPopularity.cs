#region MONO/NET System libraries
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
#endregion

#region Much appreciated thirs party libraries
using MathNet.Numerics.Distributions;
#endregion

#region NETGen libraries
using NETGen.Core;
using NETGen.Visualization;
using NETGen.Layouts.FruchtermanReingold;
using NETGen.NetworkModels.Cluster;
using NETGen.Dynamics.Synchronization;
using System.IO;
using System.Reflection;
using System.Collections;

namespace Launch
{

    public class SemiNumericComparer : IComparer<string>
    {
        public int Compare(string s1, string s2)
        {
            if (IsNumeric(s1) && IsNumeric(s2))
            {
                if (Convert.ToInt32(s1) > Convert.ToInt32(s2)) return 1;
                if (Convert.ToInt32(s1) < Convert.ToInt32(s2)) return -1;
                if (Convert.ToInt32(s1) == Convert.ToInt32(s2)) return 0;
            }

            if (IsNumeric(s1) && !IsNumeric(s2))
                return -1;

            if (!IsNumeric(s1) && IsNumeric(s2))
                return 1;

            return string.Compare(s1, s2, true);
        }

        public static bool IsNumeric(object value)
        {
            try
            {
                int i = Convert.ToInt32(value.ToString());
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }


    static class LaunchPopularity
    {
        static Kuramoto sync;
        static Network pop;
        static Dictionary<int, double> _clusterOrder = new Dictionary<int, double>();
        static Dictionary<int, bool> pacemaker_mode = new Dictionary<int, bool>();
        static double runTime = 10d;
        
        static Random rnd = new Random();
        // To read the parameter.config files for a type of network with different modularity
        public static void read_parameters(String configFile, GlobValues glob)
        {
            string[] allLines;
            ArrayList list = new ArrayList();
           

            using (StreamReader sr = File.OpenText(configFile))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    allLines = s.Split(new[] { "=" }, StringSplitOptions.None);
                    list.Add(allLines[1]);
                }

                glob.eachClusterNodes = Int32.Parse(list[0].ToString());
                glob.interEdges = Int32.Parse(list[1].ToString());
                glob.intraEdges = Int32.Parse(list[2].ToString());
                glob.clusters = Int32.Parse(list[3].ToString());
                glob.numberOfGraphs = Int32.Parse(list[4].ToString());
                glob.couplingStrength = Double.Parse(list[5].ToString());
                glob.couplingProb = Double.Parse(list[6].ToString());
                glob.runningTime = Double.Parse(list[7].ToString());
                glob.nodes = glob.eachClusterNodes * glob.clusters; 
                runTime = glob.runningTime;
            }

        }

      
        // Method to create a network for popularity

        public static void creatPopularityNetwork(GlobValues glob, String networkFile, String membershipFile)
        {

            List<Int64> node1 = new List<Int64>();
            List<Int64> node2 = new List<Int64>();
            ArrayList list = new ArrayList();
            ArrayList list1 = new ArrayList();
            int totalNodes = glob.clusters * glob.eachClusterNodes;

            // check if the number of nodes are less than intraedges
            if (glob.intraEdges < glob.eachClusterNodes) { Console.WriteLine("Intra Edges should be higher than number of nodes..program exits now"); System.Environment.Exit(1); }
            System.IO.StreamWriter sw = System.IO.File.CreateText(membershipFile);
            int j = -1;
            // write the membership of nodes into the file
            // set the id and the cluster Ids for all the nodes

            Nodes node;
            for (int i = 0; i < totalNodes; i++)
            {
                node = new Nodes();
                node.id = i;
                if (i % glob.eachClusterNodes == 0) j++;
                node.clusterID = j;
                sw.WriteLine(j);
                list.Add(node);

            }
            sw.Close();


            // we assign an edge to each each node, first.
            int min, max, randValue;
            foreach (Nodes eachNode in list)
            {
                // Console.WriteLine(eachNode.id);
                do
                {
                    min = glob.eachClusterNodes * eachNode.clusterID;
                    max = glob.eachClusterNodes * (eachNode.clusterID + 1);
                    randValue = rnd.Next(min, max);
                } while (eachNode.id == randValue);

                // Add to Dictionary
                node1.Add(eachNode.id);
                node2.Add(randValue);
            }

          
            int leftoverEdges = glob.intraEdges - glob.eachClusterNodes;
            int count = 0, randValue1, randValue2;
            while (count < leftoverEdges)
            {
                for (int i = 0; i < glob.clusters; i++)
                {
                     min = glob.eachClusterNodes * (i);
                     max = glob.eachClusterNodes * (i + 1);
                     randValue1 = rnd.Next(min, max);
                     randValue2 = rnd.Next(min, max);
                     node1.Add(randValue1);
                     node2.Add(randValue2);

                }
                count++;
            }

            // Here we connect the interedges between nodes of different cluster
            int min1, max1, min2, max2;
            for (int i =0; i<glob.interEdges;i++)
            {
                int cluster1, cluster2;
                do
                {
                    cluster1 = rnd.Next(0, glob.clusters);
                    cluster2 = rnd.Next(0, glob.clusters);
                   
                } while (cluster1 == cluster2);

                min1 = glob.eachClusterNodes * (cluster1);
                max1 = glob.eachClusterNodes * (cluster1 + 1);

                min2 = glob.eachClusterNodes * (cluster2);
                max2 = glob.eachClusterNodes * (cluster2 + 1);

                randValue1 = rnd.Next(min1, max1);
                randValue2 = rnd.Next(min2, max2);

                node1.Add(randValue1);
                node2.Add(randValue2);
            }



            // Sort the node1 and node2 based on bubble sorting.
            /*
            try
            {
                int c, d,swap;
                for (c = 0; c < (node1.Count - 1); c++)
                {
                    for (d = 0; d < (node1.Count - c - 1); d++)
                    {
                        if (node1[d] > node1[d+1]) 
                        {
                            swap = (int)node1[d];
                            node1[d] = node1[d + 1];
                            node1[d + 1] = swap;

                            swap = (int)node2[d];
                            node2[d] = node2[d+1];
                            node2[d + 1] = swap;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
            */

            // Store the values into the network.edge file
            // create the edges between
            list.Clear();
            String temp, temp1;
            System.IO.StreamWriter nwriter = System.IO.File.CreateText(networkFile);
            for (int i = 0; i<node1.Count;i++)
            {
                temp = node1[i].ToString() + node2[i].ToString();
                
                if (!list.Contains(temp))
                {
                    temp1 = node2[i].ToString() + node1[i].ToString();
                    list.Add(temp);
                    list.Add(temp1);

                    if (node1[i] != node2[i])
                        nwriter.WriteLine(node1[i] + " " + node2[i]);

                }
            }
            nwriter.Close();


            // Clear all nodes array
            node1.Clear();
            node2.Clear();
           

        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        
        [STAThread]
        static void Main(string[] args)
        {
           
            string configFile, dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), netFile, destinationNetworkFile, memFile,
            destMembershipFile, resFile, destResultFile;
            string[] path = dir.Split(new string[] { "Launch" }, StringSplitOptions.None);

            string srcResultFile = path[0] +"Launch" + Path.DirectorySeparatorChar + "Launch" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar + "result.dat";
            configFile = path[0] + Path.DirectorySeparatorChar + "Launch" + Path.DirectorySeparatorChar + "config.param.txt";
             
            Console.WriteLine("Starting the program to generate the network based on popularity..");

            GlobValues glob = new GlobValues();
            try
            {
                // Read parameters from param.config file
                read_parameters(configFile, glob);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            // Displays the information
            Console.WriteLine("Given Parameter values");
            Console.WriteLine("\n Nodes: " + glob.nodes + "\n interEdges: " + glob.interEdges + "\nintra Edges: " + glob.intraEdges + "\n Clusters: " + glob.clusters);
            Console.WriteLine(" Number of runs: " + glob.numberOfGraphs + "\n Coupling probability value: " + glob.couplingProb * 100);


         for (int i = 1; i<= glob.numberOfGraphs;i++)
            {

                // Creating network file
                netFile                =  i + "_Pop_network_N" + glob.nodes + "_intra" + glob.intraEdges + "_inter" + glob.interEdges + "_C" + glob.clusters + ".edges";
                destinationNetworkFile =  path[0] + "Launch" + Path.DirectorySeparatorChar + "output" + Path.DirectorySeparatorChar + netFile;
               
                // creating membership file
                memFile            =  i+"_Pop_mem_N"+glob.nodes+"_intra"+glob.intraEdges+"_inter"+glob.interEdges+"_C"+glob.clusters+".dat";
                destMembershipFile =  path[0] + "Launch" + Path.DirectorySeparatorChar + "output" + Path.DirectorySeparatorChar + memFile;

               
                resFile            =  i + "_Pop_res_N" + glob.nodes + "_intra" + glob.intraEdges + "_inter" + glob.interEdges + "_C" + glob.clusters + ".dat";
                destResultFile     =  path[0] + "Launch" + Path.DirectorySeparatorChar + "output" + Path.DirectorySeparatorChar + resFile;
                    try

                        {
                                // create a popularity network and the membership file for the same
                                 creatPopularityNetwork(glob, destinationNetworkFile, destMembershipFile);

                    
                                 // upload the network to run the Kuramoto Model
                                 pop = Network.LoadFromEdgeFile(destinationNetworkFile);
                                
                                // Run the Kuramoto model here and store the results in the output directory
                                NetworkColorizer colorizer = new NetworkColorizer();
                                // Distribution of natural frequencies
                                double mean_frequency = 1d;
                                Normal normal = new Normal(mean_frequency, mean_frequency / 5d);

                                                sync = new Kuramoto(pop,
                                                glob.couplingStrength,
                                                glob.couplingProb,
                                                colorizer,
                                                new Func<Vertex, Vertex[]>(v => { return new Vertex[] { v.RandomNeighbor }; })
                                                );


                    
          //          var result = pop.VertexLabels.OrderBy(t => t, new SemiNumericComparer());
                //    foreach (String v in result)
                //    Console.WriteLine();
                    
                                    
                    

                    foreach (Vertex v in pop.Vertices)
                    {
                          sync.NaturalFrequencies[v] = normal.Sample();
                        //  Console.WriteLine(v.Label);
                    }

                                //  foreach (int g in network.ClusterIDs)
                                //    pacemaker_mode[g] = false;
  

                                sync.OnStep += new Kuramoto.StepHandler(recordOrder);

                                Logger.AddMessage(LogEntryType.AppMsg, "Press enter to start synchronization experiment...");
                                Console.ReadLine();

                                // Run the simulation
                                sync.Run();

                                // Write the time series to the resultfile 
                                if (srcResultFile != null)
                                    sync.WriteTimeSeries(srcResultFile);

                                // Moving results of kuramoto model into output directory
                                System.IO.File.Move(srcResultFile, destResultFile);
                                
                }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: " + e);
                            }
                            
            }
        }
   
    private static void recordOrder(double time)
        {
           
            // Compute and record global order parameter
            double globalOrder = sync.GetOrder(pop.Vertices.ToArray());

            foreach (Vertex v in pop.Vertices)
                sync.AddDataPoint(v.Label, sync.CurrentValues[sync._mapping[v]]);

            //  if (time > 30d)
            if (time > runTime)
                sync.Stop();

            //   Logger.AddMessage(LogEntryType.SimMsg, string.Format("Time = {000000}", time)); //Avg. Cluster Order = {1:0.00}, Global Order = {2:0.00}", time, avgLocalOrder, globalOrder));

        }
      }
}           
 
  #endregion