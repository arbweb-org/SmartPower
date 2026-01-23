namespace SmartPower.Client.Models
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EspSample
    {
        public uint Time;
        public int S1;
        public int S2;
    }
}