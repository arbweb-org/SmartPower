using System.Runtime.InteropServices;

namespace SmartPower.Client.Models
{
    // Header structure matching ESP32 DataHeader (8 bytes)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DataHeader
    {
        public int Cal1;
        public int Cal2;
    }
}