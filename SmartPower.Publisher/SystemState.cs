public class SystemState
{
    public float Temp1 { get; set; }
    public float Temp2 { get; set; }
    public float Rms1 { get; set; }
    public float Rms2 { get; set; }

    public bool CompressorOn { get; set; }
    public bool EvapFanOn { get; set; }
    public bool DefrostOn { get; set; }
    public bool LightOn { get; set; }
}