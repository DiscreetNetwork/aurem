﻿using NUlid;
using Aurem.Nodes;
using Aurem.Networking;
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

        private List<Node> InitNodes(int numNodes, Network network)
        {
            List<Node> nodes = new List<Node>();
            for (int c = 0; c < numNodes; c++) {
                nodes.Add(new Node(Ulid.NewUlid(), network));
            }
            return nodes;
        }

        private void ShuffleNodes(List<Node> nodes)
        {
            Random rng = new Random();
            int n = nodes.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                Node value = nodes[k];
                nodes[k] = nodes[n];
                nodes[n] = value;
            }
        }

        private void Run()
        {
            Random random = new Random();

            // Reading configuration values.
            IConfiguration config = GetConfig();
            int numNodes = int.Parse(config["numNodes"] ?? "10");

            // Creating nodes to simulate the "minting" of units.
            Network network = new();
            List<Node> nodes = InitNodes(numNodes, network);
            while (true) {
                // NOTE The threads are being wrapped in tasks, so we can use Task.WaitAll.
                // If there is a more optimal way to do this, feel free to report this.
                // List<Thread> threads = new();
                List<Task> tasks = new();

                // Now we'll create units for each node.
                // We are randomizing the order of each node for each iteration.
                // The purpose of this is to simulate an asynchronous behavior.
                // TODO Create method Network.ShuffleNodes().
                ShuffleNodes(nodes);
                foreach (Node node in nodes) {
                    double r = random.NextDouble();
                    // We want to simulate some latency. We'll do it in a very dumb way.
                    // There's a chance that a node "won't add" a unit in this
                    // "round" or, more precisely, it won't add it "in time".
                    if (r > 0.9)
                        // We don't care about what data we store for this PoC.
                        // threads.Add(new Thread(() => {
                        //     node.CreateUnit(new byte[1]{ (byte)random.Next(0, 255) });
                        // }));
                        tasks.Add(new Task(() => {
                            node.CreateUnit(new byte[1]{ (byte)random.Next(0, 255) });
                        }));
                }
                // foreach (Thread thread in threads) { tasks.Add(Task.Run(() => thread.Start())); }
                foreach (Task task in tasks) { task.Start(); }
                Task.WaitAll(tasks.ToArray());
                Console.WriteLine("================");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Running");
            Program prgrm = new();
            prgrm.Run();
        }
    }

}