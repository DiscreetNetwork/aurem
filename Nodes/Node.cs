using System.Security.Cryptography;
using System.Numerics;
using Aurem.chDAGs;
using Aurem.Randomness;
using Aurem.Units;
using Aurem.Networking;
using Aurem.ECC;
using Aurem.ECC.Native;

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
            return _vk.CombineShares(shares);
        }

        /// <summary>
        /// GetUnitsSignature combines the shares of all the provided units into
        /// a signature.
        /// </summary>
        public AltBn128G1 GetUnitsSignature(List<Unit> units)
        {
            Dictionary<long, AltBn128G1> shares = new();
            foreach (Unit unit in units)
                shares[unit.CreatorId] = unit.Share;

            return CombineShares(shares);
        }

        /// <summary>
        /// GetUnitPriority determines the priority of a unit in a random
        /// permutation, given a signature.
        /// </summary>
        public BigInteger GetUnitPriority(Unit unit, AltBn128G1 signature)
        {
            AltBn128G1 priority = Native.Instance.ScalarPointMulG1(signature, new BigInt(unit.CreatorId));
            // TODO It should be faster to keep them as byte arrays and do
            // comparisons at byte levels.
            return new BigInteger(HashG1(priority));
        }

        /// <summary>
        /// HashG1 hashes an AltBn128G1 point using SHA-256.
        /// </summary>
        private byte[] HashG1(AltBn128G1 point)
        {
            byte[] bytes = new byte[point.X.Words.Length * sizeof(ulong) * 3];
            Buffer.BlockCopy(point.X.Words, 0, bytes, 0, bytes.Length / 3);
            Buffer.BlockCopy(point.Y.Words, 0, bytes, bytes.Length / 3, bytes.Length / 3);
            Buffer.BlockCopy(point.Z.Words, 0, bytes, 2 * bytes.Length / 3, bytes.Length / 3);

            SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        /// SecretBit returns the first bit of the hash of the signature.
        /// SecretBit does not check if the signature is valid or not.
        /// </summary>
        public bool SecretBit(AltBn128G1 signature)
        {
            // Getting most significant bit of first byte.
            return (HashG1(signature)[0] & 0x80) == 0x80;
        }
    }
}
