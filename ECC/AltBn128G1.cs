using System.Runtime.InteropServices;

namespace Aurem.ECC
{
    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AltBn128G1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] X;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Y;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Z;
    }
}
