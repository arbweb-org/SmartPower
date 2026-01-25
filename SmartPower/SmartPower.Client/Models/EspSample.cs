using System.Runtime.InteropServices;

namespace SmartPower.Client.Models
{
    // Sample structure matching ESP32 EspSample (12 bytes)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EspSample
    {
        public uint Time;
        public int S1;
        public int S2;
    }
}