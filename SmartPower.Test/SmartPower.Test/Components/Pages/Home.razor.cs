using RJCP.IO.Ports;

namespace SmartPower.Test.Components.Pages
{
    public partial class Home
    {
        async Task DoSomething()
        {
            using var port = new SerialPortStream("COM3", 9600)
            {
                NewLine = "\n"
            };

            port.Open();

            var transport = new SerialTransport(port);
            var service = new ArduinoService(transport);

            // use freely
            Console.WriteLine(service.GetTemp1());
            Console.WriteLine(service.GetRmsA1());
        }
    }
}