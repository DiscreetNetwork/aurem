using System.Runtime.InteropServices;

namespace Aurem.ECC.Native
{
    public static class AuremCore
    {
        [DllImport("AuremCore")]
        public static extern void Init();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G1 G1();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G2 G2();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt Order();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G1 ScalarMulG1([MarshalAs(UnmanagedType.Struct)]BigInt n);
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G2 ScalarMulG2([MarshalAs(UnmanagedType.Struct)]BigInt n);
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G1 ScalarPointMulG1([MarshalAs(UnmanagedType.Struct)]AltBn128G1 point, [MarshalAs(UnmanagedType.Struct)]BigInt n);
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G2 ScalarPointMulG2([MarshalAs(UnmanagedType.Struct)]AltBn128G2 point, [MarshalAs(UnmanagedType.Struct)]BigInt n);
        [DllImport("AuremCore")]
        public static extern bool PairsEqual([MarshalAs(UnmanagedType.Struct)]AltBn128G1 p1G1,
                                             [MarshalAs(UnmanagedType.Struct)]AltBn128G2 p1G2,
                                             [MarshalAs(UnmanagedType.Struct)]AltBn128G1 p2G1,
                                             [MarshalAs(UnmanagedType.Struct)]AltBn128G2 p2G2);
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt RandomCoefficient();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt EvaluatePolynomial([MarshalAs(UnmanagedType.Struct)]BigInt n);
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G1 AddG1([MarshalAs(UnmanagedType.Struct)]AltBn128G1 p1, [MarshalAs(UnmanagedType.Struct)]AltBn128G1 p2);
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern AltBn128G2 AddG2([MarshalAs(UnmanagedType.Struct)]AltBn128G2 p1, [MarshalAs(UnmanagedType.Struct)]AltBn128G2 p2);
    }

    public class Native : IDisposable
    {
        public static Native Instance;

        private static IntPtr _handle = IntPtr.Zero;

        public delegate void InitDelegate();
        public delegate AltBn128G1 G1Delegate();
        public delegate AltBn128G2 G2Delegate();
        public delegate BigInt OrderDelegate();
        public delegate AltBn128G1 ScalarMulG1Delegate([MarshalAs(UnmanagedType.Struct)]BigInt n);
        public delegate AltBn128G2 ScalarMulG2Delegate([MarshalAs(UnmanagedType.Struct)]BigInt n);
        public delegate AltBn128G1 ScalarPointMulG1Delegate([MarshalAs(UnmanagedType.Struct)]AltBn128G1 point, [MarshalAs(UnmanagedType.Struct)]BigInt n);
        public delegate AltBn128G2 ScalarPointMulG2Delegate([MarshalAs(UnmanagedType.Struct)]AltBn128G2 point, [MarshalAs(UnmanagedType.Struct)]BigInt n);
        public delegate bool PairsEqualDelegate([MarshalAs(UnmanagedType.Struct)]AltBn128G1 p1G1,
                                                [MarshalAs(UnmanagedType.Struct)]AltBn128G2 p1G2,
                                                [MarshalAs(UnmanagedType.Struct)]AltBn128G1 p2G1,
                                                [MarshalAs(UnmanagedType.Struct)]AltBn128G2 p2G2);
        public delegate BigInt RandomCoefficientDelegate();
        public delegate BigInt ModOrderDelegate([MarshalAs(UnmanagedType.Struct)]BigInt n);
        public delegate AltBn128G1 AddG1Delegate([MarshalAs(UnmanagedType.Struct)]AltBn128G1 p1, [MarshalAs(UnmanagedType.Struct)]AltBn128G1 p2);
        public delegate AltBn128G2 AddG2Delegate([MarshalAs(UnmanagedType.Struct)]AltBn128G2 p1, [MarshalAs(UnmanagedType.Struct)]AltBn128G2 p2);

        #pragma warning disable CS8618
        public InitDelegate Init;
        public G1Delegate G1;
        public G2Delegate G2;
        public OrderDelegate Order;
        public ScalarMulG1Delegate ScalarMulG1;
        public ScalarMulG2Delegate ScalarMulG2;
        public ScalarPointMulG1Delegate ScalarPointMulG1;
        public ScalarPointMulG2Delegate ScalarPointMulG2;
        public PairsEqualDelegate PairsEqual;
        public RandomCoefficientDelegate RandomCoefficient;
        public ModOrderDelegate ModOrder;
        public AddG1Delegate AddG1;
        public AddG2Delegate AddG2;
        #pragma warning restore CS8618

        private static EntryPointNotFoundException notFound(string name)
        {
            return new EntryPointNotFoundException($"failed to find endpoint \"{name}\" in library \"AuremCore\"");
        }

        static Native()
        {
            bool success = false;

            // TODO Load depending on operating system.
            // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            // {
            //     success = NativeLibrary.TryLoad("AuremCore.dll", typeof(AuremCore).Assembly, DllImportSearchPath.AssemblyDirectory, out _handle);
            // }
            success = NativeLibrary.TryLoad("AuremCore.so", typeof(AuremCore).Assembly, DllImportSearchPath.AssemblyDirectory, out _handle);

            if (!success)
            {
                throw new PlatformNotSupportedException("Failed to load \"AuremCore\" on this platform");
            }

            Instance = new Native();

            if (NativeLibrary.TryGetExport(_handle, "Init", out IntPtr _InitHandle))
                Instance.Init = Marshal.GetDelegateForFunctionPointer<InitDelegate>(_InitHandle);
            else
                Instance.Init = () => { throw notFound("Init"); };

            if (NativeLibrary.TryGetExport(_handle, "G1", out IntPtr _G1Handle))
                Instance.G1 = Marshal.GetDelegateForFunctionPointer<G1Delegate>(_G1Handle);
            else
                Instance.G1 = () => { throw notFound("G1"); };

            if (NativeLibrary.TryGetExport(_handle, "G2", out IntPtr _G2Handle))
                Instance.G2 = Marshal.GetDelegateForFunctionPointer<G2Delegate>(_G2Handle);
            else
                Instance.G2 = () => { throw notFound("G2"); };

            if (NativeLibrary.TryGetExport(_handle, "Order", out IntPtr _OrderHandle))
                Instance.Order = Marshal.GetDelegateForFunctionPointer<OrderDelegate>(_OrderHandle);
            else
                Instance.Order = () => { throw notFound("Order"); };

            if (NativeLibrary.TryGetExport(_handle, "ScalarMulG1", out IntPtr _ScalarMulG1Handle))
                Instance.ScalarMulG1 = Marshal.GetDelegateForFunctionPointer<ScalarMulG1Delegate>(_ScalarMulG1Handle);
            else
                Instance.ScalarMulG1 = (BigInt n) => { throw notFound("ScalarMulG1"); };

            if (NativeLibrary.TryGetExport(_handle, "ScalarMulG2", out IntPtr _ScalarMulG2Handle))
                Instance.ScalarMulG2 = Marshal.GetDelegateForFunctionPointer<ScalarMulG2Delegate>(_ScalarMulG2Handle);
            else
                Instance.ScalarMulG2 = (BigInt n) => { throw notFound("ScalarMulG2"); };

            if (NativeLibrary.TryGetExport(_handle, "ScalarPointMulG1", out IntPtr _ScalarPointMulG1Handle))
                Instance.ScalarPointMulG1 = Marshal.GetDelegateForFunctionPointer<ScalarPointMulG1Delegate>(_ScalarPointMulG1Handle);
            else
                Instance.ScalarPointMulG1 = (AltBn128G1 point, BigInt n) => { throw notFound("ScalarPointMulG1"); };

            if (NativeLibrary.TryGetExport(_handle, "ScalarPointMulG2", out IntPtr _ScalarPointMulG2Handle))
                Instance.ScalarPointMulG2 = Marshal.GetDelegateForFunctionPointer<ScalarPointMulG2Delegate>(_ScalarPointMulG2Handle);
            else
                Instance.ScalarPointMulG2 = (AltBn128G2 point, BigInt n) => { throw notFound("ScalarPointMulG2"); };

            if (NativeLibrary.TryGetExport(_handle, "PairsEqual", out IntPtr _PairsEqualHandle))
                Instance.PairsEqual = Marshal.GetDelegateForFunctionPointer<PairsEqualDelegate>(_PairsEqualHandle);
            else
                Instance.PairsEqual = (AltBn128G1 p1G1, AltBn128G2 p1G2,
                                       AltBn128G1 p2G1, AltBn128G2 p2G2) => { throw notFound("PairsEqual"); };

            if (NativeLibrary.TryGetExport(_handle, "RandomCoefficient", out IntPtr _RandomCoefficientHandle))
                Instance.RandomCoefficient = Marshal.GetDelegateForFunctionPointer<RandomCoefficientDelegate>(_RandomCoefficientHandle);
            else
                Instance.RandomCoefficient = () => { throw notFound("RandomCoefficient"); };

            if (NativeLibrary.TryGetExport(_handle, "ModOrder", out IntPtr _ModOrderHandle))
                Instance.ModOrder = Marshal.GetDelegateForFunctionPointer<ModOrderDelegate>(_ModOrderHandle);
            else
                Instance.ModOrder = (BigInt n) => { throw notFound("ModOrder"); };

            if (NativeLibrary.TryGetExport(_handle, "AddG1", out IntPtr _AddG1Handle))
                Instance.AddG1 = Marshal.GetDelegateForFunctionPointer<AddG1Delegate>(_AddG1Handle);
            else
                Instance.AddG1 = (AltBn128G1 p1, AltBn128G1 p2) => { throw notFound("AddG1"); };

            if (NativeLibrary.TryGetExport(_handle, "AddG2", out IntPtr _AddG2Handle))
                Instance.AddG2 = Marshal.GetDelegateForFunctionPointer<AddG2Delegate>(_AddG2Handle);
            else
                Instance.AddG2 = (AltBn128G2 p1, AltBn128G2 p2) => { throw notFound("AddG2"); };
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                NativeLibrary.Free(_handle);

                disposedValue = true;
            }
        }

        ~Native()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
