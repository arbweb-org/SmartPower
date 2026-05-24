using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Sockets;

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
    }
}