/// <summary>
/// </summary>
namespace Aurem.Units
{
    public class Unit
    {
        public byte[] Data { get; set; }
        public List<Unit> Parents { get; set; }
        public int Round { get; set; }

        public Unit(List<Unit> parents, int round, byte[] data)
        {
            this.Parents = parents;
            this.Data = data;
            this.Round = round;
        }
    }
}
