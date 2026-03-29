// Stub: OpenCLNetWin / OpenCLNetMac - GPU compute library for marching cubes terrain
// Used by 4 files in Assets/Terrain/Voxelform2/.../Ocl/
// These stubs allow compilation; actual GPU compute is not functional.
// The game already has a CPU fallback path (oclManager.CurOclOpt defaults to "Cpu").
// NumberOfPlatforms returns 0 so the init loop finds no devices and falls back to CPU.

using System;
using System.Collections.Generic;

namespace OpenCLNetWin
{
    public enum DeviceType
    {
        CPU = 1,
        GPU = 2,
        ACCELERATOR = 4,
        ALL = unchecked((int)0xFFFFFFFF),
    }

    [Flags]
    public enum MemFlags : ulong
    {
        READ_WRITE = 1,
        WRITE_ONLY = 2,
        READ_ONLY = 4,
        USE_HOST_PTR = 8,
        ALLOC_HOST_PTR = 16,
        COPY_HOST_PTR = 32,
    }

    public enum MemObjectType
    {
        BUFFER = 0,
        IMAGE2D = 1,
        IMAGE3D = 2,
    }

    public enum ChannelOrder
    {
        R = 0, A = 1, RG = 2, RA = 3, RGB = 4, RGBA = 5,
        BGRA = 6, ARGB = 7, INTENSITY = 8, LUMINANCE = 9,
    }

    public enum ChannelType
    {
        SNORM_INT8 = 0, SNORM_INT16 = 1, UNORM_INT8 = 2, UNORM_INT16 = 3,
        UNORM_SHORT_565 = 4, UNORM_SHORT_555 = 5, UNORM_INT_101010 = 6,
        SIGNED_INT8 = 7, SIGNED_INT16 = 8, SIGNED_INT32 = 9,
        UNSIGNED_INT8 = 10, UNSIGNED_INT16 = 11, UNSIGNED_INT32 = 12,
        HALF_FLOAT = 13, FLOAT = 14,
    }

    public enum ContextProperties : long
    {
        PLATFORM = 0x1084,
    }

    public enum ExecutionStatus
    {
        COMPLETE = 0,
        RUNNING = 1,
        SUBMITTED = 2,
        QUEUED = 3,
    }

    public struct ImageFormat
    {
        public ChannelOrder ChannelOrder;
        public ChannelType ChannelType;

        public ImageFormat(ChannelOrder order, ChannelType type)
        {
            ChannelOrder = order;
            ChannelType = type;
        }
    }

    public struct UInt4
    {
        public uint S0, S1, S2, S3;

        public UInt4(uint s0, uint s1, uint s2, uint s3)
        {
            S0 = s0; S1 = s1; S2 = s2; S3 = s3;
        }

        public UInt4(int s0, int s1, int s2, int s3)
        {
            S0 = (uint)s0; S1 = (uint)s1; S2 = (uint)s2; S3 = (uint)s3;
        }
    }

    public class Event
    {
        public ExecutionStatus ExecutionStatus { get { return ExecutionStatus.COMPLETE; } }
    }

    public class Mem : IDisposable
    {
        public long MemSize { get; set; }
        public void Dispose() { }
    }

    public class Kernel : IDisposable
    {
        public void SetArg(int index, Mem mem) { }
        public void SetArg(int index, uint value) { }
        public void SetArg(int index, int value) { }
        public void SetArg(int index, float value) { }
        public void SetArg(int index, UInt4 value) { }
        // Local memory allocation overload used in oclScanLaucherA.cs
        public void SetArg(int index, System.IntPtr size, System.IntPtr ptr) { }
        public void Dispose() { }
    }

    public class Program
    {
        public Kernel CreateKernel(string name) { return new Kernel(); }
    }

    public class Context
    {
        public Mem CreateBuffer(MemFlags flags, long size)
        {
            return new Mem { MemSize = size };
        }

        public Mem CreateBuffer(MemFlags flags, long size, IntPtr hostPtr)
        {
            return new Mem { MemSize = size };
        }

        public Mem CreateImage3D(MemFlags flags, ImageFormat format,
            int width, int height, int depth,
            int rowPitch, int slicePitch, IntPtr hostPtr)
        {
            return new Mem();
        }

        public bool SupportsImageFormat(MemFlags flags, MemObjectType type,
            ChannelOrder order, ChannelType channelType)
        {
            return false;
        }
    }

    public class Device
    {
        public string Name { get { return "StubCPU"; } }
        public DeviceType DeviceType { get { return DeviceType.CPU; } }
        public long MaxWorkGroupSize { get { return 256; } }
        public IntPtr[] MaxWorkItemSizes
        {
            get { return new IntPtr[] { new IntPtr(256), new IntPtr(256), new IntPtr(256) }; }
        }
    }

    public class Platform
    {
        public string Name { get { return "StubPlatform"; } }

        public Device[] QueryDevices(DeviceType type)
        {
            return new Device[0];
        }

        public static implicit operator IntPtr(Platform p) { return IntPtr.Zero; }
    }

    public class CommandQueue
    {
        public Device Device { get { return new Device(); } }
        public Context Context { get { return new Context(); } }

        public void EnqueueNDRangeKernel(Kernel kernel, int workDim,
            int[] globalWorkOffset, int[] globalWorkSize, int[] localWorkSize) { }

        public void EnqueueReadBuffer(Mem buffer, bool blockingRead,
            int offset, int cb, IntPtr ptr) { }

        public void EnqueueReadBuffer(Mem buffer, bool blockingRead,
            int offset, int cb, IntPtr ptr,
            int numEvents, Event[] waitList, out Event evt)
        {
            evt = new Event();
        }

        public void EnqueueWriteImage(Mem image, bool blockingWrite,
            int[] origin, int[] region, int rowPitch, int slicePitch, IntPtr ptr) { }

        public void EnqueueWriteBufferRect(Mem buffer, bool blockingWrite,
            int[] bufferOrigin, int[] hostOrigin, int[] region,
            int bufferRowPitch, int bufferSlicePitch,
            int hostRowPitch, int hostSlicePitch, IntPtr ptr) { }

        public void EnqueueMarker(out Event evt) { evt = new Event(); }

        public void Flush() { }

        public void Finish() { }
    }

    public class OpenCLManager : IDisposable
    {
        public string SourcePath { get; set; }
        public string BinaryPath { get; set; }
        public string BuildOptions { get; set; }
        public string Defines { get; set; }
        public Platform Platform { get; private set; }
        public Context Context { get; private set; }
        public CommandQueue[] CQ { get; private set; }

        public OpenCLManager()
        {
            Platform = new Platform();
            Context = new Context();
            CQ = new CommandQueue[0];
        }

        public void CreateContext(Platform platform, IntPtr[] properties, Device[] devices)
        {
            Platform = platform;
            Context = new Context();
            CQ = new CommandQueue[] { new CommandQueue() };
        }

        public Program CompileSource(string source)
        {
            return new Program();
        }

        public void Dispose() { }
    }

    public static class OpenCL
    {
        public static int NumberOfPlatforms { get { return 0; } }

        public static Platform GetPlatform(int index)
        {
            return new Platform();
        }
    }

    public class OpenCLBuildException : Exception
    {
        public List<string> BuildLogs { get; set; }

        public OpenCLBuildException() : base("OpenCL build failed")
        {
            BuildLogs = new List<string>();
        }

        public OpenCLBuildException(string message) : base(message)
        {
            BuildLogs = new List<string>();
        }
    }
}

// ============================================================
// OpenCLNetMac namespace - mirrors OpenCLNetWin for Mac builds
// ============================================================

namespace OpenCLNetMac
{
    using System;
    using System.Collections.Generic;

    public enum DeviceType { CPU = 1, GPU = 2, ACCELERATOR = 4, ALL = unchecked((int)0xFFFFFFFF) }

    [System.Flags]
    public enum MemFlags : ulong { READ_WRITE = 1, WRITE_ONLY = 2, READ_ONLY = 4, USE_HOST_PTR = 8, ALLOC_HOST_PTR = 16, COPY_HOST_PTR = 32 }

    public enum MemObjectType { BUFFER = 0, IMAGE2D = 1, IMAGE3D = 2 }

    public enum ChannelOrder { R = 0, A = 1, RG = 2, RA = 3, RGB = 4, RGBA = 5, BGRA = 6, ARGB = 7, INTENSITY = 8, LUMINANCE = 9 }

    public enum ChannelType { SNORM_INT8 = 0, SNORM_INT16 = 1, UNORM_INT8 = 2, UNORM_INT16 = 3, UNORM_SHORT_565 = 4, UNORM_SHORT_555 = 5, UNORM_INT_101010 = 6, SIGNED_INT8 = 7, SIGNED_INT16 = 8, SIGNED_INT32 = 9, UNSIGNED_INT8 = 10, UNSIGNED_INT16 = 11, UNSIGNED_INT32 = 12, HALF_FLOAT = 13, FLOAT = 14 }

    public enum ContextProperties : long { PLATFORM = 0x1084 }

    public enum ExecutionStatus { COMPLETE = 0, RUNNING = 1, SUBMITTED = 2, QUEUED = 3 }

    public struct ImageFormat
    {
        public ChannelOrder ChannelOrder;
        public ChannelType ChannelType;
        public ImageFormat(ChannelOrder order, ChannelType type) { ChannelOrder = order; ChannelType = type; }
    }

    public struct UInt4
    {
        public uint S0, S1, S2, S3;
        public UInt4(uint s0, uint s1, uint s2, uint s3) { S0 = s0; S1 = s1; S2 = s2; S3 = s3; }
        public UInt4(int s0, int s1, int s2, int s3) { S0 = (uint)s0; S1 = (uint)s1; S2 = (uint)s2; S3 = (uint)s3; }
    }

    public class Event { public ExecutionStatus ExecutionStatus { get { return ExecutionStatus.COMPLETE; } } }

    public class Mem : IDisposable { public long MemSize { get; set; } public void Dispose() { } }

    public class Kernel : IDisposable
    {
        public void SetArg(int index, Mem mem) { }
        public void SetArg(int index, uint value) { }
        public void SetArg(int index, int value) { }
        public void SetArg(int index, float value) { }
        public void SetArg(int index, UInt4 value) { }
        // Local memory allocation overload used in oclScanLaucherA.cs
        public void SetArg(int index, System.IntPtr size, System.IntPtr ptr) { }
        public void Dispose() { }
    }

    public class Program { public Kernel CreateKernel(string name) { return new Kernel(); } }

    public class Context
    {
        public Mem CreateBuffer(MemFlags flags, long size) { return new Mem { MemSize = size }; }
        public Mem CreateBuffer(MemFlags flags, long size, IntPtr hostPtr) { return new Mem { MemSize = size }; }
        public Mem CreateImage3D(MemFlags flags, ImageFormat format, int width, int height, int depth, int rowPitch, int slicePitch, IntPtr hostPtr) { return new Mem(); }
        public bool SupportsImageFormat(MemFlags flags, MemObjectType type, ChannelOrder order, ChannelType channelType) { return false; }
    }

    public class Device
    {
        public string Name { get { return "StubCPU"; } }
        public DeviceType DeviceType { get { return DeviceType.CPU; } }
        public long MaxWorkGroupSize { get { return 256; } }
        public IntPtr[] MaxWorkItemSizes { get { return new IntPtr[] { new IntPtr(256), new IntPtr(256), new IntPtr(256) }; } }
    }

    public class Platform
    {
        public string Name { get { return "StubPlatform"; } }
        public Device[] QueryDevices(DeviceType type) { return new Device[0]; }
        public static implicit operator IntPtr(Platform p) { return IntPtr.Zero; }
    }

    public class CommandQueue
    {
        public Device Device { get { return new Device(); } }
        public Context Context { get { return new Context(); } }
        public void EnqueueNDRangeKernel(Kernel kernel, int workDim, int[] globalWorkOffset, int[] globalWorkSize, int[] localWorkSize) { }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, int offset, int cb, IntPtr ptr) { }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, int offset, int cb, IntPtr ptr, int numEvents, Event[] waitList, out Event evt) { evt = new Event(); }
        public void EnqueueWriteImage(Mem image, bool blockingWrite, int[] origin, int[] region, int rowPitch, int slicePitch, IntPtr ptr) { }
        public void EnqueueWriteBufferRect(Mem buffer, bool blockingWrite, int[] bufferOrigin, int[] hostOrigin, int[] region, int bufferRowPitch, int bufferSlicePitch, int hostRowPitch, int hostSlicePitch, IntPtr ptr) { }
        public void EnqueueMarker(out Event evt) { evt = new Event(); }
        public void Flush() { }
        public void Finish() { }
    }

    public class OpenCLManager : IDisposable
    {
        public string SourcePath { get; set; }
        public string BinaryPath { get; set; }
        public string BuildOptions { get; set; }
        public string Defines { get; set; }
        public Platform Platform { get; private set; }
        public Context Context { get; private set; }
        public CommandQueue[] CQ { get; private set; }

        public OpenCLManager()
        {
            Platform = new Platform();
            Context = new Context();
            CQ = new CommandQueue[0];
        }

        public void CreateContext(Platform platform, IntPtr[] properties, Device[] devices)
        {
            Platform = platform;
            Context = new Context();
            CQ = new CommandQueue[] { new CommandQueue() };
        }

        public Program CompileSource(string source) { return new Program(); }
        public void Dispose() { }
    }

    public static class OpenCL
    {
        public static int NumberOfPlatforms { get { return 0; } }
        public static Platform GetPlatform(int index) { return new Platform(); }
    }

    public class OpenCLBuildException : Exception
    {
        public List<string> BuildLogs { get; set; }
        public OpenCLBuildException() : base("OpenCL build failed") { BuildLogs = new List<string>(); }
        public OpenCLBuildException(string message) : base(message) { BuildLogs = new List<string>(); }
    }
}
