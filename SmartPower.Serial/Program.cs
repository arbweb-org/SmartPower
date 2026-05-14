namespace SmartPower.Serial
{
    public class Program
    {
        private static readonly ArduinoService _arduino = new ArduinoService();

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host
            .CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    string socketPath = "/tmp/smartpower.serial.sock";
                    options.ListenUnixSocket(socketPath);
                });

                // Inline the middleware configuration instead of using a Startup class
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/status", async context =>
                        {
                            await context.Response.WriteAsync(_arduino.IsConnected ? "Connected" : "Disconnected");
                        });

                        endpoints.MapGet("/get/{cmd}/{val?}", async context =>
                        {
                            // Access parameters from the route path
                            string? cmd = context.Request.RouteValues["cmd"]?.ToString();
                            string? val = context.Request.RouteValues["val"]?.ToString();

                            // Process and return
                            string result = await ProcessRequestAsync(cmd, val);
                            await context.Response.WriteAsync(result);
                        });
                    });
                });
            });

            return hostBuilder;
        }

        /// <summary>
        /// Orchestrates the serial communication within a thread-safe lock.
        /// </summary>
        private static async Task<string> ProcessRequestAsync(string? cmd, string? val)
        {
            if (string.IsNullOrEmpty(cmd)) return "Error: No Command";

            try
            {
                return cmd.ToUpper() switch
                {
                    "X" => _arduino.CheckConnection(),
                    "T1" => _arduino.GetTemperature(1),
                    "T2" => _arduino.GetTemperature(2),
                    "W" => _arduino.GetRmsData(),
                    "P" => _arduino.GetParameters(),
                    "C" => _arduino.GetCalibration(),

                    // Relay controls
                    "ON" => _arduino.SetRelay(int.Parse(val ?? string.Empty), true),
                    "OFF" => _arduino.SetRelay(int.Parse(val ?? string.Empty), false),

                    // Updates parameters
                    "D" => _arduino.UpdateTargetTemp(int.Parse(val ?? string.Empty)),
                    "E" => _arduino.UpdateDefrostTemp(int.Parse(val ?? string.Empty)),
                    "F" => _arduino.UpdateDifferential(int.Parse(val ?? string.Empty)),
                    "G" => _arduino.UpdateDelayTime(int.Parse(val ?? string.Empty)),
                    "H" => _arduino.UpdateCoolingDuration(int.Parse(val ?? string.Empty)),
                    "I" => _arduino.UpdateDefrostDuration(int.Parse(val ?? string.Empty)),

                    // Update calibration
                    "A" => _arduino.UpdateCurrentCal(float.Parse(val ?? string.Empty)),
                    "V" => _arduino.UpdateVoltageCal(float.Parse(val ?? string.Empty)),            

                    _ => "Error: Unknown Command"
                };
            }
            catch
            {
                return "Error";
            }
        }
    }
}