/// <summary>
/// </summary>
namespace Aurem.Units
{
    public class Unit
    {
        public byte[] Data { get; set; }
        public List<Unit>? Parents { get; set; }
        public int Round { get; set; }

        public Unit(byte[] data)
        {
            this.Data = data;
            this.Round = 0;
        }
    }
}
