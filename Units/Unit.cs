using NUlid;

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
        public Ulid CreatorId { get; set; }
        public byte[] Data { get; set; }
        public List<Unit>? Parents { get; set; }
        public int Round { get; set; }

        public Unit(Ulid creatorId, byte[] data)
        {
            this.CreatorId = creatorId;
            this.Id = Ulid.NewUlid();
            this.Data = data;
            this.Round = 0;
        }
    }
}
