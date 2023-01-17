/// <summary>
/// </summary>
namespace Aurem.Networking
{
    /// <summary>
    /// This class simulates some operations that would be needed by an actual
    /// network.
    /// </summary>
    public static class Network
    {
        public static int NodesCount { get; set; } = 10;
        public static int MinimumParents() {
            return (int)double.Round(Network.NodesCount - (Network.NodesCount - 1) / 3.0);
        }
    }
}
