﻿using NUlid;
using Aurem.Nodes;
using Aurem.Networking;
using Aurem.Randomness;
using Aurem.chDAGs;
using Aurem.ECC;
using Aurem.ECC.Native;
using Aurem.Shared;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Aurem
/// </summary>
namespace Aurem
{
    class Program
    {
        private IConfiguration GetConfig() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");

            return builder.Build();
        }

        /// <summary>
        /// InitNodes initiates the nodes that will be participating in the
        /// network. It also acts as a trusted dealer that secretly sends a
        /// secret key to each node, along with a public verification key.
        /// </summary>
        private List<Node> InitNodes(int numNodes, Network network)
        {
            ThresholdSignature ts = new(network.MinimumParents(numNodes), numNodes);
            (VerificationKey vk, List<SecretKey> sks) = ts.GenerateKeys();

            List<Node> nodes = new List<Node>();
            for (int c = 0; c < numNodes; c++) {
                nodes.Add(new Node((long) c, network, sks[c], vk));
            }
            return nodes;
        }

        private void SaveGraphs(string graphsDir, List<Node> nodes) {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string graphsPath = Path.Combine(homePath, graphsDir);
            // Creating directory to contain the generated graphs.
            if (graphsDir != "") {
                DirectoryInfo dir = new DirectoryInfo(graphsPath);
                if (!dir.Exists) dir.Create();
            }
            // Saving each chDAG.
            foreach (Node node in nodes)
                node.GetChDAG().Save(graphsPath);
        }

        private void Run()
        {
            /////////////////////////
            Random random = new Random();

            // Reading configuration values.
            IConfiguration config = GetConfig();
            int numNodes = int.Parse(config["numNodes"] ?? "10");
            int fixedRounds = int.Parse(config["fixedRounds"] ?? "-1");
            string graphsDir = config["graphsDirectory"] ?? "";
            bool stepByStepGraphs = false;
            _ = bool.TryParse(config["stepByStepGraphs"] ?? "false", out stepByStepGraphs);
            bool unitPerRoundDiffColor = false;
            _ = bool.TryParse(config["unitPerRoundDiffColor"] ?? "false", out unitPerRoundDiffColor);

            // Creating nodes to simulate the "minting" of units.
            Network network = new();
            List<Node> nodes = InitNodes(numNodes, network);

            // Syncing units for every node in an infinite loop.
            // NOTE This will not be necessary later, as we should adopt a
            // publisher-subscriber architecture or something similar, where
            // we just listen to a publisher for new units, instead of requesting
            // for units from a particular round.
            bool runSync = true;
            Thread sync = new Thread(() => {
                while (runSync) {
                    foreach (Node node in nodes) {
                        chDAG chDAG = node.GetChDAG();
                        int round = chDAG.Round;
                        int lastN = 4;
                        int c = round - lastN;
                        if (round < lastN) {
                            c = 0;
                        }
                        for ( ; c < round; c++)
                            chDAG.Sync(c);
                    }
                }
            });
            sync.Start();

            List<Thread> threads = new();
            int threadIdx = 0;

            // Run forever or fixedRounds times.
            for (int c = 0; c < fixedRounds || fixedRounds < 1; c++) {

                // Now we'll create units for each node.
                // We are randomizing the order of each node for each iteration.
                // The purpose of this is to simulate an asynchronous behavior.
                // network.ShuffleNodes();
                foreach (Node node in nodes) {
                    threads.Add(new Thread(() => {
                        // Simulating latency.
                        if (node == nodes[0] || node == nodes[1])
                            Thread.Sleep(random.Next(0, 1000));
                        // We don't care about what data we store for this PoC.
                        node.CreateUnit(new byte[1]{ (byte)random.Next(0, 255) });
                    }));
                }
                for (int i = threadIdx; i < threads.Count; i++)
                    threads[i].Start();
                threadIdx += nodes.Count;
            }

            // Wait for all the threads to finish execution.
            while(!threads.TrueForAll((thread) => !thread.IsAlive )) { }
            runSync = false;

            // Reporting any byzantine units found.
            network.ReportByzantine();

            // If fixedRounds is set, also save a graph of the final chDAG.
            // Note that if it's not set, c != fixedRounds always holds.
            if (fixedRounds > 0) {
                SaveGraphs(graphsDir, nodes);
            }
        }

        static void Main(string[] args)
        {
            Native.Instance.Init();
            Console.WriteLine("Running Aurem");
            Program prgrm = new();
            prgrm.Run();
            Console.WriteLine("Done");
        }
    }

}
