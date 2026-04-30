public class Refrigerator
{
    public bool CompressorOn { get; private set; } = false;
    public bool DefrostOn { get; private set; } = false;
    public bool EvapFanOn { get; private set; } = false;
    public bool LightOn { get; private set; } = false;

    public int TargetTemp { get; set; } = 5;
    public int DefrostTemp { get; set; } = 10;
    public int Differential { get; set; } = 5;
    public int DelayTime { get; set; } = 30;
    public int CoolingDuration { get; set; } = 10 * 60;
    public int DefrostDuration { get; set; } = 5 * 60;

    private float _boxTemp = 0;
    private float _evapTemp = 0;
    private bool _defrostStopped = false;

    private DateTime startTime = DateTime.Now;
    private DateTime lastCompressorOff = DateTime.Now;
    private int timeElapsed { get { return (int)(DateTime.Now - startTime).TotalSeconds; } }

    void TurnCompressorOn()
    {
        if (lastCompressorOff.AddSeconds(DelayTime) > DateTime.Now)
        {
            return;
        }
        CompressorOn = true;
    }

    void TurnCompressorOff()
    {
        if (!CompressorOn) { return; } // Keeps lastCompressorOff from being updated if compressor is already off

        CompressorOn = false;
        lastCompressorOff = DateTime.Now;
    }

    void TurnDefrostOn()
    {
        DefrostOn = true;
    }

    void TurnDefrostOff()
    {
        DefrostOn = false;
    }

    void Cool()
    {
        float maxTemp = TargetTemp + Differential;

        if (_boxTemp >= maxTemp)
        {
            TurnCompressorOn();
        }
        else if (_boxTemp <= TargetTemp)
        {
            TurnCompressorOff();
        }
    }

    void Defrost()
    {
        if (_defrostStopped) { return; }

        TurnCompressorOff();
        if (_evapTemp < DefrostTemp)
        {
            TurnDefrostOn();
        }
        else
        {
            TurnDefrostOff();
            _defrostStopped = true;
        }
    }

    public void Loop(float boxTemp, float evapTemp)
    {
        _boxTemp = boxTemp;
        _evapTemp = evapTemp;

        // If cooling cycle, cool
        if (timeElapsed < CoolingDuration)
        {
            Cool();
        }
        else if (timeElapsed < CoolingDuration + DefrostDuration)
        {
            Defrost();
        }
        else
        {
            startTime = DateTime.Now;
            DefrostOn = false;
            _defrostStopped = false;
        }
    }
}