using NUlid;
using Aurem.chDAGs;
using Aurem.Randomness;
using Aurem.Units;
using Aurem.Networking;
using Aurem.ECC;
using Aurem.Shared;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Node represents a Unit creator; an entity responsible for creating valid
/// chDAG units.
/// </summary>
namespace Aurem.Nodes
{
    /// <summary>
    /// A node represents an entity that is producing DAG units for a
    /// network of nodes.
    /// </summary>
    public class Node
    {
        public long Id;
        // Each node has its on local copy of the chDAG in the network.
        public chDAG _chDAG { get; }
        private SecretKey _sk;
        private VerificationKey _vk;
        private Network _network;
        private Queue<Unit> _unitQueue;
        private readonly object _lock = new object();

        public Node(long id, Network network, SecretKey sk, VerificationKey vk)
        {
            Id = id;
            // We first register the node to the network, and then we create a
            // chDAG associated to this network.
            network.Add(this);
            _network = network;
            _chDAG = new(network, this);
            _sk = sk;
            _vk = vk;
            _unitQueue = new();
        }

        /// <summary>
        /// GetChDAG returns the chDAG to which the node is registered to.
        /// </summary>
        public chDAG GetChDAG()
        {
            return _chDAG;
        }

        /// <summary>
        /// CreateUnit creates a new unit for this node's chDAG, which contains
        /// some data.
        /// </summary>
        public void CreateUnit(byte[] data)
        {
            lock (_lock) {
                BigInt round = new BigInt(_chDAG.Round);
                _chDAG.Add(new Unit(Id, data, _sk.GenerateShare(round)));
            }
        }

        /// <summary>
        /// ValidateShare validates a share belonging to a node.
        /// </summary>
        public bool ValidateShare(AltBn128G1 share, long vkIdx, int round)
        {
            return _vk.VerifyShare(share, vkIdx, new BigInt(round));
        }

        /// <summary>
        /// ValidateSignature validates a signature belonging to a round.
        /// </summary>
        public bool ValidateSignature(AltBn128G1 signature, int round)
        {
            return _vk.VerifySignature(signature, new BigInt(round));
        }

        /// <summary>
        /// CombineShares combines a list of message shares to create a signature.
        /// </summary>
        public AltBn128G1 CombineShares(Dictionary<long, AltBn128G1> shares)
        {
            // Dictionary<int, AltBn128G1> dshares = new();
            // int threshold = _network.MinimumParents();
            // for (int i = 0; i < threshold; i++)
            //     dshares[i] = shares[i];
            return _vk.CombineShares(shares);
        }

        /// <summary>
        /// SecretBit returns the first bit of the hash of the signature.
        /// SecretBit does not check if the signature is valid or not.
        /// </summary>
        public bool SecretBit(AltBn128G1 signature)
        {
            byte[] bytes = new byte[signature.X.Words.Length * sizeof(ulong) * 3];
            Buffer.BlockCopy(signature.X.Words, 0, bytes, 0, bytes.Length / 3);
            Buffer.BlockCopy(signature.Y.Words, 0, bytes, bytes.Length / 3, bytes.Length / 3);
            Buffer.BlockCopy(signature.Z.Words, 0, bytes, 2 * bytes.Length / 3, bytes.Length / 3);

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);

            // TODO Check if this was actually necessary, or if hash[0] is enough.
            byte sum = 0;
            for (int i = 0; i < hash.Length; i++)
            {
                sum += hash[i];
            }

            // Getting most significant bit of first byte.
            // TODO For some reason, with byte 0 always throws False. Using byte
            // 1 shouldn't affect at all, but still check why.
            // return (hash[0] & 0x80) == 0x80;
            return (sum & 0x80) == 0x80;
        }
    }
}
