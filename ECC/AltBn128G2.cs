using System.Runtime.InteropServices;

namespace Aurem.ECC
{
    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AltBn128G2
    {
        public BigInt8 X;
        // public BigInt Xc0;
        // public BigInt Xc1;
        public BigInt8 Y;
        // public BigInt Yc0;
        // public BigInt Yc1;
        public BigInt8 Z;
        // public BigInt Zc0;
        // public BigInt Zc1;
    }
}
