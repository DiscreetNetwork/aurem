using System.Runtime.InteropServices;

namespace Aurem.ECC
{
    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AltBn128G1
    {
        public BigInt X;
        public BigInt Y;
        public BigInt Z;
    }
}
