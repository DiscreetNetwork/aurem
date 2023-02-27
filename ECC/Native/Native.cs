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
        public static extern AltBn128G1 G2();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt Order();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt RandomFq();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt RandomFq2();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt RandomCoefficient();
        [DllImport("AuremCore")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern BigInt EvaluatePolynomial([MarshalAs(UnmanagedType.Struct)]BigInt n);
    }

    public class Native : IDisposable
    {
        public static Native Instance;

        private static IntPtr _handle = IntPtr.Zero;

        public delegate void InitDelegate();
        public delegate AltBn128G1 G1Delegate();
        public delegate AltBn128G2 G2Delegate();
        public delegate BigInt OrderDelegate();
        public delegate BigInt RandomCoefficientDelegate();
        public delegate BigInt ModOrderDelegate([MarshalAs(UnmanagedType.Struct)]BigInt n);

        public InitDelegate Init;
        public G1Delegate G1;
        public G2Delegate G2;
        public OrderDelegate Order;
        public RandomCoefficientDelegate RandomCoefficient;
        public ModOrderDelegate ModOrder;

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

            if (NativeLibrary.TryGetExport(_handle, "RandomCoefficient", out IntPtr _RandomCoefficientHandle))
                Instance.RandomCoefficient = Marshal.GetDelegateForFunctionPointer<RandomCoefficientDelegate>(_RandomCoefficientHandle);
            else
                Instance.RandomCoefficient = () => { throw notFound("RandomCoefficient"); };

            if (NativeLibrary.TryGetExport(_handle, "ModOrder", out IntPtr _ModOrderHandle))
                Instance.ModOrder = Marshal.GetDelegateForFunctionPointer<ModOrderDelegate>(_ModOrderHandle);
            else
                Instance.ModOrder = (BigInt n) => { throw notFound("ModOrder"); };
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
