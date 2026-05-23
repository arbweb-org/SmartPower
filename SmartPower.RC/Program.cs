using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SmartPower.RC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/func1", (string input) =>
            {
                // Your logic here
                var result = $"Received: {input}";

                return Results.Ok(result);
            });

            app.MapGet("/func2", (string input) =>
            {
                // Your logic here
                var result = $"Received: {input}";

                return Results.Ok(result);
            });

            app.Run("http://0.0.0.0:5000");
        }
    }
}