using System;
using System.Runtime.InteropServices;

namespace PresentationApp
{
    public static class KernelDll
    {
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void RtlMoveMemory(IntPtr destination, IntPtr source, uint length);
    }
}