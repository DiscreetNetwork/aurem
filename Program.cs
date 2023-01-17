using NUlid;
using Aurem.Nodes;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Aurem
/// </summary>
namespace Aurem
{
    class Program
    {
        int numNodes = 10;

        private IConfiguration GetConfig() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");

            // IConfiguration config = builder.Build();
            return builder.Build();
            // int numNodes = config["numNodes"] != null ? int.Parse(config["numNodes"]) : 10 ;
            // Console.WriteLine(numNodes);
        }

        private List<Node> InitNodes(int numNodes)
        {
            List<Node> nodes = new List<Node>();
            for (int c = 0; c < numNodes; c++) {
                nodes.Add(new Node(Ulid.NewUlid()));
            }
            return nodes;
        }

        private void Run()
        {
            IConfiguration config = GetConfig();
            int numNodes = int.Parse(config["numNodes"] ?? "10");
            // int numNodes = int.Parse(ConfigurationManager.AppSettings["numNodes"]);

            Random random = new Random();
            // chDAG<Unit> dag = new();

            // Creating nodes to simulate the "minting" of units.
            List<Node> nodes = InitNodes(numNodes);
            while (true) {
                // Now we'll create units for each node.
                // We are randomizing the order of each node for each iteration.
                // The purpose of this is to simulate an asynchronous behavior.
                foreach (Node node in nodes.OrderBy(node => random.Next())) {
                    double r = random.NextDouble();
                    // We want to simulate some latency. We'll do it in a very dumb way.
                    // There's a chance that a node "won't add" a unit in this
                    // "round" or, more precisely, it won't add it "in time".
                    if (r > 0.9)
                        // We don't care about what data we store for this PoC.
                        node.CreateUnit(new byte[1]);
                }
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
