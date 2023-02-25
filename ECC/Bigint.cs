using System.Runtime.InteropServices;

namespace Aurem.ECC
{
    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BigInt
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public ulong[] Words;
    }
}
