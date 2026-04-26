namespace SmartPower.Publisher
{
    public class Worker : BackgroundService
    {
        private readonly ArduinoService _arduino;
        private readonly Refrigerator _fridge = new Refrigerator();

        public Worker(ArduinoService arduino)
        {
            _arduino = arduino;
        }

        // Runs every second
        async Task Control()
        {
            float temp1 = _arduino.GetTemp1();
            float temp2 = _arduino.GetTemp2();
            float rms1 = _arduino.GetRms1();
            float rms2 = _arduino.GetRms2();

            _fridge.Loop(temp1, temp2);

            _arduino.TurnRelayOnOff(0, _fridge.CompressorOn);
            _arduino.TurnRelayOnOff(1, _fridge.EvapFanOn);
            _arduino.TurnRelayOnOff(2, _fridge.DefrostOn);
            _arduino.TurnRelayOnOff(3, _fridge.LightOn);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Control();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}