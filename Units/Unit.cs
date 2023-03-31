using NUlid;
using Aurem.ECC;

/// <summary>
/// Aurem.Units contains code related to chDAG units, which are data structures
/// that represent nodes in a DAG. Units hold transactions data.
/// </summary>
namespace Aurem.Units
{
    public class Unit
    {
        // TODO We need to calculate a hash to use as ID.
        public Ulid Id { get; set;}
        public long CreatorId { get; set; }
        public byte[] Data { get; set; }
        public List<Unit>? Parents { get; set; }
        public int Round { get; set; }
        public AltBn128G1 Share;

        public Unit(long creatorId, byte[] data, AltBn128G1 share)
        {
            CreatorId = creatorId;
            Id = Ulid.NewUlid();
            Data = data;
            Round = 0;
            Share = share;
        }
    }
}
