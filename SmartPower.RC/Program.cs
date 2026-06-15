using System.Net.Sockets;
using System.Diagnostics;

namespace SmartPower.RC
{
    public class Program
    {
        private static readonly HttpClient _client = CreateUnixSocketClient();

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Forward status check
            app.MapGet("/status", async () =>
            {
                return await ForwardAsync("http://localhost/status");
            });

            // Update WiFi AP password
            app.MapGet("/wifi-password/{password}", async (string password) =>
            {
                if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                { return Results.BadRequest("Password must be at least 8 characters"); }

                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[\x20-\x7E]+$") ||
                    password.IndexOfAny(new[] { '\'', '"', '`', '\\', '$', ';', '&', '|', '<', '>' }) >= 0)
                { return Results.BadRequest("Password contains invalid characters"); }

                try
                {
                    var result = RunCommand("nmcli", $"connection modify XCooling wifi-sec.psk \"{password}\"");
                    if (result.ExitCode != 0)
                        return Results.Problem($"nmcli modify failed: {result.Stderr}");

                    result = RunCommand("nmcli", "connection up XCooling");
                    if (result.ExitCode != 0)
                        return Results.Problem($"nmcli up failed: {result.Stderr}");

                    return Results.Ok("Password updated");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error: {ex.Message}");
                }
            });

            app.MapGet("/wifi-ssid/{ssid}", async (string ssid) =>
            {
                if (string.IsNullOrWhiteSpace(ssid))
                    return Results.BadRequest("SSID cannot be empty");

                if (ssid.Length > 32)
                    return Results.BadRequest("SSID must be 32 characters or less");

                if (ssid.IndexOfAny(new[] { '\'', '"', '`', '\\', '$', ';', '&', '|', '<', '>' }) >= 0)
                    return Results.BadRequest("SSID contains invalid characters");

                try
                {
                    var result = RunCommand("nmcli", $"connection modify XCooling 802-11-wireless.ssid \"{ssid}\"");
                    if (result.ExitCode != 0)
                        return Results.Problem($"nmcli modify failed: {result.Stderr}");

                    result = RunCommand("nmcli", "connection up XCooling");
                    if (result.ExitCode != 0)
                        return Results.Problem($"nmcli up failed: {result.Stderr}");

                    return Results.Ok("SSID updated");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error: {ex.Message}");
                }
            });

            // Forward commands without value
            app.MapGet("/get/{cmd}", async (string cmd) =>
            {
                return await ForwardAsync($"http://localhost/get/{cmd}");
            });

            // Forward commands with value
            app.MapGet("/get/{cmd}/{val}", async (string cmd, string val) =>
            {
                return await ForwardAsync($"http://localhost/get/{cmd}/{val}");
            });            

            app.Run("http://0.0.0.0:5000");
        }

        private static async Task<string> ForwardAsync(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private static HttpClient CreateUnixSocketClient()
        {
            var handler = new SocketsHttpHandler
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                    var endpoint = new UnixDomainSocketEndPoint("/tmp/smartpower.serial.sock");
                    await socket.ConnectAsync(endpoint, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            };

            return new HttpClient(handler);
        }

        private static (int ExitCode, string Stderr) RunCommand(string cmd, string args)
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi)!;
            proc.WaitForExit();
            return (proc.ExitCode, proc.StandardError.ReadToEnd());
        }
    }
}