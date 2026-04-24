using RJCP.IO.Ports;
using System.IO.Ports;

namespace SmartPower.Test.Components.Pages
{
    public partial class Home : IDisposable
    {
        bool isConnected = false;
        SerialPortStream port = new SerialPortStream("", 9600)
        {
            NewLine = "\n",
            ReadTimeout = 1000,
            //DtrEnable = false,
            //RtsEnable = false
        };
        double? temp1 { get; set; }
        double? temp2 { get; set; }
        double? current1 { get; set; }
        double? current2 { get; set; }

        record struct Relay(string Name, int Index, Boolean IsOn);
        Relay[] Relays = new Relay[]
        {
            new Relay("Compressor", 0, false),
            new Relay("Fan", 1, false),
            new Relay("Defrost", 2, false),
            new Relay("Light", 3, false)
        };

        bool isLoading { get; set; }
        string? errorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await RefreshParameters();
        }

        SerialPortStream GetPortStream(string portName)
        {
            if (!port.IsOpen)
            {
                if (portName == null) { return null; }
                port.PortName = portName;
                port.Open();
            }

            return port;
        }

        void ClosePortStream()
        {
            try
            {
                port.Close();
            }
            catch { }
            finally { port.Dispose(); }
        }

        async Task RefreshParameters()
        {
            isLoading = true;
            errorMessage = null;

            var ports = SerialPort.GetPortNames();

            foreach (var p in ports)
            {
                try
                {
                    var port = GetPortStream(p);

                    //temp1 = PiService.GetTemp1();
                    //temp2 = PiService.GetTemp2();
                    //current1 = PiService.GetRms1();
                    //current2 = PiService.GetRms2();

                    isConnected = true;
                    break;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
                finally
                {
                    isLoading = false;
                }
            }

            StateHasChanged();
        }

        string RelayButtonClass(bool onOff)
        {
            return "btn btn-lg btn-primary btn-" + (onOff ? "danger" : "success");
        }

        string RelayButtonText(bool onOff, string name)
        {
            return onOff ? $"Turn Off {name}" : $"Turn On {name}";
        }

        async Task ToggleRelay(int index)
        {
            isLoading = !isLoading;
            errorMessage = null;
            try
            {
                var service = new PiService();

                service.TurnRelayOnOff(index, !Relays[index].IsOn);
                Relays[index].IsOn = !Relays[index].IsOn;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isLoading = false;
            }
            StateHasChanged();
        }

        public void Dispose()
        {
            ClosePortStream();
        }
    }
}